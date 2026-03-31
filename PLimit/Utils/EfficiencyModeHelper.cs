using System;
using System.ComponentModel;
using System.Diagnostics;
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
            out PROCESS_POWER_THROTTLING_STATE processInformation,
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

        public bool EnableEfficiencyMode(int pid)
        {
            IntPtr hProc = OpenProcess(
                PROCESS_SET_INFORMATION | PROCESS_QUERY_LIMITED_INFORMATION,
                false,
                pid);

            if (hProc == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                if (!SetPriorityClass(hProc, IDLE_PRIORITY_CLASS))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                var state = new PROCESS_POWER_THROTTLING_STATE
                {
                    Version = PROCESS_POWER_THROTTLING_CURRENT_VERSION,
                    ControlMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
                    StateMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED
                };

                if (!SetProcessInformation(
                    hProc,
                    PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
                    ref state,
                    (uint)Marshal.SizeOf<PROCESS_POWER_THROTTLING_STATE>()))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                return true;
            }
            finally
            {
                CloseHandle(hProc);
            }
        }

        public bool DisableEfficiencyMode(int pid)
        {
            IntPtr hProc = OpenProcess(
                PROCESS_SET_INFORMATION | PROCESS_QUERY_LIMITED_INFORMATION,
                false,
                pid);

            if (hProc == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                if (!SetPriorityClass(hProc, NORMAL_PRIORITY_CLASS))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                var state = new PROCESS_POWER_THROTTLING_STATE
                {
                    Version = PROCESS_POWER_THROTTLING_CURRENT_VERSION,
                    ControlMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
                    StateMask = 0
                };

                if (!SetProcessInformation(
                    hProc,
                    PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
                    ref state,
                    (uint)Marshal.SizeOf<PROCESS_POWER_THROTTLING_STATE>()))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                return true;
            }
            finally
            {
                CloseHandle(hProc);
            }
        }
        public bool IsEfficiencyModeEnabled(int pid)
        {
            IntPtr hProc = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
            if (hProc == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                uint priorityClass = GetPriorityClass(hProc);
                if (priorityClass == 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                bool lowPrio =
                    priorityClass == IDLE_PRIORITY_CLASS ||
                    priorityClass == BELOW_NORMAL_PRIORITY_CLASS;

                bool gotInfo = GetProcessInformation(
                    hProc,
                    PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
                    out var state,
                    (uint)Marshal.SizeOf<PROCESS_POWER_THROTTLING_STATE>());

                if (!gotInfo)
                {
                    int err = Marshal.GetLastWin32Error();

                    // Optional: log this instead of throwing if you want "false" on unsupported systems
                    // throw new Win32Exception(err);

                    return false;
                }

                bool ecoQosEnabled =
                    (state.StateMask & PROCESS_POWER_THROTTLING_EXECUTION_SPEED) != 0;

                return lowPrio && ecoQosEnabled;
            }
            finally
            {
                CloseHandle(hProc);
            }
        }
    }
}