using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace PLimit.Utils;

/// <summary>Retains a native handle so every query, change and restoration targets the same process instance.</summary>
internal sealed class WindowsAutomationProcess : IAutomationProcess, IDisposable
{
    private readonly SafeProcessHandle _handle;
    private long _lastCpu;
    private TimeSpan? _lastSample;
    public ProcessIdentity Identity { get; }
    public string Name { get; }
    public double? CpuPercent { get; private set; }
    public bool IsAlive => WaitForSingleObject(_handle, 0) == 258; // WAIT_TIMEOUT; exited handles are signaled.
    internal static bool SupportsAffinity => GetActiveProcessorGroupCount() == 1;

    private WindowsAutomationProcess(SafeProcessHandle handle, string name, int pid)
    {
        _handle = handle;
        Name = name;
        Check(GetProcessTimes(handle, out long created, out _, out _, out _));
        Identity = new(pid, created);
    }

    public static WindowsAutomationProcess? TryOpen(Process process, int sessionId)
    {
        SafeProcessHandle? handle = null;
        try
        {
            if (process.Id <= 4 || process.Id == Environment.ProcessId || process.SessionId != sessionId)
                return null;
            string name = process.ProcessName;
            if (ProtectedNames.Contains(name)) return null;
            long created = process.StartTime.ToUniversalTime().ToFileTimeUtc();
            handle = OpenProcess(0x101200, false, process.Id); // SYNCHRONIZE | SET_INFORMATION | QUERY_LIMITED_INFORMATION
            if (handle.IsInvalid || !IsProcessCritical(handle, out bool critical) || critical)
                return null;
            var result = new WindowsAutomationProcess(handle, name, process.Id);
            if (result.Identity.StartTime != created) return null;
            handle = null;
            return result;
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or ArgumentException or NotSupportedException)
        {
            return null;
        }
        finally { handle?.Dispose(); }
    }

    private static readonly HashSet<string> ProtectedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "System", "Registry", "Memory Compression", "Idle", "csrss", "smss", "wininit", "winlogon",
        "lsass", "services", "svchost", "dwm", "audiodg", "fontdrvhost", "MsMpEng"
    };

    public void Sample(TimeSpan now)
    {
        CpuPercent = null;
        Check(GetProcessTimes(_handle, out _, out _, out long kernel, out long user));
        long cpu = kernel + user;
        double elapsed = _lastSample.HasValue ? (now - _lastSample.Value).TotalSeconds : 0;
        CpuPercent = elapsed > 0 && cpu >= _lastCpu
            ? Math.Clamp((cpu - _lastCpu) / 10000000.0 / elapsed / Environment.ProcessorCount * 100, 0, 100)
            : null;
        _lastCpu = cpu;
        _lastSample = now;
    }

    public ProcessPriorityClass Priority
    {
        get
        {
            uint priority = GetPriorityClass(_handle);
            Check(priority != 0);
            return (ProcessPriorityClass)priority;
        }
        set => Check(SetPriorityClass(_handle, (uint)value));
    }

    public ulong Affinity
    {
        get
        {
            if (!SupportsAffinity) throw new NotSupportedException("CPU affinity automation is unavailable across multiple processor groups.");
            Check(GetProcessAffinityMask(_handle, out nuint mask, out _));
            return mask;
        }
        set
        {
            if (!SupportsAffinity) throw new NotSupportedException("CPU affinity automation is unavailable across multiple processor groups.");
            Check(GetProcessAffinityMask(_handle, out _, out nuint available));
            if (value == 0 || (value & ~(ulong)available) != 0)
                throw new InvalidDataException("Saved CPU affinity includes unavailable processors.");
            Check(SetProcessAffinityMask(_handle, checked((nuint)value)));
        }
    }

    public void ApplySaved(ProcessData settings, bool applyPriority, bool applyAffinity, Action<string> report)
    {
        void Apply(string label, Action action)
        {
            try { action(); }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or ArgumentException
                or NotSupportedException or InvalidDataException or OverflowException)
            { report($"Saved {label}: {ex.Message}"); }
        }

        // Efficiency changes priority; apply it first so an explicit CPU priority wins consistently.
        bool hasPriority = Enum.TryParse<ProcessPriorityClass>(settings.Property, out var priority) && Enum.IsDefined(priority);
        if (SettingState.TryParse(settings.Efficiency, out bool efficiency))
            Apply("efficiency mode", () =>
            {
                var helper = new EfficiencyModeHelper();
                bool changePriority = applyPriority && !hasPriority;
                if (helper.IsEcoQosEnabled(_handle.DangerousGetHandle()) != efficiency
                    || (efficiency && changePriority && Priority != ProcessPriorityClass.Idle))
                    helper.SetEfficiencyMode(_handle.DangerousGetHandle(), efficiency, changePriority);
            });
        if (applyPriority && hasPriority)
            Apply("priority", () => { if (Priority != priority) Priority = priority; });
        if (applyAffinity && long.TryParse(settings.Affinity, out long affinity) && affinity != 0)
            Apply("affinity", () => { if (Affinity != unchecked((ulong)affinity)) Affinity = unchecked((ulong)affinity); });

        // Legacy Wdptb wins when old files contain conflicting values, matching the previous load order.
        if (SettingState.TryParse(settings.Wdptb, out bool boost) || SettingState.TryParse(settings.Boosted, out boost))
            Apply("dynamic boost", () =>
            {
                Check(GetProcessPriorityBoost(_handle, out bool disabled));
                if (disabled == boost) Check(SetProcessPriorityBoost(_handle, !boost));
            });
        if (Enum.TryParse<ProcessesManage.IO_PRIORITY_HINT>(settings.IOProperty, out var io) && Enum.IsDefined(io))
            Apply("I/O priority", () =>
            {
                int value = (int)io;
                int status = NtQueryInformationProcess(_handle, 33, out int current, sizeof(int), out _);
                if (status == 0 && current == value) return;
                if (io >= ProcessesManage.IO_PRIORITY_HINT.High)
                    ProcessesManage.EnablePrivilege("SeIncreaseBasePriorityPrivilege");
                status = NtSetInformationProcess(_handle, 33, ref value, sizeof(int));
                if (status != 0) throw new InvalidOperationException($"Windows rejected I/O priority (NTSTATUS 0x{status:X8}).");
            });
    }

    internal static bool SetIoPriority(int pid, ProcessesManage.IO_PRIORITY_HINT priority)
    {
        using var handle = OpenProcess(0x0200, false, pid);
        if (handle.IsInvalid) return false;
        int value = (int)priority;
        return NtSetInformationProcess(handle, 33, ref value, sizeof(int)) == 0;
    }

    private static void Check(bool success)
    {
        if (!success) throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    public void Dispose() => _handle.Dispose();

    [DllImport("kernel32.dll", SetLastError = true)] private static extern SafeProcessHandle OpenProcess(uint access, bool inherit, int pid);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GetProcessTimes(SafeProcessHandle process, out long created, out long exited, out long kernel, out long user);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern uint WaitForSingleObject(SafeProcessHandle process, uint milliseconds);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool IsProcessCritical(SafeProcessHandle process, out bool critical);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern uint GetPriorityClass(SafeProcessHandle process);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool SetPriorityClass(SafeProcessHandle process, uint priority);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GetProcessAffinityMask(SafeProcessHandle process, out nuint mask, out nuint available);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool SetProcessAffinityMask(SafeProcessHandle process, nuint mask);
    [DllImport("kernel32.dll")] private static extern ushort GetActiveProcessorGroupCount();
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GetProcessPriorityBoost(SafeProcessHandle process, out bool disabled);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool SetProcessPriorityBoost(SafeProcessHandle process, bool disabled);
    [DllImport("ntdll.dll")] private static extern int NtSetInformationProcess(SafeProcessHandle process, int infoClass, ref int value, int size);
    [DllImport("ntdll.dll")] private static extern int NtQueryInformationProcess(SafeProcessHandle process, int infoClass, out int value, int size, out int returned);
}
