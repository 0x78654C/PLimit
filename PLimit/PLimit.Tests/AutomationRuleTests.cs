using System.Text.Json;
using PLimit.Json;
using PLimit.Utils;
using Xunit;

namespace PLimit.Tests;

public sealed class AutomationRuleTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "plimit-rules-" + Guid.NewGuid().ToString("N"));
    private string ExportPath => Path.Combine(_directory, "rules.json");

    [Theory]
    [InlineData("chrome", "CHROME.EXE", true)]
    [InlineData(" game*.exe ", "GameClient", true)]
    [InlineData("game*", "game", true)]
    [InlineData("game*", "mygame", false)]
    [InlineData("worker?", "worker1", true)]
    [InlineData("worker?", "worker", false)]
    [InlineData("worker?", "worker12", false)]
    [InlineData("*helper*", "BrowserHelperGpu", true)]
    [InlineData("a*b*c", "axbybzc", true)]
    [InlineData("a*b*c", "axbybzcd", false)]
    [InlineData("*", "anything", true)]
    [InlineData("**.exe", "anything", true)]
    [InlineData("app[1]", "app[1]", true)]
    [InlineData("app[1]", "app1", false)]
    [InlineData("a+b", "a+b", true)]
    public void Wildcards_MatchEntireProcessName(string pattern, string name, bool expected) =>
        Assert.Equal(expected, ProcessRuleMatcher.Matches(pattern, name));

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".exe")]
    [InlineData("C:\\app.exe")]
    [InlineData("folder/app")]
    [InlineData("app\n*")]
    [InlineData("app|other")]
    public void InvalidPattern_IsRejected(string pattern) =>
        Assert.Throws<InvalidDataException>(() => new AutomationSettings { Rules = [new() { ProcessName = pattern }] }.Validate());

    [Fact]
    public void LegacyRules_DefaultToEnabledAndRetainLimiterBehavior()
    {
        var settings = JsonSerializer.Deserialize<AutomationSettings>("""
            {"Rules":[{"ProcessName":"app","CpuLimit":{"ThresholdPercent":30,"OverSeconds":5,"CoreCount":2,"DurationSeconds":10}}]}
            """)!;
        settings.Validate();
        var rule = Assert.Single(settings.Rules);
        Assert.True(rule.Enabled);
        Assert.Equal(0, rule.CpuLimit!.CooldownSeconds);
        Assert.False(rule.CpuLimit.OnlyWhenBackground);
        Assert.Same(rule, settings.ResolveRule("APP.exe"));
    }

    [Fact]
    public void FirstEnabledMatchWinsWithoutMergingActions()
    {
        var exact = new AutomationRule { ProcessName = "gameclient", ExcludeBalance = true };
        var wildcard = new AutomationRule { ProcessName = "game*", CpuLimit = new() };
        var settings = new AutomationSettings { Rules = [exact, wildcard] };
        Assert.Same(exact, settings.ResolveRule("GameClient"));
        Assert.Null(settings.ResolveRule("GameClient")!.CpuLimit);
        exact.Enabled = false;
        Assert.Same(wildcard, settings.ResolveRule("GameClient"));
        Assert.Same(exact, settings.FindRule("GameClient")); // Editing still finds paused rules.
        settings.Rules.Reverse();
        exact.Enabled = true;
        Assert.Same(wildcard, settings.ResolveRule("GameClient"));
    }

    [Fact]
    public void EditingPreservesOrderAndInvalidEditDoesNotMutateRules()
    {
        var first = new AutomationRule { ProcessName = "game*" };
        var settings = new AutomationSettings { Rules = [first, new() { ProcessName = "*" }] };
        var edited = new AutomationRule { ProcessName = "game*", Enabled = false };
        settings.SetRule(edited, first);
        Assert.Same(edited, settings.Rules[0]);
        Assert.Throws<InvalidDataException>(() => settings.SetRule(new() { ProcessName = "**.exe" }, edited));
        Assert.Same(edited, settings.Rules[0]);
        Assert.Equal(2, settings.Rules.Count);
        settings.SetRule(new() { ProcessName = "gameclient" }, insertFirst: true);
        Assert.Equal("gameclient", settings.Rules[0].ProcessName);
    }

    [Fact]
    public void ExportImport_PreservesRulesOrderAndNewFieldsWithoutChangingGlobalOptions()
    {
        var source = new AutomationSettings { Rules = [new()
        {
            ProcessName = "game*", Enabled = false, ExcludeBalance = true,
            CpuLimit = new() { CooldownSeconds = 30, OnlyWhenBackground = true }
        }, new() { ProcessName = "*", ExcludeForegroundBoost = true }] };
        source.ExportRules(ExportPath);
        var destination = new AutomationSettings { ForegroundBoostEnabled = true, BalanceEnabled = true };
        Assert.Equal((2, 0), destination.ImportRules(ExportPath));
        Assert.True(destination.ForegroundBoostEnabled && destination.BalanceEnabled);
        Assert.Equal("game*", destination.Rules[0].ProcessName);
        Assert.False(destination.Rules[0].Enabled);
        Assert.Equal(30, destination.Rules[0].CpuLimit!.CooldownSeconds);
        Assert.True(destination.Rules[0].CpuLimit!.OnlyWhenBackground);
        Assert.Equal("*", destination.Rules[1].ProcessName);
    }

    [Fact]
    public void Import_SkipsExistingPatternsAndAppendsWithoutChangingTheirPrecedence()
    {
        new AutomationSettings { Rules = [new() { ProcessName = "GAME**.exe" }, new() { ProcessName = "worker?" }] }.ExportRules(ExportPath);
        var existing = new AutomationRule { ProcessName = "game*", ExcludeBalance = true };
        var settings = new AutomationSettings { Rules = [existing] };
        Assert.Equal((1, 1), settings.ImportRules(ExportPath));
        Assert.Same(existing, settings.Rules[0]);
        Assert.True(settings.Rules[0].ExcludeBalance);
        Assert.Equal("worker?", settings.Rules[1].ProcessName);
    }

    [Fact]
    public void Import_InvalidLaterRuleLeavesEntireDestinationUnchanged()
    {
        JsonManage.CreateJsonFile(ExportPath, new AutomationRuleFile { Rules = [new() { ProcessName = "new" },
            new() { ProcessName = "broken", CpuLimit = new() { CooldownSeconds = -1 } }] });
        var existing = new AutomationRule { ProcessName = "original" };
        var settings = new AutomationSettings { Rules = [existing] };
        Assert.Throws<InvalidDataException>(() => settings.ImportRules(ExportPath));
        Assert.Same(existing, Assert.Single(settings.Rules));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"Rules\":[]}")]
    [InlineData("{\"Format\":\"other\",\"Version\":1,\"Rules\":[]}")]
    [InlineData("{\"Format\":\"PLimit.AutomationRules\",\"Version\":2,\"Rules\":[]}")]
    public void Import_RejectsWrongOrMissingFormat(string json)
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(ExportPath, json);
        Assert.ThrowsAny<Exception>(() => new AutomationSettings().ImportRules(ExportPath));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true);
    }
}
