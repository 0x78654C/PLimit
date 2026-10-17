using System.Diagnostics;
using System.Runtime.InteropServices;
using PLimit.Json;

namespace PLimit.Utils;

internal sealed class ProcessAutomationService : IDisposable
{
    private readonly CancellationTokenSource _stop = new();
    private readonly Dictionary<int, WindowsAutomationProcess> _processes = new();
    private readonly Queue<string> _log = new();
    private readonly ProcessAutomationEngine _engine;
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly int _sessionId;
    private readonly Func<Process, bool>? _includeProcess;
    private Task? _worker;
    private string? _lastSettingsError;
    private long _previousIdle, _previousTotal;
    private bool _hasSystemSample;
    private AutomationSettings _settings = new();
    private IReadOnlyDictionary<string, ProcessData> _saved = new Dictionary<string, ProcessData>();
    public volatile bool EnforceSavedSettings;
    private volatile string _status = "Automation idle";
    public string Status => _status;

    public AutomationSettings GetSettingsSnapshot() =>
        System.Text.Json.JsonSerializer.Deserialize<AutomationSettings>(System.Text.Json.JsonSerializer.Serialize(_settings))!;

    public ProcessAutomationService(Func<Process, bool>? includeProcess = null)
    {
        _includeProcess = includeProcess;
        using var current = Process.GetCurrentProcess();
        _sessionId = current.SessionId;
        _engine = new ProcessAutomationEngine(Log);
    }

    public void Start() => _worker ??= Task.Run(RunAsync);

    public string[] GetLog()
    {
        lock (_log) return _log.Reverse().ToArray();
    }

    private void Log(string message)
    {
        lock (_log)
        {
            _log.Enqueue($"{DateTime.Now:HH:mm:ss}  {message}");
            while (_log.Count > 250) _log.Dequeue();
        }
    }

    private void ReadConfiguration()
    {
        var errors = new List<string>();
        try
        {
            _settings = AutomationSettings.Load();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidDataException)
        {
            errors.Add($"Automation: {ex.Message}");
        }
        try
        {
            var saved = new Dictionary<string, ProcessData>(StringComparer.OrdinalIgnoreCase);
            if (EnforceSavedSettings && File.Exists(GlobalVars.LogFilePath))
                foreach (var item in JsonManage.ReadJsonFromFile<ProcessData[]>(GlobalVars.LogFilePath))
                    if (item != null && !string.IsNullOrWhiteSpace(item.ProcessName))
                        saved[AutomationSettings.NormalizeName(item.ProcessName)] = item;
            _saved = saved;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidDataException)
        {
            errors.Add($"Saved process settings: {ex.Message}");
        }
        string? error = errors.Count > 0 ? string.Join(" ", errors) : null;
        if (error != null && _lastSettingsError != error)
            Log($"Settings could not be read; keeping last valid rules. {error}");
        _lastSettingsError = error;
        if (!EnforceSavedSettings) _saved = new Dictionary<string, ProcessData>();
    }

    private async Task RunAsync()
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        try
        {
            do
            {
                try
                {
                    ReadConfiguration();
                    CaptureProcesses();
                    GetWindowThreadProcessId(GetForegroundWindow(), out int foreground);
                    var processes = _processes.Values.Cast<IAutomationProcess>().ToArray();
                    _engine.Tick(processes, foreground, SampleSystemCpu(), _clock.Elapsed, _settings, _saved);
                    _status = $"Boost: {_engine.ActiveBoosts} | Balanced: {_engine.ActiveBalances} | Limited: {_engine.ActiveLimits}"
                        + (EnforceSavedSettings ? " | Saved rules active" : "")
                        + (_lastSettingsError != null ? " | Settings error — see activity" : "");
                }
                catch (Exception ex) { Log($"Automation cycle failed: {ex.Message}"); }
            }
            while (await timer.WaitForNextTickAsync(_stop.Token).ConfigureAwait(false));
        }
        catch (OperationCanceledException) when (_stop.IsCancellationRequested) { }
        finally
        {
            // Existing handles remain valid even if the foreground changes during shutdown.
            var live = _processes.Values.Where(p => p.IsAlive).Cast<IAutomationProcess>().ToArray();
            _engine.RestoreAll(live);
            foreach (var process in _processes.Values) process.Dispose();
            _processes.Clear();
        }
    }

    private void CaptureProcesses()
    {
        foreach (int pid in _processes.Where(p => !p.Value.IsAlive).Select(p => p.Key).ToArray())
        {
            _processes[pid].Dispose();
            _processes.Remove(pid);
        }
        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                if (_processes.ContainsKey(process.Id)) continue;
                if (_includeProcess != null && !_includeProcess(process)) continue;
                var candidate = WindowsAutomationProcess.TryOpen(process, _sessionId);
                if (candidate != null) _processes[process.Id] = candidate;
            }
        }
        foreach (var process in _processes.Values)
        {
            try { process.Sample(_clock.Elapsed); }
            catch (System.ComponentModel.Win32Exception) { /* Retry on next sample, retain restoration state. */ }
        }
    }

    private double? SampleSystemCpu()
    {
        if (!GetSystemTimes(out long idle, out long kernel, out long user)) return null;
        long total = kernel + user;
        long elapsed = total - _previousTotal;
        double? result = _hasSystemSample && elapsed > 0
            ? Math.Clamp(100.0 * (elapsed - (idle - _previousIdle)) / elapsed, 0, 100) : null;
        _hasSystemSample = true;
        _previousIdle = idle;
        _previousTotal = total;
        return result;
    }

    public async Task StopAsync()
    {
        _stop.Cancel();
        if (_worker != null) await _worker.ConfigureAwait(false);
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
        _stop.Dispose();
    }

    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out int pid);
    [DllImport("kernel32.dll")] private static extern bool GetSystemTimes(out long idle, out long kernel, out long user);
}
