namespace PLimit.Utils
{
    public class ReadSettings
    {
        private string _logDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");
        private string _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings", "processes.json");
        public ReadSettings() { }

        /// <summary>
        /// Read the settings from the specified file path and apply the boost settings to the processes in the list box.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        /// <param name="from"></param>
        public void ReadSettingsBoost(
    DoubleBufferedListView processesListBox,
    Label label,
    TextBox searchBox,
    Form from)
        {
            if (!File.Exists(_logFilePath))
                return;

            ProcessData[]? settings;
            try
            {
                settings = Json.JsonManage.ReadJsonFromFile<ProcessData[]>(_logFilePath);
            }
            catch
            {
                return;
            }

            if (settings == null || settings.Length == 0)
                return;

            var processLookup = processesListBox.Items
                .Cast<ListViewItem>()
                .Where(item => item.SubItems.Count > 1 && !string.IsNullOrWhiteSpace(item.SubItems[0].Text))
                .GroupBy(item => item.SubItems[0].Text, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().SubItems[1].Text,
                    StringComparer.OrdinalIgnoreCase);

            var boost = new Boost();

            foreach (var setting in settings)
            {
                if (setting == null ||
                    string.IsNullOrWhiteSpace(setting.ProcessName) ||
                    string.IsNullOrWhiteSpace(setting.Boosted))
                {
                    continue;
                }

                if (!processLookup.TryGetValue(setting.ProcessName, out var pid))
                    continue;

                if (bool.TryParse(setting.Boosted, out bool isBoosted))
                {
                    boost.SetBoost(from, processesListBox, label, searchBox, isBoosted, pid);
                }
            }
        }

        /// <summary>
        /// Read the settings from the specified file path and apply the efficiency settings to the processes in the list box.
        /// </summary>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        /// <param name="from"></param>
        public void ReadSettingsEfficiency(
    DoubleBufferedListView processesListBox,
    Label label,
    TextBox searchBox,
    Form from)
        {
            if (!File.Exists(_logFilePath))
                return;

            ProcessData[]? settings;
            try
            {
                settings = Json.JsonManage.ReadJsonFromFile<ProcessData[]>(_logFilePath);
            }
            catch
            {
                return;
            }

            if (settings == null || settings.Length == 0)
                return;

            var processLookup = processesListBox.Items
                .Cast<ListViewItem>()
                .Where(item => item.SubItems.Count > 1 && !string.IsNullOrWhiteSpace(item.SubItems[0].Text))
                .GroupBy(item => item.SubItems[0].Text, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().SubItems[1].Text,
                    StringComparer.OrdinalIgnoreCase);

            var efficiency = new Efficiency();

            foreach (var setting in settings)
            {
                if (setting == null ||
                    string.IsNullOrWhiteSpace(setting.ProcessName) ||
                    string.IsNullOrWhiteSpace(setting.Efficiency))
                {
                    continue;
                }

                if (!processLookup.TryGetValue(setting.ProcessName, out var pid))
                    continue;

                if (bool.TryParse(setting.Efficiency, out bool isEnabled))
                {
                    if (isEnabled)
                        efficiency.EnableEfficiency(from, processesListBox, label, searchBox, pid);
                    else
                        efficiency.DisableEfficiency(from, processesListBox, label, searchBox, pid);
                }
            }
        }

        /// <summary>
        /// Read the settings from the specified file path and apply the affinity settings to the processes in the list box.
        /// </summary>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        /// <param name="from"></param>
        public void ReadSettingsAffinity(DoubleBufferedListView processesListBox,
Label label,
TextBox searchBox,
Form from)
        {
            if (!File.Exists(_logFilePath))
                return;

            ProcessData[]? settings;
            try
            {
                settings = Json.JsonManage.ReadJsonFromFile<ProcessData[]>(_logFilePath);
            }
            catch
            {
                return;
            }

            if (settings == null || settings.Length == 0)
                return;

            var processLookup = processesListBox.Items
                .Cast<ListViewItem>()
                .Where(item => item.SubItems.Count > 1 && !string.IsNullOrWhiteSpace(item.SubItems[0].Text))
                .GroupBy(item => item.SubItems[0].Text)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().SubItems[1].Text,
                    StringComparer.OrdinalIgnoreCase);

            var affinity = new Affinity();

            foreach (var setting in settings)
            {
                if (setting == null ||
                    string.IsNullOrWhiteSpace(setting.ProcessName) ||
                    string.IsNullOrWhiteSpace(setting.Affinity))
                {
                    continue;
                }

                if (processLookup.TryGetValue(setting.ProcessName, out var pidText))
                {
                    affinity.SetAffinity(
                        from,
                        processesListBox,
                        null,
                        label,
                        searchBox,
                        null,
                        setting.Affinity,
                        pidText);
                }
            }
        }
        /// <summary>
        /// Read the settings from the specified file path and apply the priority settings to the processes in the list box.
        /// </summary>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        /// <param name="from"></param>
        public void ReadSettingsPriority(
    DoubleBufferedListView processesListBox,
    Label label,
    TextBox searchBox,
    Form from)
        {
            if (!File.Exists(_logFilePath))
                return;

            ProcessData[]? settings;
            try
            {
                settings = Json.JsonManage.ReadJsonFromFile<ProcessData[]>(_logFilePath);
            }
            catch
            {
                return;
            }

            if (settings == null || settings.Length == 0)
                return;

            var processLookup = processesListBox.Items
                .Cast<ListViewItem>()
                .Where(item => item.SubItems.Count > 1 && !string.IsNullOrWhiteSpace(item.SubItems[0].Text))
                .GroupBy(item => item.SubItems[0].Text, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().SubItems[1].Text,
                    StringComparer.OrdinalIgnoreCase);

            var priority = new PriorityProcess();

            foreach (var setting in settings)
            {
                if (setting == null ||
                    string.IsNullOrWhiteSpace(setting.ProcessName) ||
                    string.IsNullOrWhiteSpace(setting.Property))
                {
                    continue;
                }

                if (!processLookup.TryGetValue(setting.ProcessName, out var pid))
                    continue;

                switch (setting.Property.Trim())
                {
                    case "Normal":
                        priority.NormalPriority(from, processesListBox, label, searchBox, pid);
                        break;

                    case "High":
                        priority.HighPriority(from, processesListBox, label, searchBox, pid);
                        break;

                    case "AboveNormal":
                        priority.AboveNormalPriority(from, processesListBox, label, searchBox, pid);
                        break;

                    case "BelowNormal":
                        priority.BelowNormalPriority(from, processesListBox, label, searchBox, pid);
                        break;

                    case "RealTime":
                        priority.RealTimePriority(from, processesListBox, label, searchBox, pid);
                        break;
                }
            }
        }
        /// <summary>
        /// Read the settings from the specified file path and apply the IO priority settings to the processes in the list box.
        /// </summary>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        /// <param name="from"></param>
        public void ReadSettingsIOPriority(
    DoubleBufferedListView processesListBox,
    Label label,
    TextBox searchBox,
    Form from)
        {
            if (!File.Exists(_logFilePath))
                return;

            ProcessData[]? settings;
            try
            {
                settings = Json.JsonManage.ReadJsonFromFile<ProcessData[]>(_logFilePath);
            }
            catch
            {
                return;
            }

            if (settings == null || settings.Length == 0)
                return;

            var processLookup = processesListBox.Items
                .Cast<ListViewItem>()
                .Where(item => item.SubItems.Count > 1 && !string.IsNullOrWhiteSpace(item.SubItems[0].Text))
                .GroupBy(item => item.SubItems[0].Text, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().SubItems[1].Text,
                    StringComparer.OrdinalIgnoreCase);

            var ioPriority = new IOPriority();

            foreach (var setting in settings)
            {
                if (setting == null ||
                    string.IsNullOrWhiteSpace(setting.ProcessName) ||
                    string.IsNullOrWhiteSpace(setting.IOProperty))
                {
                    continue;
                }

                if (!processLookup.TryGetValue(setting.ProcessName, out var pid))
                    continue;

                switch (setting.IOProperty)
                {
                    case "Normal":
                        ioPriority.IONormalPriority(from, processesListBox, label, searchBox, pid);
                        break;

                    case "Low":
                        ioPriority.IOLowPriority(from, processesListBox, label, searchBox, pid);
                        break;

                    case "High":
                        ioPriority.IOHighPriority(from, processesListBox, label, searchBox, pid);
                        break;

                    case "VeryLow":
                        ioPriority.IOVeryLowPriority(from, processesListBox, label, searchBox, pid);
                        break;
                }
            }
        }
    }
}
