using PLimit.Json;

namespace PLimit.Utils;

public sealed class CpuLimitRule
{
    public int ThresholdPercent { get; set; } = 25;
    public int OverSeconds { get; set; } = 5;
    public int CoreCount { get; set; } = 1;
    public int DurationSeconds { get; set; } = 15;
    public int CooldownSeconds { get; set; }
    public bool OnlyWhenBackground { get; set; }

    public bool IsValid => ThresholdPercent is >= 1 and <= 100 && OverSeconds is >= 1 and <= 300
        && CoreCount is >= 1 and <= 64 && DurationSeconds is >= 1 and <= 3600
        && CooldownSeconds is >= 0 and <= 3600;

    internal CpuLimitRule Copy() => (CpuLimitRule)MemberwiseClone();

    internal bool SameAs(CpuLimitRule? other) => other != null
        && ThresholdPercent == other.ThresholdPercent && OverSeconds == other.OverSeconds
        && CoreCount == other.CoreCount && DurationSeconds == other.DurationSeconds
        && CooldownSeconds == other.CooldownSeconds && OnlyWhenBackground == other.OnlyWhenBackground;
}

public sealed class AutomationRule
{
    public string ProcessName { get; set; } = "";
    public bool Enabled { get; set; } = true;
    public bool ExcludeForegroundBoost { get; set; }
    public bool ExcludeBalance { get; set; }
    public CpuLimitRule? CpuLimit { get; set; }

    public bool Matches(string processName) => ProcessRuleMatcher.Matches(ProcessName, processName);

    internal AutomationRule Copy() => new()
    {
        ProcessName = ProcessName, Enabled = Enabled, ExcludeForegroundBoost = ExcludeForegroundBoost,
        ExcludeBalance = ExcludeBalance, CpuLimit = CpuLimit?.Copy()
    };
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
            if (rule == null) throw new InvalidDataException("A process rule cannot be null.");
            ProcessRuleMatcher.Validate(rule.ProcessName);
            if (!names.Add(ProcessRuleMatcher.NormalizePattern(rule.ProcessName)))
                throw new InvalidDataException($"A rule already exists for '{rule.ProcessName}'. Edit the existing rule.");
            if (rule.CpuLimit is { IsValid: false })
                throw new InvalidDataException($"Invalid CPU limiter values for '{rule.ProcessName}'.");
        }
    }

    internal static string NormalizeName(string name) =>
        name.Trim().EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? name.Trim()[..^4] : name.Trim();

    public AutomationRule? FindRule(string name) => Rules.LastOrDefault(rule =>
        string.Equals(ProcessRuleMatcher.NormalizePattern(rule.ProcessName), ProcessRuleMatcher.NormalizePattern(name), StringComparison.OrdinalIgnoreCase));

    /// <summary>The first enabled matching rule wins as a whole; actions are never merged across rules.</summary>
    public AutomationRule? ResolveRule(string processName) => Rules.FirstOrDefault(rule => rule.Enabled && rule.Matches(processName));

    public void SetRule(AutomationRule rule, AutomationRule? replace = null, bool insertFirst = false)
    {
        var candidate = new List<AutomationRule>(Rules);
        int index = replace == null ? -1 : candidate.IndexOf(replace);
        if (replace != null && index < 0) throw new InvalidOperationException("The rule being edited no longer exists.");
        if (index >= 0) candidate[index] = rule;
        else if (insertFirst) candidate.Insert(0, rule);
        else candidate.Add(rule);
        new AutomationSettings { Rules = candidate }.Validate();
        Rules = candidate;
    }

    public void ExportRules(string path)
    {
        Validate();
        JsonManage.CreateJsonFile(path, new AutomationRuleFile { Rules = Rules.Select(rule => rule.Copy()).ToList() });
    }

    /// <summary>Validate the entire import before changing anything. Existing patterns retain precedence.</summary>
    public (int Added, int Skipped) ImportRules(string path)
    {
        var file = JsonManage.ReadJsonFromFile<AutomationRuleFile>(path);
        if (file.Format != AutomationRuleFile.FormatName || file.Version != 1 || file.Rules == null)
            throw new InvalidDataException("This is not a supported PLimit rules export (version 1).");
        new AutomationSettings { Rules = file.Rules }.Validate();
        var merged = new List<AutomationRule>(Rules);
        var patterns = Rules.Select(rule => ProcessRuleMatcher.NormalizePattern(rule.ProcessName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        int skipped = 0;
        foreach (var rule in file.Rules)
        {
            if (!patterns.Add(ProcessRuleMatcher.NormalizePattern(rule.ProcessName))) { skipped++; continue; }
            merged.Add(rule.Copy());
        }
        new AutomationSettings { Rules = merged }.Validate();
        int added = merged.Count - Rules.Count;
        Rules = merged;
        return (added, skipped);
    }

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

internal sealed class AutomationRuleFile
{
    internal const string FormatName = "PLimit.AutomationRules";
    [System.Text.Json.Serialization.JsonRequired]
    public string Format { get; set; } = FormatName;
    [System.Text.Json.Serialization.JsonRequired]
    public int Version { get; set; } = 1;
    [System.Text.Json.Serialization.JsonRequired]
    public List<AutomationRule>? Rules { get; set; }
}
