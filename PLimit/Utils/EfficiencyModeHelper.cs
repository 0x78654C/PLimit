using System.ComponentModel;
using System.Runtime.InteropServices;

namespace PLimit.Utils
{
    public class EfficiencyModeHelper
    {
        private const uint PROCESS_SET_INFORMATION = 0x0200;
        private const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

        private const uint IDLE_PRIORITY_CLASS = 0x00000040;
        private const uint BELOW_NORMAL_PRIORITY_CLASS = 0x00004000;
        private const uint NORMAL_PRIORITY_CLASS = 0x00000020;

        private const uint PROCESS_POWER_THROTTLING_CURRENT_VERSION = 1;
        private const uint PROCESS_POWER_THROTTLING_EXECUTION_SPEED = 0x1;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
            uint dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetPriorityClass(IntPtr hProcess, uint dwPriorityClass);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetPriorityClass(IntPtr hProcess);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetProcessInformation(
            IntPtr hProcess,
            PROCESS_INFORMATION_CLASS processInformationClass,
            ref PROCESS_POWER_THROTTLING_STATE processInformation,
            uint processInformationSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetProcessInformation(
            IntPtr hProcess,
            PROCESS_INFORMATION_CLASS processInformationClass,
            ref PROCESS_POWER_THROTTLING_STATE processInformation,
            uint processInformationSize);

        private enum PROCESS_INFORMATION_CLASS
        {
            ProcessMemoryPriority = 0,
            ProcessMemoryExhaustionInfo = 1,
            ProcessAppMemoryInfo = 2,
            ProcessInPrivateInfo = 3,
            ProcessPowerThrottling = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_POWER_THROTTLING_STATE
        {
            public uint Version;
            public uint ControlMask;
            public uint StateMask;
        }

        public bool EnableEfficiencyMode(int pid) => SetEfficiencyMode(pid, true);

        public bool DisableEfficiencyMode(int pid) => SetEfficiencyMode(pid, false);

        private bool SetEfficiencyMode(int pid, bool enabled)
        {
            IntPtr hProc = OpenProcess(
                PROCESS_SET_INFORMATION | PROCESS_QUERY_LIMITED_INFORMATION,
                false,
                pid);

            if (hProc == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                SetEfficiencyMode(hProc, enabled, changePriority: true);
                return true;
            }
            finally
            {
                CloseHandle(hProc);
            }
        }

        internal void SetEfficiencyMode(IntPtr hProc, bool enabled, bool changePriority)
        {
            uint previousPriority = GetPriorityClass(hProc);
            if (previousPriority == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            var previous = new PROCESS_POWER_THROTTLING_STATE { Version = PROCESS_POWER_THROTTLING_CURRENT_VERSION };
            bool canRestore = GetProcessInformation(hProc, PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
                ref previous, (uint)Marshal.SizeOf<PROCESS_POWER_THROTTLING_STATE>());
            var state = new PROCESS_POWER_THROTTLING_STATE
            {
                Version = PROCESS_POWER_THROTTLING_CURRENT_VERSION,
                ControlMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
                StateMask = enabled ? PROCESS_POWER_THROTTLING_EXECUTION_SPEED : 0
            };
            // Apply EcoQoS first: an unsupported API must not leave the process stuck at Idle priority.
            if (!SetProcessInformation(hProc, PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
                ref state, (uint)Marshal.SizeOf<PROCESS_POWER_THROTTLING_STATE>()))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            uint desired = enabled ? IDLE_PRIORITY_CLASS
                : previousPriority is IDLE_PRIORITY_CLASS or BELOW_NORMAL_PRIORITY_CLASS ? NORMAL_PRIORITY_CLASS : previousPriority;
            if (changePriority && desired != previousPriority && !SetPriorityClass(hProc, desired))
            {
                int error = Marshal.GetLastWin32Error();
                if (canRestore)
                    SetProcessInformation(hProc, PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
                        ref previous, (uint)Marshal.SizeOf<PROCESS_POWER_THROTTLING_STATE>());
                throw new Win32Exception(error);
            }
        }
        public bool IsEfficiencyModeEnabled(int pid)
        {
            IntPtr hProc = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
            if (hProc == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                return IsEfficiencyModeEnabled(hProc);
            }
            finally
            {
                CloseHandle(hProc);
            }
        }

        internal bool IsEfficiencyModeEnabled(IntPtr processHandle)
        {
            uint priorityClass = GetPriorityClass(processHandle);
            if (priorityClass == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            bool lowPriority =
                priorityClass == IDLE_PRIORITY_CLASS ||
                priorityClass == BELOW_NORMAL_PRIORITY_CLASS;

            return lowPriority && IsEcoQosEnabled(processHandle);
        }

        internal bool IsEcoQosEnabled(IntPtr processHandle)
        {
            var state = new PROCESS_POWER_THROTTLING_STATE
            {
                Version = PROCESS_POWER_THROTTLING_CURRENT_VERSION
            };
            bool gotInfo = GetProcessInformation(
                processHandle,
                PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
                ref state,
                (uint)Marshal.SizeOf<PROCESS_POWER_THROTTLING_STATE>());

            if (!gotInfo)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            bool ecoQosEnabled =
                (state.StateMask & PROCESS_POWER_THROTTLING_EXECUTION_SPEED) != 0;

            return ecoQosEnabled;
        }
    }
}
