using PLimit.Json;

namespace PLimit.Utils;

public sealed class CpuLimitRule
{
    public int ThresholdPercent { get; set; } = 25;
    public int OverSeconds { get; set; } = 5;
    public int CoreCount { get; set; } = 1;
    public int DurationSeconds { get; set; } = 15;

    public bool IsValid => ThresholdPercent is >= 1 and <= 100 && OverSeconds is >= 1 and <= 300
        && CoreCount is >= 1 and <= 64 && DurationSeconds is >= 1 and <= 3600;
}

public sealed class AutomationRule
{
    public string ProcessName { get; set; } = "";
    public bool ExcludeForegroundBoost { get; set; }
    public bool ExcludeBalance { get; set; }
    public CpuLimitRule? CpuLimit { get; set; }
}

public sealed class AutomationSettings
{
    public bool ForegroundBoostEnabled { get; set; }
    public bool BalanceEnabled { get; set; }
    public int BalanceSystemThreshold { get; set; } = 80;
    public int BalanceProcessThreshold { get; set; } = 15;
    public int BalanceOverSeconds { get; set; } = 3;
    public List<AutomationRule> Rules { get; set; } = new();

    public void Validate()
    {
        if (BalanceSystemThreshold is < 20 or > 100 || BalanceProcessThreshold is < 1 or > 100
            || BalanceOverSeconds is < 1 or > 60 || Rules == null)
            throw new InvalidDataException("Invalid background balancing settings.");
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var rule in Rules)
        {
            if (rule == null || string.IsNullOrWhiteSpace(rule.ProcessName)
                || !names.Add(NormalizeName(rule.ProcessName)) || rule.CpuLimit is { IsValid: false })
                throw new InvalidDataException("Invalid or duplicate process automation rule.");
        }
    }

    internal static string NormalizeName(string name) =>
        name.Trim().EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? name.Trim()[..^4] : name.Trim();

    public AutomationRule? FindRule(string name) => Rules.LastOrDefault(rule =>
        string.Equals(NormalizeName(rule.ProcessName), NormalizeName(name), StringComparison.OrdinalIgnoreCase));

    public static string FilePath => Path.Combine(GlobalVars.LogDirPath, "automation.json");

    public static AutomationSettings Load(string? path = null)
    {
        path ??= FilePath;
        var settings = File.Exists(path) ? JsonManage.ReadJsonFromFile<AutomationSettings>(path) : new();
        settings.Validate();
        return settings;
    }

    public void Save(string? path = null)
    {
        Validate();
        JsonManage.CreateJsonFile(path ?? FilePath, this);
    }
}
