using System.Diagnostics;

namespace PLimit.Utils
{
    public class PriorityProcess
    {
        public void HighPriority(
            Form from,
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            string pid = "",
            bool isStartUp = false) =>
            SetPriority(from, processesListBox, label, searchBox, ProcessPriorityClass.High, pid, isStartUp);

        public void AboveNormalPriority(
            Form from,
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            string pid = "",
            bool isStartUp = false) =>
            SetPriority(from, processesListBox, label, searchBox, ProcessPriorityClass.AboveNormal, pid, isStartUp);

        public void RealTimePriority(
            Form from,
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            string pid = "",
            bool isStartUp = false) =>
            SetPriority(from, processesListBox, label, searchBox, ProcessPriorityClass.RealTime, pid, isStartUp);

        public void NormalPriority(
            Form from,
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            string pid = "",
            bool isStartUp = false) =>
            SetPriority(from, processesListBox, label, searchBox, ProcessPriorityClass.Normal, pid, isStartUp);

        public void BelowNormalPriority(
            Form from,
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            string pid = "",
            bool isStartUp = false) =>
            SetPriority(from, processesListBox, label, searchBox, ProcessPriorityClass.BelowNormal, pid, isStartUp);

        /// <summary>
        /// Enables or disables dynamic priority boost for every accessible thread
        /// in the selected process.
        /// </summary>
        public void SetThreadPriorityBoost(
            Form from,
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            bool enable,
            string pid = "",
            bool isStartUp = false)
        {
            if (!ProcessTarget.TryResolve(processesListBox, pid, out var target))
                return;

            var processManager = new ProcessesManage();
            if (!processManager.SetThreadBoost(enable, target.ProcessId))
                return;

            SaveSettingIfRequested(
                target,
                StoreSettings.SettingType.Wdptb,
                SettingState.FromBoolean(enable));
            RefreshIfRequested(from, processesListBox, label, searchBox, isStartUp);
        }

        private static void SetPriority(
            Form from,
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            ProcessPriorityClass priority,
            string pid,
            bool isStartUp)
        {
            if (!ProcessTarget.TryResolve(processesListBox, pid, out var target))
                return;

            var processManager = new ProcessesManage();
            if (!processManager.SetPriorityClass(priority, target.ProcessId))
                return;

            SaveSettingIfRequested(target, StoreSettings.SettingType.Priority, priority.ToString());
            RefreshIfRequested(from, processesListBox, label, searchBox, isStartUp);
        }

        private static void SaveSettingIfRequested(
            ProcessTarget target,
            StoreSettings.SettingType settingType,
            string value)
        {
            if (!target.IsUserAction || !Properties.Settings.Default.isSaveingSettings)
                return;

            var settings = new StoreSettings();
            settings.UpdateSetting(settingType, target.ProcessName!, value);
        }

        private static void RefreshIfRequested(
            Form from,
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            bool isStartUp)
        {
            if (isStartUp)
                return;

            from.BeginInvoke(new Action(() =>
            {
                var utils = new Utils();
                utils.RefreshProcessList(from, processesListBox, label);
                utils.SearchProcess(searchBox, processesListBox);
            }));
        }
    }
}
