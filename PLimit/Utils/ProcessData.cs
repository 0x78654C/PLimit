namespace PLimit
{
    using System.Text.Json.Serialization;

    public class ProcessData
    {
        private string _processName = string.Empty;
        private string? _boosted;
        private string? _ioProperty;
        private string? _property;
        private string? _affinity;
        private string? _efficiency;
        private string? _wdptb;

        public string ProcessName { get => _processName; set => _processName = value ?? string.Empty; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Boosted { get => _boosted; set => _boosted = NormalizeOptionalSetting(value); }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? IOProperty { get => _ioProperty; set => _ioProperty = NormalizeOptionalSetting(value); }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Property { get => _property; set => _property = NormalizeOptionalSetting(value); }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Affinity { get => _affinity; set => _affinity = NormalizeOptionalSetting(value); }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Efficiency { get => _efficiency; set => _efficiency = NormalizeOptionalSetting(value); }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Wdptb { get => _wdptb; set => _wdptb = NormalizeOptionalSetting(value); }

        private static string? NormalizeOptionalSetting(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value;
        public override bool Equals(object? obj)
        {
            return obj is ProcessData details &&
                   ProcessName == details.ProcessName &&
                   Boosted == details.Boosted &&
                   IOProperty == details.IOProperty &&
                   Property == details.Property &&
                   Affinity == details.Affinity &&
                   Efficiency == details.Efficiency &&
                   Wdptb == details.Wdptb;
        }

        public override int GetHashCode()
        {
            int hashCode = -1670917873;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProcessName);
            hashCode = hashCode * -1521134295 + (Boosted?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (IOProperty?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Property?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Affinity?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Efficiency?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Wdptb?.GetHashCode() ?? 0);
            return hashCode;
        }
    }
}
