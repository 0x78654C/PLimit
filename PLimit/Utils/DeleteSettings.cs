namespace PLimit.Utils
{
    public class DeleteSettings
    {
        public DeleteSettings() { }

        /// <summary>
        /// Deletes the settings of the selected process from the JSON file and refreshes the process list in the UI.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="processesListBox"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        /// <param name="jsonFilePath"></param>
        public void DeleteSettingsApp(Form from, DoubleBufferedListView processesListBox, Label label, TextBox searchBox, string jsonFilePath)
        {
            if (processesListBox.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select a process first.", "Process Limiter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string processName = processesListBox.SelectedItems[0].SubItems[0].Text;
            bool removed = false;
            try
            {
                if (!File.Exists(jsonFilePath))
                {
                    MessageBox.Show("No process settings have been saved yet.", "Process Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Json.JsonManage.UpdateJsonFileParameter<List<ProcessData>>(jsonFilePath, data =>
                {
                    removed = data.RemoveAll(item =>
                        item == null || string.Equals(item.ProcessName, processName, StringComparison.OrdinalIgnoreCase)) > 0;
                });

                if (!removed)
                {
                    MessageBox.Show($"There are no settings saved for: {processName}", "Process Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to delete the saved settings: {ex.Message}", "Process Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            from.BeginInvoke(new Action(() =>
            {
                var utils = new Utils();
                utils.RefreshProcessList(from, processesListBox, label);
                utils.SearchProcess(searchBox, processesListBox);
            }));
        }
    }
}
