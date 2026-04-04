using System.Diagnostics;


namespace PLimit.Utils
{
    public class PriorityProcess
    {
        public PriorityProcess() { }

        /// <summary>
        /// Sets the priority of the process to High.
        /// This means that the process will have a higher priority than normal processes, but not as high as real-time processes.
        /// It is suitable for processes that require more CPU time than normal processes but do not need to be prioritized over real-time processes.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        public void HighPriority(Form from, DoubleBufferedListView processesListBox, Label label, TextBox searchBox, string pid = "")
        {
            var processId = string.IsNullOrEmpty(pid) ? processesListBox.SelectedItems[0].SubItems[1].Text : pid;
            var setPriority = new ProcessesManage();
            setPriority.SetPriorityClass(ProcessPriorityClass.High, int.Parse(processId));
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
                    var settingPriority = new StoreSettings();
                    settingPriority.UpdateSetting(StoreSettings.SettingType.Priority, processesListBox.SelectedItems[0].SubItems[0].Text, "High");
                }
            }
        }

        /// <summary>
        /// Sets the priority of the process to Above Normal.
        /// This means that the process will have a higher priority than normal processes, but not as high as real-time processes.
        /// It is suitable for processes that require more CPU time than normal processes but do not need to be prioritized over real-time processes.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        public void AboveNormalPriority(Form from, DoubleBufferedListView processesListBox, Label label, TextBox searchBox, string pid = "")
        {
            var processId = string.IsNullOrEmpty(pid) ? processesListBox.SelectedItems[0].SubItems[1].Text : pid;
            var setPriority = new ProcessesManage();
            setPriority.SetPriorityClass(ProcessPriorityClass.AboveNormal, int.Parse(processId));
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
                    var settingPriority = new StoreSettings();
                    settingPriority.UpdateSetting(StoreSettings.SettingType.Priority, processesListBox.SelectedItems[0].SubItems[0].Text, "AboveNormal");
                }
            }
        }

        /// <summary>
        /// Sets the priority of the process to Real Time.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        public void RealTimePriority(Form from, DoubleBufferedListView processesListBox, Label label, TextBox searchBox, string pid = "")
        {
            var processId = string.IsNullOrEmpty(pid) ? processesListBox.SelectedItems[0].SubItems[1].Text : pid;
            var setPriority = new ProcessesManage();
            setPriority.SetPriorityClass(ProcessPriorityClass.RealTime, int.Parse(processId));
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
                    var settingPriority = new StoreSettings();
                    settingPriority.UpdateSetting(StoreSettings.SettingType.Priority, processesListBox.SelectedItems[0].SubItems[0].Text, "RealTime");
                }
            }
        }

        /// <summary>
        /// Sets the priority of the process to Normal.
        /// This means that the process will have a normal priority level, which is the default priority for most processes. 
        /// It is suitable for processes that require a balanced performance and do not need to be prioritized over other processes.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        public void NormalPriority(Form from, DoubleBufferedListView processesListBox, Label label, TextBox searchBox, string pid = "")
        {
            var processId = string.IsNullOrEmpty(pid) ? processesListBox.SelectedItems[0].SubItems[1].Text : pid;
            var setPriority = new ProcessesManage();
            setPriority.SetPriorityClass(ProcessPriorityClass.Normal, int.Parse(processId));
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
                    var settingPriority = new StoreSettings();
                    settingPriority.UpdateSetting(StoreSettings.SettingType.Priority, processesListBox.SelectedItems[0].SubItems[0].Text, "Normal");
                }
            }
        }


        /// <summary>
        /// Sets the priority of the process to Below Normal. 
        /// This means that the process will have a lower priority than Normal, but higher than Idle. 
        /// It is useful for processes that are not time-sensitive and can run in the background without affecting the performance of other processes.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        public void BelowNormalPriority(Form from, DoubleBufferedListView processesListBox, Label label, TextBox searchBox, string pid = "")
        {
            var processId = string.IsNullOrEmpty(pid) ? processesListBox.SelectedItems[0].SubItems[1].Text : pid;
            var setPriority = new ProcessesManage();
            setPriority.SetPriorityClass(ProcessPriorityClass.BelowNormal, int.Parse(processId));
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
                    var settingPriority = new StoreSettings();
                    settingPriority.UpdateSetting(StoreSettings.SettingType.Priority, processesListBox.SelectedItems[0].SubItems[0].Text, "BelowNormal");
                }
            }
        }
        /// <summary>
        /// Reads the dynamic thread priority boost status for the selected process.
        /// Returns true if boost is enabled, false if disabled, null if the status could not be read (e.g. access denied).
        /// </summary>
        /// <param name="processId"></param>
        public bool? GetThreadPriorityBoost(int processId)
        {
            var manage = new ProcessesManage();
            return manage.GetThreadBoost(processId);
        }

        /// <summary>
        /// Enables or disables the dynamic thread priority boost for all threads of the selected process.
        /// When enabled, Windows temporarily raises a thread's priority after it wakes from a wait (the default OS behavior).
        /// When disabled, threads run at their base priority without any dynamic boost.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        /// <param name="enable">true to enable boost, false to disable it.</param>
        /// <param name="pid"></param>
        public void SetThreadPriorityBoost(Form from, DoubleBufferedListView processesListBox, Label label, TextBox searchBox, bool enable, string pid = "")
        {
            var processId = string.IsNullOrEmpty(pid) ? processesListBox.SelectedItems[0].SubItems[1].Text : pid;
            var manage = new ProcessesManage();
            manage.SetThreadBoost(enable, int.Parse(processId));
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
                    settingIO.UpdateSetting(StoreSettings.SettingType.Wdptb, processesListBox.SelectedItems[0].SubItems[0].Text, enable ? "Enabled" : "Disabled");
                }
            }
        }
    }
}
