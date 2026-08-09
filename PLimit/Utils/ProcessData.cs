namespace PLimit
{
    public class ProcessData
    {
        private string _processName = string.Empty;
        private string _boosted = string.Empty;
        private string _ioProperty = string.Empty;
        private string _property = string.Empty;
        private string _affinity = string.Empty;
        private string _efficiency = string.Empty;
        private string _wdptb = string.Empty;

        public string ProcessName { get => _processName; set => _processName = value ?? string.Empty; }
        public string Boosted { get => _boosted; set => _boosted = value ?? string.Empty; }
        public string IOProperty { get => _ioProperty; set => _ioProperty = value ?? string.Empty; }
        public string Property { get => _property; set => _property = value ?? string.Empty; }
        public string Affinity { get => _affinity; set => _affinity = value ?? string.Empty; }
        public string Efficiency { get => _efficiency; set => _efficiency = value ?? string.Empty; }
        public string Wdptb { get => _wdptb; set => _wdptb = value ?? string.Empty; }
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
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Boosted);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(IOProperty);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Property);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Affinity);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Efficiency);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Wdptb);
            return hashCode;
        }
    }
}
