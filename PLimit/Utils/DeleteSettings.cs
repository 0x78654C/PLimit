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
                        data.Remove(processData);
                    }
                    else
                    {
                        MessageBox.Show($"Selected process not found in settings: {processesListBox.SelectedItems[0].SubItems[0].Text}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while deleting the settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
