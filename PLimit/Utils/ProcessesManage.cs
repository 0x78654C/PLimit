
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


        public enum IO_PRIORITY_HINT
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
            // const uint THREAD_SET_INFORMATION = 0x0020;
            // const uint THREAD_QUERY_INFORMATION = 0x0040;
            const uint THREAD_ALL_ACCESS = 0x1F03FF;
            IntPtr hThread = OpenThread(THREAD_ALL_ACCESS, false, t.Id);

            if (hThread == IntPtr.Zero)
                return false;

            bool ok = SetThreadInformation(
                hThread,
                THREAD_INFORMATION_CLASS.ThreadIoPriority,
                ref priority,
                sizeof(int));

            CloseHandle(hThread);
            return ok;
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
        public static IO_PRIORITY_HINT GetIoPriority(ProcessThread t)
        {
            IntPtr hThread = OpenThread(0x0040 /* QUERY_LIMITED_INFORMATION */, false, t.Id);

            if (hThread == IntPtr.Zero)
                return IO_PRIORITY_HINT.Normal;

            IO_PRIORITY_HINT hint;
            GetThreadInformation(hThread,
                                 THREAD_INFORMATION_CLASS.ThreadIoPriority,
                                 out hint,
                                 sizeof(int));

            CloseHandle(hThread);
            return hint;
        }


        /// <summary>
        /// Add running processes to list box.
        /// </summary>
        /// <param name="listBox"></param>
        public void GetProcesses(ref ListView listView)
        {
            listView.Items.Clear();
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                var user = GetProcessUser(process);
                if (user == Environment.UserName)
                {
                    IntPtr handle = process.Handle;
                    bool disabled;
                    GetProcessPriorityBoost(handle, out disabled);
                    var cpus = CountBits(process.ProcessorAffinity.ToInt64());
                    var io = "";
                    foreach (ProcessThread thread in process.Threads)
                        io = GetIoPriority(thread).ToString();
                    var efficiencyMode = EfficiencyModeHelper.IsEfficiencyModeEnabled(process.Id) ? "Enabled" : "Disabled";
                    var listViewItem = new ListViewItem([process.ProcessName, process.Id.ToString(), process.PriorityClass.ToString(), cpus.ToString(), io, disabled.ToString(), efficiencyMode]);
                    listView.Items.Add(listViewItem);
                }
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
        public void SearchProcess(ref ListView listView, string searchString)
        {
            ListViewItem? foundItem =
                listView.FindItemWithText(searchString, true, 0, true);
            if (foundItem != null)
                listView.TopItem = foundItem;
            else
                MessageBox.Show($"Process '{searchString}' was not found. Try refresh the list!", "InjectX GUI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
