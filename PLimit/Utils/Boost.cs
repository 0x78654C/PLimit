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
            if (!ProcessTarget.TryResolve(processesListBox, pid, out var target))
                return;

            var processManager = new ProcessesManage();
            if (!processManager.SetBoost(isEnable, target.ProcessId))
                return;

            if (target.IsUserAction && Properties.Settings.Default.isSaveingSettings)
            {
                var storeBoost = new StoreSettings();
                storeBoost.UpdateSetting(
                    StoreSettings.SettingType.Boosted,
                    target.ProcessName!,
                    SettingState.FromBoolean(isEnable));
            }

            if (!isStartUP)
            {
                from.BeginInvoke(new Action(() =>
                {
                    var utils = new Utils();
                    utils.RefreshProcessList(from, processesListBox, label);
                    utils.SearchProcess(searchBox, processesListBox);
                }));
            }

        }
    }
}
