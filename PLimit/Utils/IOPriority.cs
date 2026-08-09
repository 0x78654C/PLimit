namespace PLimit.Utils
{
    public class IOPriority
    {
        public void IOVeryLowPriority(
            Form from,
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            string pid = "",
            bool isStartUp = false) =>
            SetPriority(from, processesListBox, label, searchBox, ProcessesManage.IO_PRIORITY_HINT.VeryLow, pid, isStartUp);

        public void IOLowPriority(
            Form from,
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            string pid = "",
            bool isStartUp = false) =>
            SetPriority(from, processesListBox, label, searchBox, ProcessesManage.IO_PRIORITY_HINT.Low, pid, isStartUp);

        public void IONormalPriority(
            Form from,
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            string pid = "",
            bool isStartUp = false) =>
            SetPriority(from, processesListBox, label, searchBox, ProcessesManage.IO_PRIORITY_HINT.Normal, pid, isStartUp);

        public void IOHighPriority(
            Form from,
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            string pid = "",
            bool isStartUp = false) =>
            SetPriority(from, processesListBox, label, searchBox, ProcessesManage.IO_PRIORITY_HINT.High, pid, isStartUp);

        private static void SetPriority(
            Form from,
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            ProcessesManage.IO_PRIORITY_HINT priority,
            string pid,
            bool isStartUp)
        {
            if (!ProcessTarget.TryResolve(processesListBox, pid, out var target))
                return;

            var processManager = new ProcessesManage();
            if (!processManager.SetIoPriorityAllThreads(target.ProcessId, priority))
            {
                MessageBox.Show("Failed to set I/O priority! Try running the application as administrator.", "Process Limiter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (target.IsUserAction && Properties.Settings.Default.isSaveingSettings)
            {
                var settings = new StoreSettings();
                settings.UpdateSetting(
                    StoreSettings.SettingType.IOPriority,
                    target.ProcessName!,
                    priority.ToString());
            }

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
