using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PLimit.Utils
{
    internal class EfficiencyModeHelper
    {
       // const uint PROCESS_SET_INFORMATION = 0x0200;
        //const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

        // From Win32
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetPriorityClass(IntPtr hProcess, uint dwPriorityClass);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetPriorityClass(IntPtr hProcess);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetProcessInformation(
            IntPtr hProcess,
            PROCESS_INFORMATION_CLASS processInformationClass,
            ref PROCESS_POWER_THROTTLING_STATE processPowerThrottlingState,
            uint dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetProcessInformation(
            IntPtr hProcess,
            PROCESS_INFORMATION_CLASS processInformationClass,
            out PROCESS_POWER_THROTTLING_STATE processPowerThrottlingState,
            uint dwLength);

        enum PROCESS_INFORMATION_CLASS
        {
            ProcessMemoryPriority,
            ProcessPowerThrottling,
            // … other values …
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PROCESS_POWER_THROTTLING_STATE
        {
            public uint Version;
            public uint ControlMask;
            public uint StateMask;
        }

        const uint PROCESS_POWER_THROTTLING_CURRENT_VERSION = 1;
        const uint PROCESS_POWER_THROTTLING_EXECUTION_SPEED = 0x1;

        /// <summary>
        /// Ctor.
        /// </summary>
        public EfficiencyModeHelper() { }

        public bool EnableEfficiencyMode(int pid)
        {
            IntPtr hProc = Process.GetProcessById(pid).Handle;
            if (hProc == IntPtr.Zero) return false;

            // 1) Lower priority
            const uint IDLE_PRIORITY_CLASS = 0x40;
            if (!SetPriorityClass(hProc, IDLE_PRIORITY_CLASS))
                return false;

            // 2) Enable EcoQoS / throttle
            var throttling = new PROCESS_POWER_THROTTLING_STATE
            {
                Version = PROCESS_POWER_THROTTLING_CURRENT_VERSION,
                ControlMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
                StateMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED
            };

            return SetProcessInformation(
                hProc,
                PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
                ref throttling,
                (uint)Marshal.SizeOf(typeof(PROCESS_POWER_THROTTLING_STATE))
            );
        }

        public bool DisableEfficiencyMode(int pid)
        {
            IntPtr hProc = Process.GetProcessById(pid).Handle;
            if (hProc == IntPtr.Zero) return false;

            // Restore normal priority
            const uint NORMAL_PRIORITY_CLASS = 0x20; // or use current priority prior to change
            SetPriorityClass(hProc, NORMAL_PRIORITY_CLASS);

            // Clear throttling
            var throttling = new PROCESS_POWER_THROTTLING_STATE
            {
                Version = PROCESS_POWER_THROTTLING_CURRENT_VERSION,
                ControlMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
                StateMask = 0  // disable
            };

            return SetProcessInformation(
                hProc,
                PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
                ref throttling,
                (uint)Marshal.SizeOf(typeof(PROCESS_POWER_THROTTLING_STATE))
            );
        }

        public static bool IsEfficiencyModeEnabled(int pid)
        {
            IntPtr hProc = Process.GetProcessById(pid).Handle;

            // Check priority
            int prio = (int)Process.GetProcessById(pid).PriorityClass;
            bool lowPrio = (prio == (int)ProcessPriorityClass.Idle ||
                           prio == (int)ProcessPriorityClass.BelowNormal);

            // Check EcoQoS throttle
            if (GetProcessInformation(
                    hProc,
                    PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
                    out PROCESS_POWER_THROTTLING_STATE state,
                    (uint)Marshal.SizeOf(typeof(PROCESS_POWER_THROTTLING_STATE)))
                && (state.StateMask & PROCESS_POWER_THROTTLING_EXECUTION_SPEED) != 0)
            {
                return lowPrio;
            }

            return false;
        }
    }
}
