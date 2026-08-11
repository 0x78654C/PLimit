using System.Text.Json;

namespace PLimit.Utils
{
    public class ReadSettings
    {
        private IReadOnlyDictionary<string, ProcessData>? _settingsByName;

        /// <summary>
        /// Applies the saved process priority-boost state. Both the current
        /// Enabled/Disabled format and legacy True/False values are supported.
        /// </summary>
        public void ReadSettingsBoost(
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            Form from,
            string pid = "",
            bool isStartUp = false)
        {
            var boost = new Boost();
            ApplyToTargets(processesListBox, pid, (setting, processId) =>
            {
                if (SettingState.TryParse(setting.Boosted, out bool enabled))
                    boost.SetBoost(from, processesListBox, label, searchBox, enabled, processId, isStartUp);
            });
        }

        /// <summary>
        /// Applies the saved efficiency-mode state.
        /// </summary>
        public void ReadSettingsEfficiency(
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            Form from,
            string pid = "",
            bool isStartUp = false)
        {
            var efficiency = new Efficiency();
            ApplyToTargets(processesListBox, pid, (setting, processId) =>
            {
                if (!SettingState.TryParse(setting.Efficiency, out bool enabled))
                    return;

                if (enabled)
                    efficiency.EnableEfficiency(from, processesListBox, label, searchBox, processId, isStartUp);
                else
                    efficiency.DisableEfficiency(from, processesListBox, label, searchBox, processId, isStartUp);
            });
        }

        /// <summary>
        /// Applies the saved processor affinity.
        /// </summary>
        public void ReadSettingsAffinity(
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            Form from,
            string pid = "",
            bool isStartUp = false)
        {
            var affinity = new Affinity();
            ApplyToTargets(processesListBox, pid, (setting, processId) =>
            {
                if (!string.IsNullOrWhiteSpace(setting.Affinity))
                {
                    affinity.SetAffinity(
                        from,
                        processesListBox,
                        null,
                        label,
                        searchBox,
                        null,
                        setting.Affinity,
                        processId,
                        isStartUp);
                }
            });
        }

        /// <summary>
        /// Applies the saved process priority class.
        /// </summary>
        public void ReadSettingsPriority(
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            Form from,
            string pid = "",
            bool isStartUp = false)
        {
            var priority = new PriorityProcess();
            ApplyToTargets(processesListBox, pid, (setting, processId) =>
            {
                switch (setting.Property)
                {
                    case "Normal":
                        priority.NormalPriority(from, processesListBox, label, searchBox, processId, isStartUp);
                        break;
                    case "High":
                        priority.HighPriority(from, processesListBox, label, searchBox, processId, isStartUp);
                        break;
                    case "AboveNormal":
                        priority.AboveNormalPriority(from, processesListBox, label, searchBox, processId, isStartUp);
                        break;
                    case "BelowNormal":
                        priority.BelowNormalPriority(from, processesListBox, label, searchBox, processId, isStartUp);
                        break;
                    case "RealTime":
                        priority.RealTimePriority(from, processesListBox, label, searchBox, processId, isStartUp);
                        break;
                }
            });
        }

        /// <summary>
        /// Applies the saved I/O priority.
        /// </summary>
        public void ReadSettingsIOPriority(
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            Form from,
            string pid = "",
            bool isStartUp = false)
        {
            var ioPriority = new IOPriority();
            ApplyToTargets(processesListBox, pid, (setting, processId) =>
            {
                switch (setting.IOProperty)
                {
                    case "Normal":
                        ioPriority.IONormalPriority(from, processesListBox, label, searchBox, processId, isStartUp);
                        break;
                    case "Low":
                        ioPriority.IOLowPriority(from, processesListBox, label, searchBox, processId, isStartUp);
                        break;
                    case "High":
                        ioPriority.IOHighPriority(from, processesListBox, label, searchBox, processId, isStartUp);
                        break;
                    case "VeryLow":
                        ioPriority.IOVeryLowPriority(from, processesListBox, label, searchBox, processId, isStartUp);
                        break;
                }
            });
        }

        /// <summary>
        /// Applies the saved dynamic thread priority-boost state.
        /// </summary>
        public void ReadWdptbSettings(
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            Form from,
            string pid = "",
            bool isStartUp = false)
        {
            var priority = new PriorityProcess();
            ApplyToTargets(processesListBox, pid, (setting, processId) =>
            {
                if (SettingState.TryParse(setting.Wdptb, out bool enabled))
                {
                    priority.SetThreadPriorityBoost(
                        from,
                        processesListBox,
                        label,
                        searchBox,
                        enabled,
                        processId,
                        isStartUp);
                }
            });
        }

        /// <summary>
        /// Shows the saved settings for the selected process without rewriting the file.
        /// </summary>
        public void ShowSettings(
            DoubleBufferedListView processesListBox,
            Label label,
            TextBox searchBox,
            Form from,
            string jsonFilePath)
        {
            if (processesListBox.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select a process first.", "Process Limiter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string processName = processesListBox.SelectedItems[0].SubItems[0].Text;
            try
            {
                if (!File.Exists(jsonFilePath))
                {
                    MessageBox.Show("No process settings have been saved yet.", "Process Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var data = Json.JsonManage.ReadJsonFromFile<ProcessData[]>(jsonFilePath);
                var processData = data.LastOrDefault(item =>
                    item != null && string.Equals(item.ProcessName, processName, StringComparison.OrdinalIgnoreCase));

                if (processData == null)
                {
                    MessageBox.Show($"There are no settings saved for: {processName}", "Process Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string message = $"Process Name: {processData.ProcessName}\n" +
                                 $"Process Boost: {DisplayValue(processData.Boosted)}\n" +
                                 $"Efficiency: {DisplayValue(processData.Efficiency)}\n" +
                                 $"Affinity: {DisplayValue(processData.Affinity)}\n" +
                                 $"Priority: {DisplayValue(processData.Property)}\n" +
                                 $"I/O Priority: {DisplayValue(processData.IOProperty)}\n" +
                                 $"Thread Boost: {DisplayValue(processData.Wdptb)}";
                MessageBox.Show(message, "Process Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
            {
                MessageBox.Show($"Unable to read the saved settings: {ex.Message}", "Process Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyToTargets(
            DoubleBufferedListView processesListBox,
            string requestedPid,
            Action<ProcessData, string> apply)
        {
            var settings = GetSettingsByName();
            if (settings.Count == 0)
                return;

            foreach (ListViewItem item in processesListBox.Items)
            {
                if (item.SubItems.Count < 2)
                    continue;

                string processId = item.SubItems[1].Text;
                if (!string.IsNullOrEmpty(requestedPid) &&
                    !string.Equals(processId, requestedPid, StringComparison.Ordinal))
                {
                    continue;
                }

                if (settings.TryGetValue(item.SubItems[0].Text, out var setting))
                    apply(setting, processId);

                if (!string.IsNullOrEmpty(requestedPid))
                    return;
            }
        }

        private IReadOnlyDictionary<string, ProcessData> GetSettingsByName()
        {
            if (_settingsByName != null)
                return _settingsByName;

            var settings = new Dictionary<string, ProcessData>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(GlobalVars.LogFilePath))
                return _settingsByName = settings;

            try
            {
                foreach (var setting in Json.JsonManage.ReadJsonFromFile<ProcessData[]>(GlobalVars.LogFilePath))
                {
                    if (setting != null && !string.IsNullOrWhiteSpace(setting.ProcessName))
                        settings[setting.ProcessName] = setting;
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
            {
                // A missing, unreadable, or malformed file must not crash startup.
            }

            return _settingsByName = settings;
        }

        private static string DisplayValue(string? value) =>
            string.IsNullOrWhiteSpace(value) ? "Not saved" : value;
    }
}
