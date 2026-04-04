namespace PLimit.Utils
{
    public class GlobalVars
    {
        public static string LogDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");
        public static string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings", "processes.json");
    }
}
