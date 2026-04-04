namespace PLimit.Utils
{
    public class StoreSettings
    {
        public StoreSettings() { }

        /// <summary>
        /// Updates the specified setting for a process in the configuration file. If the process does not exist, it is added
        /// with the provided setting.
        /// </summary>
        /// <remarks>If the process specified by processName does not exist in the configuration, it is created and the
        /// setting is applied. The configuration file is updated on disk after the change.</remarks>
        /// <param name="settingType">The type of setting to update. Must be a valid value of the SettingType enumeration.</param>
        /// <param name="processName">The name of the process for which the setting is being updated. Cannot be null.</param>
        /// <param name="value">The value to assign to the specified setting for the process.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if settingType is not a valid value of the SettingType enumeration.</exception>
        public void UpdateSetting(SettingType settingType, string processName, string value)
        {
            if (!Directory.Exists(GlobalVars.LogDirPath))
                Directory.CreateDirectory(GlobalVars.LogDirPath);

            Json.JsonManage.UpdateJsonFileParameter<List<ProcessData>>(GlobalVars.LogFilePath, data =>
            {
                data ??= new List<ProcessData>();

                var processData = data.FirstOrDefault(d => d.ProcessName == processName);

                if (processData == null)
                {
                    processData = new ProcessData
                    {
                        ProcessName = processName
                    };

                    data.Add(processData);
                }

                switch (settingType)
                {
                    case SettingType.Boosted:
                        processData.Boosted = value;
                        break;

                    case SettingType.IOPriority:
                        processData.IOProperty = value;
                        break;

                    case SettingType.Priority:
                        processData.Property = value;
                        break;

                    case SettingType.Affinity:
                        processData.Affinity = value;
                        break;

                    case SettingType.Efficiency:
                        processData.Efficiency = value;
                        break;

                    case SettingType.Wdptb:
                        processData.Wdptb = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(settingType), settingType, null);
                }
            });
        }

        /// <summary>
        /// Retrieves the specified setting value for a given process name.
        /// </summary>
        /// <param name="processName">The name of the process for which to retrieve the setting. Cannot be null.</param>
        /// <param name="settingType">The type of setting to retrieve for the specified process.</param>
        /// <returns>The value of the requested setting as a string, or an empty string if the process is not found. Returns null
        /// if the specified setting type is not recognized.</returns>
        public string? GetSetting(string processName, SettingType settingType)
        {
            var data = Json.JsonManage.ReadJsonFromFile<ProcessData[]>(GlobalVars.LogFilePath);
            var processData = data.FirstOrDefault(d => d.ProcessName == processName);
            if (processData == null) { return ""; }
            return settingType switch
            {
                SettingType.ProcessName => processData.ProcessName,
                SettingType.Boosted => processData.Boosted,
                SettingType.IOPriority => processData.IOProperty,
                SettingType.Priority => processData.Property,
                SettingType.Affinity => processData.Affinity,
                SettingType.Efficiency => processData.Efficiency,
                _ => null
            };
        }

        /// <summary>
        /// Deletes all settings associated with the specified process name from the log file.
        /// </summary>
        /// <remarks>If no settings exist for the specified process name, the method performs no action.
        /// This operation is not reversible.</remarks>
        /// <param name="processName">The name of the process whose settings should be deleted. Cannot be null or empty.</param>
        public void DeleteSetting(string processName)
           => Json.JsonManage.DeleteJsonData<ProcessData>(GlobalVars.LogFilePath, data => data.Where(d => d.ProcessName == processName));

        /// <summary>
        /// Specifies the types of process settings that can be configured or queried.
        /// </summary>
        /// <remarks>This enumeration is used to identify individual process settings, such as process
        /// name, priority, or CPU affinity, when managing or retrieving process configuration values. The meaning and
        /// valid usage of each value depend on the context in which the enumeration is used.</remarks>
        public enum SettingType
        {
            ProcessName,
            Boosted,
            IOPriority,
            Priority,
            Affinity,
            Efficiency,
            Wdptb
        }

    }
}
