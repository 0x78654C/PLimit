namespace PLimit.Utils
{
    internal sealed record ProcessSnapshot(
        string Name,
        int ProcessId,
        string Priority,
        int? CpuCount,
        string IoPriority,
        string Boost,
        string EfficiencyMode,
        bool HasStoredSettings,
        string ThreadPriorityBoost,
        string User)
    {
        public bool Matches(string query)
        {
            query = query.Trim();
            return query.Length == 0
                || Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                || ProcessId.ToString().Contains(query, StringComparison.OrdinalIgnoreCase);
        }

        public ListViewItem ToListViewItem() => new(new[]
        {
            Name,
            ProcessId.ToString(),
            Priority,
            CpuCount?.ToString() ?? "Unknown",
            IoPriority,
            Boost,
            EfficiencyMode,
            HasStoredSettings ? "Yes" : "No",
            ThreadPriorityBoost,
            User
        });
    }
}
