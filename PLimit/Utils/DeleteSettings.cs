namespace PLimit.Utils
{
    public class DeleteSettings
    {
        public DeleteSettings() { }
        public void DeleteSettingsApp(string processName, string jsonFilePath)
        {
            try
            {
                if (!File.Exists(jsonFilePath))
                    return;
                Json.JsonManage.UpdateJsonFileParameter<List<ProcessData>>(jsonFilePath, data =>
                {
                    if (data == null)
                        return;
                    var processData = data.FirstOrDefault(d => d.ProcessName == processName);
                    if (processData != null)
                    {
                        data.Remove(processData);
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
