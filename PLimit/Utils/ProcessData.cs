namespace PLimit
{
    public class ProcessData
    {
        public string ProcessName { get; set; } = string.Empty;
        public string Boosted { get; set; } = string.Empty;

        public string IOProperty { get; set; } = string.Empty;
        public string Property { get; set; } = string.Empty;
        public string Affinity { get; set; } = string.Empty;
        public string Efficiency { get; set; } = string.Empty;
        public string Wdptb { get; set; } = string.Empty;
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
