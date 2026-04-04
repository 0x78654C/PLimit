namespace PLimit.Utils
{
    public class IOPriority
    {   
        public IOPriority() { }

        /// <summary>
        /// Sets the I/O priority of all threads in the selected process to Very Low.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        public void IOVeryLowPriority(Form from, DoubleBufferedListView processesListBox, Label label, TextBox searchBox, string pid = "")
        {
            var processId = string.IsNullOrEmpty(pid) ? processesListBox.SelectedItems[0].SubItems[1].Text : pid;
            var setIoPriority = new ProcessesManage();
            setIoPriority.SetIoPriorityAllThreads(int.Parse(processId), ProcessesManage.IO_PRIORITY_HINT.VeryLow);
            from.BeginInvoke(new Action(() =>
            {
                var utils = new Utils();
                utils.RefreshProcessList(from, processesListBox, label);
                utils.SearchProcess(searchBox, processesListBox);
            }));
            if (string.IsNullOrEmpty(pid))
            {
                if (Properties.Settings.Default.isSaveingSettings)
                {
                    var settingIO = new StoreSettings();
                    settingIO.UpdateSetting(StoreSettings.SettingType.IOPriority, processesListBox.SelectedItems[0].SubItems[0].Text, "VeryLow");
                }
            }
        }

        /// <summary>
        /// Sets the I/O priority of all threads in the selected process to Low.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        public void IOLowPriority(Form from, DoubleBufferedListView processesListBox, Label label, TextBox searchBox, string pid = "")
        {
            var processId = string.IsNullOrEmpty(pid) ? processesListBox.SelectedItems[0].SubItems[1].Text : pid;
            var setIoPriority = new ProcessesManage();
            setIoPriority.SetIoPriorityAllThreads(int.Parse(processId), ProcessesManage.IO_PRIORITY_HINT.Low);
            from.BeginInvoke(new Action(() =>
            {
                var utils = new Utils();
                utils.RefreshProcessList(from, processesListBox, label);
                utils.SearchProcess(searchBox, processesListBox);
            }));
            if (string.IsNullOrEmpty(pid))
            {
                if (Properties.Settings.Default.isSaveingSettings)
                {
                    var settingIO = new StoreSettings();
                    settingIO.UpdateSetting(StoreSettings.SettingType.IOPriority, processesListBox.SelectedItems[0].SubItems[0].Text, "Low");
                }
            }
        }

        /// <summary>
        /// Sets the I/O priority of all threads in the selected process to Normal.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        public void IONormalPriority(Form from, DoubleBufferedListView processesListBox, Label label, TextBox searchBox, string pid = "")
        {
            var processId = string.IsNullOrEmpty(pid) ? processesListBox.SelectedItems[0].SubItems[1].Text : pid;
            var setIoPriority = new ProcessesManage();
            setIoPriority.SetIoPriorityAllThreads(int.Parse(processId), ProcessesManage.IO_PRIORITY_HINT.Normal);
            from.BeginInvoke(new Action(() =>
            {
                var utils = new Utils();
                utils.RefreshProcessList(from, processesListBox, label);
                utils.SearchProcess(searchBox, processesListBox);
            }));
            if (string.IsNullOrEmpty(pid))
            {
                if (Properties.Settings.Default.isSaveingSettings)
                {
                    var settingIO = new StoreSettings();
                    settingIO.UpdateSetting(StoreSettings.SettingType.IOPriority, processesListBox.SelectedItems[0].SubItems[0].Text, "Normal");
                }
            }
        }

        /// <summary>
        /// Sets the I/O priority of all threads in the selected process to High.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        public void IOHighPriority(Form from, DoubleBufferedListView processesListBox, Label label, TextBox searchBox, string pid = "")
        {
            var processId = string.IsNullOrEmpty(pid) ? processesListBox.SelectedItems[0].SubItems[1].Text : pid;
            var setIoPriority = new ProcessesManage();
            setIoPriority.SetIoPriorityAllThreads(int.Parse(processId), ProcessesManage.IO_PRIORITY_HINT.High);
            from.BeginInvoke(new Action(() =>
            {
                var utils = new Utils();
                utils.RefreshProcessList(from, processesListBox, label);
                utils.SearchProcess(searchBox, processesListBox);
            }));
            if (string.IsNullOrEmpty(pid))
            {
                if (Properties.Settings.Default.isSaveingSettings)
                {
                    var settingIO = new StoreSettings();
                    settingIO.UpdateSetting(StoreSettings.SettingType.IOPriority, processesListBox.SelectedItems[0].SubItems[0].Text, "High");
                }
            }
        }
    }
}
