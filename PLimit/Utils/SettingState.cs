namespace PLimit.Utils
{
    /// <summary>
    /// Canonical text values used for settings that can be enabled or disabled.
    /// Legacy JSON files used Boolean text, so parsing continues to accept it.
    /// </summary>
    public static class SettingState
    {
        public const string Enabled = "Enabled";
        public const string Disabled = "Disabled";

        public static string FromBoolean(bool enabled) => enabled ? Enabled : Disabled;

        public static bool TryParse(string? value, out bool enabled)
        {
            if (string.Equals(value, Enabled, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase))
            {
                enabled = true;
                return true;
            }

            if (string.Equals(value, Disabled, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, bool.FalseString, StringComparison.OrdinalIgnoreCase))
            {
                enabled = false;
                return true;
            }

            enabled = false;
            return false;
        }
    }
}
