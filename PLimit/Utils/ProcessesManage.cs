
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;


namespace PLimit.Utils
{
    internal class ProcessesManage
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool GetTokenInformation(IntPtr TokenHandle, int TokenInformationClass, IntPtr TokenInformation, int TokenInformationLength, out int ReturnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetProcessPriorityBoost(
    IntPtr hProcess,
    out bool pDisablePriorityBoost);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetProcessPriorityBoost(IntPtr hProcess, bool DisablePriorityBoost);

        const int TOKEN_QUERY = 0x0008;
        const int TokenUser = 1;

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
        const uint THREAD_QUERY_LIMITED_INFORMATION = 0x0800; // safer than 0x0040 for this case

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

        /// <summary>
        /// Set IO priority for all threads in a process.
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="priority"></param>
        public void SetIoPriorityAllThreads(int processId, IO_PRIORITY_HINT priority)
        {
            var getProcess = Process.GetProcessById(processId);
            foreach (ProcessThread thread in getProcess.Threads)
                SetIoPriority(thread, priority);
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
        /// Add running processes to list box.
        /// </summary>
        /// <param name="listBox"></param>
        public void GetProcesses(ref DoubleBufferedListView listView)
        {
            listView.BeginUpdate();
            try
            {
                listView.Items.Clear();

                var processes = Process.GetProcesses();
                foreach (var process in processes)
                {
                    var user = GetProcessUser(process);
                    if (user != Environment.UserName) continue;

                    IntPtr handle = IntPtr.Zero;
                    try
                    {
                        handle = process.Handle;

                        bool disabled;
                        GetProcessPriorityBoost(handle, out disabled);

                        var cpus = CountBits(process.ProcessorAffinity.ToInt64());

                        var io = "----";
                        foreach (ProcessThread thread in process.Threads)
                            io = GetIoPriority(thread).ToString();
                        var efficency = new EfficiencyModeHelper();
                        var efficiencyMode = "Disable";

                        //Workaround for efficiency mode, since there is no official way to check if it's enabled or not, we will check if the priority class is set to Idle or BelowNormal, which are the only two options that enable efficiency mode
                        efficiencyMode = (process.PriorityClass.ToString()=="Idle" || process.PriorityClass.ToString()=="BelowNormal") ? "Enabled" : "Disabled";
                        // var efficiencyMode = efficency.IsEfficiencyModeEnabled(process.Id) ? "Enabled" : "Disabled";

                        var item = new ListViewItem(new[]
                        {
                    process.ProcessName,
                    process.Id.ToString(),
                    process.PriorityClass.ToString(),
                    cpus.ToString(),
                    io,
                    disabled.ToString(),
                    efficiencyMode
                });

                        listView.Items.Add(item);
                    }
                    catch
                    {
                        // process may exit / access denied etc. skip it
                    }
                }
            }
            finally
            {
                listView.EndUpdate();
            }
        }


        /// <summary>
        /// Set priority class for a process.
        /// </summary>
        /// <param name="priorityClass"></param>
        /// <param name="processId"></param>
        public void SetPriorityClass(ProcessPriorityClass priorityClass, int processId)
        {
            try
            {
                var getProcess = Process.GetProcessById(processId);
                getProcess.PriorityClass = priorityClass;
            }
            catch
            {
                MessageBox.Show("Failed to set priority class! Try running the application as administrator.", "Process Limitator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Set priority boost for a process.
        /// </summary>
        /// <param name="isEnabled"></param>
        /// <param name="processId"></param>
        public void SetBoost(bool isEnabled, int processId)
        {
            try
            {
                var getProcess = Process.GetProcessById(processId);
                IntPtr handle = getProcess.Handle;
                SetProcessPriorityBoost(handle, isEnabled);
            }
            catch
            {
                MessageBox.Show("Failed to set priority boost! Try running the application as administrator.", "Process Limitator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Search process in process list.
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="searchString"></param>
        public void SearchProcess(ref DoubleBufferedListView listView, string searchString, bool isMessage = true)
        {
            ListViewItem? foundItem =
                listView.FindItemWithText(searchString, true, 0, true);
            if (foundItem != null)
                listView.TopItem = foundItem;
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

            try
            {
                if (!OpenProcessToken(process.Handle, TOKEN_QUERY, out tokenHandle))
                    return "-";

                int tokenInfoLength = 0;
                GetTokenInformation(tokenHandle, TokenUser, IntPtr.Zero, 0, out tokenInfoLength);
                IntPtr tokenInfo = Marshal.AllocHGlobal(tokenInfoLength);

                if (!GetTokenInformation(tokenHandle, TokenUser, tokenInfo, tokenInfoLength, out _))
                    return "-";

                var sid = Marshal.ReadIntPtr(tokenInfo);
                var account = new SecurityIdentifier(sid);
                string fullName = account.Translate(typeof(NTAccount)).ToString();

                Marshal.FreeHGlobal(tokenInfo);

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
        {
            int count = 0;
            while (value != 0)
            {
                count += (int)(value & 1);
                value >>= 1;
            }
            return count;
        }
    }
}
