namespace PLimit.Utils
{
    public class Efficiency
    {
        public Efficiency() { }

        /// <summary>
        /// Enables efficiency mode for the selected process. 
        /// This method retrieves the process ID from the selected item in the processes list box, then uses the EfficiencyModeHelper to enable efficiency mode for that process.
        /// After enabling efficiency mode, it refreshes the process list and applies any search filters to update the display accordingly.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        public void EnableEfficiency(Form from, DoubleBufferedListView processesListBox, Label label, TextBox searchBox, string pid = "")
        {
            var processId = string.IsNullOrEmpty(pid) ? processesListBox.SelectedItems[0].SubItems[1].Text : pid;
            var setEfficiencyMode = new EfficiencyModeHelper();
            setEfficiencyMode.EnableEfficiencyMode(int.Parse(processId));
            from.BeginInvoke(new Action(() =>
            {
                var utils = new Utils();
                utils.RefreshProcessList(from, processesListBox, label);
                utils.SearchProcess(searchBox, processesListBox);
            }));
            if (string.IsNullOrEmpty(pid))
            {
                var storeEfficiency = new StoreSettings();
                storeEfficiency.UpdateSetting(StoreSettings.SettingType.Efficiency, processesListBox.SelectedItems[0].SubItems[0].Text, "True");
            }
        }

        /// <summary>
        /// Disables efficiency mode for the selected process.
        /// This method retrieves the process ID from the selected item in the processes list box, then uses the EfficiencyModeHelper to disable efficiency mode for that process.
        /// After disabling efficiency mode, it refreshes the process list and applies any search filters to update the display accordingly.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        public void DisableEfficiency(Form from, DoubleBufferedListView processesListBox, Label label, TextBox searchBox, string pid = "")
        {
            var processId = string.IsNullOrEmpty(pid) ? processesListBox.SelectedItems[0].SubItems[1].Text : pid;
            var setEfficiencyMode = new EfficiencyModeHelper();
            setEfficiencyMode.DisableEfficiencyMode(int.Parse(processId));
            from.BeginInvoke(new Action(() =>
            {
                var utils = new Utils();
                utils.RefreshProcessList(from, processesListBox, label);
                utils.SearchProcess(searchBox, processesListBox);
            }));
            if (string.IsNullOrEmpty(pid))
            {
                var storeEfficiency = new StoreSettings();
                storeEfficiency.UpdateSetting(StoreSettings.SettingType.Efficiency, processesListBox.SelectedItems[0].SubItems[0].Text, "False");
            }
        }
    }
}
