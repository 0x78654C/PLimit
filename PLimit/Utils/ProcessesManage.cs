using System.Diagnostics;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Principal;


namespace PLimit.Utils
{
    internal class ProcessesManage
    {
        private static readonly ConcurrentDictionary<string, string> UserNameCache =
            new(StringComparer.OrdinalIgnoreCase);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool GetTokenInformation(IntPtr TokenHandle, int TokenInformationClass, IntPtr TokenInformation, int TokenInformationLength, out int ReturnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetProcessPriorityBoost(
    IntPtr hProcess,
    out bool pDisablePriorityBoost);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetProcessPriorityBoost(IntPtr hProcess, bool DisablePriorityBoost);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetThreadPriorityBoost(IntPtr hThread, out bool pDisablePriorityBoost);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetThreadPriorityBoost(IntPtr hThread, bool bDisablePriorityBoost);

        const int TOKEN_QUERY = 0x0008;
        const int TokenUser = 1;
        const uint PROCESS_SET_INFORMATION = 0x0200;

        // native NTSTATUS version
        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationThread(
            IntPtr ThreadHandle,
            int ThreadInformationClass,
            out int ThreadInformation,
            int ThreadInformationLength,
            out int ReturnLength);

        [DllImport("ntdll.dll")]
        static extern int NtSetInformationThread(
            IntPtr ThreadHandle,
            int ThreadInformationClass,
            ref IO_PRIORITY_HINT ThreadInformation,
            int ThreadInformationLength
        );

        const int ThreadIoPriority = 22; // native THREADINFOCLASS value
        const uint THREAD_SET_INFORMATION = 0x0020;
        const int TOKEN_ADJUST_PRIVILEGES = 0x0020;
        const uint SE_PRIVILEGE_ENABLED = 0x00000002;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool LookupPrivilegeValue(string? lpSystemName, string lpName, out LUID lpLuid);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool AdjustTokenPrivileges(
            IntPtr TokenHandle,
            bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState,
            int BufferLength,
            IntPtr PreviousState,
            IntPtr ReturnLength);

        [StructLayout(LayoutKind.Sequential)]
        struct LUID { public uint LowPart; public int HighPart; }

        [StructLayout(LayoutKind.Sequential)]
        struct LUID_AND_ATTRIBUTES { public LUID Luid; public uint Attributes; }

        [StructLayout(LayoutKind.Sequential)]
        struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
        }

        public enum IO_PRIORITY_HINT : int
        {
            VeryLow = 0,
            Low = 1,
            Normal = 2,
            High = 3,
            Critical = 4
        }

        public enum THREAD_INFORMATION_CLASS
        {
            ThreadMemoryPriority = 0,
            ThreadAbsoluteCpuPriority = 1,
            ThreadDynamicCodePolicy = 2,
            ThreadPowerThrottling = 3,
            ThreadIoPriority = 21
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetThreadInformation(
            IntPtr hThread,
            THREAD_INFORMATION_CLASS ThreadInformationClass,
            out IO_PRIORITY_HINT ThreadInformation,
            int ThreadInformationSize
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenThread(uint desiredAccess, bool inheritHandle, int threadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetThreadInformation(
    IntPtr hThread,
    THREAD_INFORMATION_CLASS infoClass,
    ref IO_PRIORITY_HINT info,
    int infoSize);

        /// <summary>
        /// Ctor
        /// </summary>
        public ProcessesManage() { }

        /// <summary>
        /// Set IO priority of a thread.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        private static bool SetIoPriority(ProcessThread t, IO_PRIORITY_HINT priority)
        {
            // High and Critical IO priority require SeIncreaseBasePriorityPrivilege
            if (priority >= IO_PRIORITY_HINT.High)
                EnablePrivilege("SeIncreaseBasePriorityPrivilege");

            IntPtr hThread = OpenThread(THREAD_SET_INFORMATION, false, t.Id);
            if (hThread == IntPtr.Zero)
                return false;

            try
            {
                int status = NtSetInformationThread(
                    hThread,
                    ThreadIoPriority,
                    ref priority,
                    sizeof(IO_PRIORITY_HINT));

                return status == 0;
            }
            finally
            {
                CloseHandle(hThread);
            }
        }

        private static bool EnablePrivilege(string privilegeName)
        {
            if (!OpenProcessToken(Process.GetCurrentProcess().Handle,
                (uint)(TOKEN_QUERY | TOKEN_ADJUST_PRIVILEGES), out IntPtr hToken))
                return false;

            try
            {
                if (!LookupPrivilegeValue(null, privilegeName, out LUID luid))
                    return false;

                var tp = new TOKEN_PRIVILEGES
                {
                    PrivilegeCount = 1,
                    Privileges = new LUID_AND_ATTRIBUTES { Luid = luid, Attributes = SE_PRIVILEGE_ENABLED }
                };

                return AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero)
                       && Marshal.GetLastWin32Error() == 0;
            }
            finally
            {
                CloseHandle(hToken);
            }
        }

        /// <summary>
        /// Set IO priority for all threads in a process.
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="priority"></param>
        public bool SetIoPriorityAllThreads(int processId, IO_PRIORITY_HINT priority)
        {
            try
            {
                using var getProcess = Process.GetProcessById(processId);
                bool updatedAnyThread = false;
                foreach (ProcessThread thread in getProcess.Threads)
                    updatedAnyThread |= SetIoPriority(thread, priority);
                return updatedAnyThread;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get IO priority of a thread.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IO_PRIORITY_HINT? GetIoPriority(ProcessThread t)
        {
            IntPtr hThread = OpenThread(0x0800, false, t.Id); // THREAD_QUERY_LIMITED_INFORMATION
            if (hThread == IntPtr.Zero)
                return null;

            try
            {
                int returnLength;
                int rawValue;

                int status = NtQueryInformationThread(
                    hThread,
                    22, // ThreadIoPriority
                    out rawValue,
                    sizeof(int),
                    out returnLength);

                if (status != 0)
                    return null;

                return Enum.IsDefined(typeof(IO_PRIORITY_HINT), rawValue)
                    ? (IO_PRIORITY_HINT)rawValue
                    : null;
            }
            finally
            {
                CloseHandle(hThread);
            }
        }
        /// <summary>
        /// Captures process information without touching UI controls, allowing callers
        /// to do the expensive work on a background thread.
        /// </summary>
        public IReadOnlyList<ProcessSnapshot> CaptureProcesses(CancellationToken cancellationToken = default)
        {
            var snapshots = new List<ProcessSnapshot>();
            var storedProcessNames = LoadStoredProcessNames();
            var efficiency = new EfficiencyModeHelper();
            var processes = Process.GetProcesses();

            try
            {
                foreach (var process in processes)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    string processName;
                    int processId;
                    try
                    {
                        processName = process.ProcessName;
                        processId = process.Id;
                    }
                    catch
                    {
                        continue;
                    }

                    string priority = "Unknown";
                    int? cpuCount = null;
                    string boost = "Unknown";
                    string efficiencyMode = "Unknown";
                    string user = "-";

                    try { priority = process.PriorityClass.ToString(); } catch { }
                    try { cpuCount = CountBits(process.ProcessorAffinity.ToInt64()); } catch { }

                    try
                    {
                        IntPtr handle = process.Handle;
                        if (GetProcessPriorityBoost(handle, out bool boostDisabled))
                            boost = boostDisabled ? "Disabled" : "Enabled";

                        try
                        {
                            efficiencyMode = efficiency.IsEfficiencyModeEnabled(handle)
                                ? "Enabled"
                                : "Disabled";
                        }
                        catch
                        {
                            efficiencyMode = "Unknown";
                        }

                        user = GetProcessUser(process);
                    }
                    catch
                    {
                        // Some protected processes expose only their name and PID.
                    }

                    var (ioPriority, threadBoost) = GetThreadStatus(process);
                    snapshots.Add(new ProcessSnapshot(
                        processName,
                        processId,
                        priority,
                        cpuCount,
                        ioPriority,
                        boost,
                        efficiencyMode,
                        storedProcessNames.Contains(processName),
                        threadBoost,
                        user));
                }
            }
            finally
            {
                foreach (var process in processes)
                    process.Dispose();
            }

            snapshots.Sort((left, right) =>
                string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));
            return snapshots;
        }

        /// <summary>
        /// Replaces the visible process rows using a previously captured snapshot.
        /// </summary>
        public void DisplayProcesses(DoubleBufferedListView listView, IEnumerable<ProcessSnapshot> snapshots)
        {
            var items = snapshots.Select(snapshot => snapshot.ToListViewItem()).ToArray();

            listView.BeginUpdate();
            try
            {
                listView.Items.Clear();
                listView.Items.AddRange(items);
                listView.Sort();
            }
            finally
            {
                listView.EndUpdate();
            }
        }

        /// <summary>
        /// Synchronous compatibility path for callers that do not own an async UI flow.
        /// </summary>
        public void GetProcesses(ref DoubleBufferedListView listView) =>
            DisplayProcesses(listView, CaptureProcesses());

        private static HashSet<string> LoadStoredProcessNames()
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(GlobalVars.LogFilePath))
                return names;

            try
            {
                var settings = Json.JsonManage.ReadJsonFromFile<ProcessData[]>(GlobalVars.LogFilePath);
                foreach (var setting in settings)
                {
                    if (setting != null && !string.IsNullOrWhiteSpace(setting.ProcessName))
                        names.Add(setting.ProcessName);
                }
            }
            catch
            {
                // A malformed or concurrently replaced settings file should not prevent
                // the process list from loading. It can be retried on the next refresh.
            }

            return names;
        }

        private static (string IoPriority, string ThreadBoost) GetThreadStatus(Process process)
        {
            IO_PRIORITY_HINT? ioPriority = null;
            bool? threadBoost = null;

            try
            {
                foreach (ProcessThread thread in process.Threads)
                {
                    ioPriority ??= GetIoPriority(thread);
                    threadBoost ??= GetThreadBoost(thread);

                    if (ioPriority.HasValue && threadBoost.HasValue)
                        break;
                }
            }
            catch
            {
                // Access to process threads is best-effort.
            }

            return (
                ioPriority?.ToString() ?? "Unknown",
                threadBoost.HasValue ? threadBoost.Value ? "Enabled" : "Disabled" : "Unknown");
        }


        /// <summary>
        /// Set priority class for a process.
        /// </summary>
        /// <param name="priorityClass"></param>
        /// <param name="processId"></param>
        public bool SetPriorityClass(ProcessPriorityClass priorityClass, int processId)
        {
            try
            {
                using var getProcess = Process.GetProcessById(processId);
                getProcess.PriorityClass = priorityClass;
                return true;
            }
            catch
            {
                MessageBox.Show("Failed to set priority class! Try running the application as administrator.", "Process Limitator", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Set priority boost for a process.
        /// </summary>
        /// <param name="isEnabled"></param>
        /// <param name="processId"></param>
        public bool SetBoost(bool isEnabled, int processId)
        {
            IntPtr handle = IntPtr.Zero;
            try
            {
                // SetProcessPriorityBoost needs only PROCESS_SET_INFORMATION.
                // Process.Handle can request broader rights and fail even when this
                // specific operation is allowed for the elevated application.
                handle = OpenProcess(PROCESS_SET_INFORMATION, false, processId);
                if (handle == IntPtr.Zero)
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

                // The Win32 API accepts a "disable" flag, while the application API
                // accepts an "enable" flag.
                if (!SetProcessPriorityBoost(handle, !isEnabled))
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                return true;
            }
            catch
            {
                MessageBox.Show(
                    "Failed to set priority boost. The process may be protected or may no longer be running.",
                    "Process Limitator",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                if (handle != IntPtr.Zero)
                    CloseHandle(handle);
            }
        }

        /// <summary>
        /// Gets the dynamic thread priority boost status for the first accessible thread of a process.
        /// Returns true if boost is enabled, false if disabled, null on error.
        /// </summary>
        /// <param name="processId"></param>
        public bool? GetThreadBoost(int processId)
        {
            try
            {
                using var process = Process.GetProcessById(processId);
                foreach (ProcessThread thread in process.Threads)
                {
                    var boost = GetThreadBoost(thread);
                    if (boost.HasValue)
                        return boost;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private static bool? GetThreadBoost(ProcessThread thread)
        {
            IntPtr hThread = OpenThread(0x0800 /* THREAD_QUERY_LIMITED_INFORMATION */, false, thread.Id);
            if (hThread == IntPtr.Zero)
                return null;

            try
            {
                return GetThreadPriorityBoost(hThread, out bool disabled) ? !disabled : null;
            }
            finally
            {
                CloseHandle(hThread);
            }
        }

        /// <summary>
        /// Enables or disables the dynamic thread priority boost for all threads of a process.
        /// </summary>
        /// <param name="enable"></param>
        /// <param name="processId"></param>
        public bool SetThreadBoost(bool enable, int processId)
        {
            try
            {
                using var process = Process.GetProcessById(processId);
                bool updatedAnyThread = false;
                foreach (ProcessThread thread in process.Threads)
                {
                    IntPtr hThread = OpenThread(THREAD_SET_INFORMATION, false, thread.Id);
                    if (hThread == IntPtr.Zero) continue;
                    try
                    {
                        updatedAnyThread |= SetThreadPriorityBoost(hThread, !enable);
                    }
                    finally
                    {
                        CloseHandle(hThread);
                    }
                }

                if (updatedAnyThread)
                    return true;
            }
            catch
            {
                // The shared error below covers access denied and processes that exited.
            }

            MessageBox.Show("Failed to set thread priority boost! Try running the application as administrator.", "Process Limitator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        /// <summary>
        /// Search process in process list.
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="searchString"></param>
        public void SearchProcess(DoubleBufferedListView listView, string searchString, bool isMessage = true)
        {
            string query = searchString.Trim();
            if (query.Length == 0)
                return;

            ListViewItem? foundItem = listView.Items.Cast<ListViewItem>().FirstOrDefault(item =>
                item.Text.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (item.SubItems.Count > 1 && item.SubItems[1].Text.Contains(query, StringComparison.OrdinalIgnoreCase)));
            if (foundItem != null)
            {
                foundItem.Selected = true;
                foundItem.Focused = true;
                foundItem.EnsureVisible();
            }
            else if (isMessage)
                MessageBox.Show($"Process '{searchString}' was not found. Try refresh the list!", "Process Limitator", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Gets the user of a process.
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private string GetProcessUser(Process process)
        {
            IntPtr tokenHandle = IntPtr.Zero;
            IntPtr tokenInfo = IntPtr.Zero;

            try
            {
                if (!OpenProcessToken(process.Handle, TOKEN_QUERY, out tokenHandle))
                    return "-";

                int tokenInfoLength = 0;
                GetTokenInformation(tokenHandle, TokenUser, IntPtr.Zero, 0, out tokenInfoLength);
                if (tokenInfoLength <= 0)
                    return "-";

                tokenInfo = Marshal.AllocHGlobal(tokenInfoLength);

                if (!GetTokenInformation(tokenHandle, TokenUser, tokenInfo, tokenInfoLength, out _))
                    return "-";

                var sid = Marshal.ReadIntPtr(tokenInfo);
                var account = new SecurityIdentifier(sid);
                string sidValue = account.Value;
                string fullName = UserNameCache.GetOrAdd(sidValue, _ =>
                {
                    try
                    {
                        return account.Translate(typeof(NTAccount)).ToString();
                    }
                    catch
                    {
                        return sidValue;
                    }
                });

                // Strip domain or machine name
                int slashIndex = fullName.IndexOf('\\');
                return slashIndex >= 0 ? fullName[(slashIndex + 1)..] : fullName;
            }
            catch
            {
                return "-";
            }
            finally
            {
                if (tokenInfo != IntPtr.Zero)
                    Marshal.FreeHGlobal(tokenInfo);
                if (tokenHandle != IntPtr.Zero)
                    CloseHandle(tokenHandle);
            }
        }


        /// <summary>
        /// Gets the number of set bits in a long value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int CountBits(long value)
            => BitOperations.PopCount(unchecked((ulong)value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        /// <param name="pid"></param>
        public void KillProcess(Form from, DoubleBufferedListView processesListBox, Label label, TextBox searchBox, string pid = "")
        {
            if (string.IsNullOrEmpty(pid) && processesListBox.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select a process first.", "Process Limiter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string processId = string.IsNullOrEmpty(pid)
                ? processesListBox.SelectedItems[0].SubItems[1].Text
                : pid;

            if (!int.TryParse(processId, out int parsedProcessId) || !IsPidValid(processId))
            {
                MessageBox.Show("Invalid PID. Refresh process list!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string processName = string.IsNullOrEmpty(pid)
                ? processesListBox.SelectedItems[0].SubItems[0].Text
                : processId;
            if (MessageBox.Show(
                    $"Terminate {processName} (PID {processId})?",
                    "Confirm process termination",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            try
            {
                using var getProcess = Process.GetProcessById(parsedProcessId);
                getProcess.Kill();
            }
            catch
            {
                MessageBox.Show("Failed to kill process! Try running the application as administrator.", "Process Limitator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            from.BeginInvoke(new Action(() =>
            {
                var utils = new Utils();
                utils.RefreshProcessList(from, processesListBox, label);
                utils.SearchProcess(searchBox, processesListBox);
            }));
        }

        /// <summary>
        /// Check if the given PID corresponds to a running process.
        /// </summary>
        /// <param name="pid">The process ID to check.</param>
        /// <returns>True if the PID corresponds to a running process, otherwise false.</returns>
        public bool IsPidValid(string pid)
        {
            if (!int.TryParse(pid, out int processId))
                return false;
            try
            {
                using var process = Process.GetProcessById(processId);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
