namespace PLimit.Utils
{
    public class Boost
    {
        public Boost() { }


        /// <summary>
        /// Set boost for the selected process, then refresh the process list and search for the process again to update the boost status in the list.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        public void SetBoost(Form from, DoubleBufferedListView processesListBox, Label label, TextBox searchBox, bool isEnable, string pid = "", bool isStartUP = false)
        {
            var processId = string.IsNullOrEmpty(pid) ? processesListBox.SelectedItems[0].SubItems[1].Text : pid;
            var setBoost = new ProcessesManage();
            setBoost.SetBoost(isEnable, int.Parse(processId));
            if (!isStartUP)
            {
                from.BeginInvoke(new Action(() =>
                {
                    var utils = new Utils();
                    utils.RefreshProcessList(from, processesListBox, label);
                    utils.SearchProcess(searchBox, processesListBox);
                }));
            }

            if (string.IsNullOrEmpty(pid))
            {
                if (Properties.Settings.Default.isSaveingSettings)
                {
                    var storeBoost = new StoreSettings();
                    storeBoost.UpdateSetting(StoreSettings.SettingType.Boosted, processesListBox.SelectedItems[0].SubItems[0].Text, isEnable ? "True" : "False");
                }
            }
        }
    }
}
