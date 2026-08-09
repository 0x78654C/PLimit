namespace PLimit.Utils
{
    public class Efficiency
    {
        public void EnableEfficiency(
            Form from,
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            string pid = "",
            bool isStartUp = false) =>
            SetEfficiency(from, processesListBox, label, searchBox, true, pid, isStartUp);

        public void DisableEfficiency(
            Form from,
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            string pid = "",
            bool isStartUp = false) =>
            SetEfficiency(from, processesListBox, label, searchBox, false, pid, isStartUp);

        private static void SetEfficiency(
            Form from,
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            bool enabled,
            string pid,
            bool isStartUp)
        {
            if (!ProcessTarget.TryResolve(processesListBox, pid, out var target))
                return;

            try
            {
                var efficiency = new EfficiencyModeHelper();
                if (enabled)
                    efficiency.EnableEfficiencyMode(target.ProcessId);
                else
                    efficiency.DisableEfficiencyMode(target.ProcessId);
            }
            catch
            {
                MessageBox.Show("Failed to change efficiency mode! Try running the application as administrator.", "Process Limiter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (target.IsUserAction && Properties.Settings.Default.isSaveingSettings)
            {
                var settings = new StoreSettings();
                settings.UpdateSetting(
                    StoreSettings.SettingType.Efficiency,
                    target.ProcessName!,
                    SettingState.FromBoolean(enabled));
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
