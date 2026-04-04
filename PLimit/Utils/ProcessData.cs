namespace PLimit
{
    public class ProcessData
    {
        public string ProcessName { get; set; }
        public string Boosted { get; set; }

        public string IOProperty { get; set; }
        public string Property { get; set; }
        public string Affinity { get; set; }
        public string Efficiency { get; set; }
        public override bool Equals(object obj)
        {
            return obj is ProcessData details &&
                   ProcessName == details.ProcessName &&
                   Boosted == details.Boosted &&
                   IOProperty == details.IOProperty &&
                   Property == details.Property &&
                   Affinity == details.Affinity &&
                   Efficiency == details.Efficiency;
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
            return hashCode;
        }
    }
}
