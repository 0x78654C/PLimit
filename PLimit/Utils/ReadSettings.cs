namespace PLimit.Utils
{
    public class ReadSettings
    {

        public ReadSettings() { }

        /// <summary>
        /// Read the settings from the specified file path and apply the boost settings to the processes in the list box.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        /// <param name="from"></param>
        public void ReadSettingsBoost(DoubleBufferedListView processesListBox, Label label, TextBox searchBox, Form from)
        {
            if (!Directory.Exists(GlobalVars.LogDirPath))
                return;
            var settings = Json.JsonManage.ReadJsonFromFile<ProcessData[]>(GlobalVars.LogFilePath).ToList();
            if (settings == null)
                return;

            foreach (var setting in settings)
            {
                foreach (var item in processesListBox.Items.Cast<ListViewItem>())
                {
                    if (item.SubItems[0].Text == setting.ProcessName)
                    {
                        if (string.IsNullOrEmpty(setting.Boosted))
                            continue;
                        var boost = new Boost();
                        if (setting.Boosted == "True")
                            boost.SetBoost(from, processesListBox, label, searchBox, true, item.SubItems[1].Text);
                        else
                            boost.SetBoost(from, processesListBox, label, searchBox, false, item.SubItems[1].Text);
                    }
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
        public void ReadSettingsEfficiency(DoubleBufferedListView processesListBox, Label label, TextBox searchBox, Form from)
        {
            if (!Directory.Exists(GlobalVars.LogDirPath))
                return;
            var settings = Json.JsonManage.ReadJsonFromFile<ProcessData[]>(GlobalVars.LogFilePath).ToList();
            if (settings == null)
                return;
            foreach (var setting in settings)
            {
                foreach (var item in processesListBox.Items.Cast<ListViewItem>())
                {
                    if (item.SubItems[0].Text == setting.ProcessName)
                    {
                        if (string.IsNullOrEmpty(setting.Efficiency))
                            continue;
                        var efficiency = new Efficiency();
                        if (setting.Efficiency == "True")
                            efficiency.EnableEfficiency(from, processesListBox, label, searchBox, item.SubItems[1].Text);
                        else
                            efficiency.DisableEfficiency(from, processesListBox, label, searchBox, item.SubItems[1].Text);
                    }
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
        public void ReadSettingsAffinity(DoubleBufferedListView processesListBox, Label label, TextBox searchBox, Form from)
        {
            if (!Directory.Exists(GlobalVars.LogDirPath))
                return;
            var settings = Json.JsonManage.ReadJsonFromFile<ProcessData[]>(GlobalVars.LogFilePath).ToList();
            if (settings == null)
                return;
            foreach (var setting in settings)
            {
                foreach (var item in processesListBox.Items.Cast<ListViewItem>())
                {
                    if (item.SubItems[0].Text == setting.ProcessName)
                    {
                        var affinity = new Affinity();
                        if (!string.IsNullOrEmpty(setting.Affinity))
                            affinity.SetAffinity(from, processesListBox, null, label, searchBox, null, setting.Affinity, item.SubItems[1].Text);
                    }
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
        public void ReadSettingsPriority(DoubleBufferedListView processesListBox, Label label, TextBox searchBox, Form from)
        {
            if (!Directory.Exists(GlobalVars.LogDirPath))
                return;
            var settings = Json.JsonManage.ReadJsonFromFile<ProcessData[]>(GlobalVars.LogFilePath).ToList();
            if (settings == null)
                return;
            foreach (var setting in settings)
            {
                foreach (var item in processesListBox.Items.Cast<ListViewItem>())
                {
                    if (item.SubItems[0].Text == setting.ProcessName)
                    {
                        if (string.IsNullOrEmpty(setting.Property))
                            continue;
                        if (setting.Property == "Normal")
                        {
                            var priority = new PriorityProcess();
                            priority.NormalPriority(from, processesListBox, label, searchBox, item.SubItems[1].Text);
                        }
                        if (setting.Property == "High")
                        {
                            var priority = new PriorityProcess();
                            priority.HighPriority(from, processesListBox, label, searchBox, item.SubItems[1].Text);
                        }
                        if (setting.Property == "AboveNormal")
                        {
                            var priority = new PriorityProcess();
                            priority.AboveNormalPriority(from, processesListBox, label, searchBox, item.SubItems[1].Text);
                        }
                        if (setting.Property == "BelowNormal")
                        {
                            var priority = new PriorityProcess();
                            priority.BelowNormalPriority(from, processesListBox, label, searchBox, item.SubItems[1].Text);
                        }
                        if (setting.Property == "RealTime")
                        {
                            var priority = new PriorityProcess();
                            priority.RealTimePriority(from, processesListBox, label, searchBox, item.SubItems[1].Text);
                        }
                    }
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
        public void ReadSettingsIOPriority(DoubleBufferedListView processesListBox, Label label, TextBox searchBox, Form from)
        {
            if (!Directory.Exists(GlobalVars.LogDirPath))
                return;
            var settings = Json.JsonManage.ReadJsonFromFile<ProcessData[]>(GlobalVars.LogFilePath).ToList();
            if (settings == null)
                return;
            foreach (var setting in settings)
            {
                foreach (var item in processesListBox.Items.Cast<ListViewItem>())
                {
                    if (item.SubItems[0].Text == setting.ProcessName)
                    {
                        if (string.IsNullOrEmpty(setting.IOProperty))
                            continue;
                        if (setting.IOProperty == "Normal")
                        {
                            var ioPriority = new IOPriority();
                            ioPriority.IONormalPriority(from, processesListBox, label, searchBox, item.SubItems[1].Text);
                        }
                        if (setting.IOProperty == "Low")
                        {
                            var ioPriority = new IOPriority();
                            ioPriority.IOLowPriority(from, processesListBox, label, searchBox, item.SubItems[1].Text);
                        }
                        if (setting.IOProperty == "High")
                        {
                            var ioPriority = new IOPriority();
                            ioPriority.IOHighPriority(from, processesListBox, label, searchBox, item.SubItems[1].Text);
                        }
                        if (setting.IOProperty == "VeryLow")
                        {
                            var ioPriority = new IOPriority();
                            ioPriority.IOVeryLowPriority(from, processesListBox, label, searchBox, item.SubItems[1].Text);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Show the settings of the selected process in the list box by reading the settings from the specified file path and displaying them in a message box.
        /// </summary>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        /// <param name="from"></param>
        /// <param name="jsonFilePath"></param>
        public void ShowSettings(DoubleBufferedListView processesListBox, Label label, TextBox searchBox, Form from, string jsonFilePath)
        {
            try
            {
                if (!File.Exists(jsonFilePath))
                    return;
                Json.JsonManage.UpdateJsonFileParameter<List<ProcessData>>(jsonFilePath, data =>
                {
                    if (data == null)
                        return;
                    var processData = data.FirstOrDefault(d => d.ProcessName == processesListBox.SelectedItems[0].SubItems[0].Text);
                    if (processData != null)
                    {
                        var message = $"Process Name: {processData.ProcessName}\n" +
                                      $"Boosted: {processData.Boosted}\n" +
                                      $"Efficiency: {processData.Efficiency}\n" +
                                      $"Affinity: {processData.Affinity}\n" +
                                      $"Priority: {processData.Property}\n" +
                                      $"IO Priority: {processData.IOProperty}";
                        MessageBox.Show(message, "Process Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"There are no settings saved for: {processesListBox.SelectedItems[0].SubItems[0].Text}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while deleting the settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}