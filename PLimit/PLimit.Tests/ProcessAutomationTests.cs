using System.Diagnostics;
using PLimit.Utils;
using Xunit;

namespace PLimit.Tests;

public class ProcessAutomationTests
{
    private readonly List<string> _events = new();
    private readonly ProcessAutomationEngine _engine;
    private readonly AutomationSettings _settings = new();
    private readonly Dictionary<string, ProcessData> _saved = new(StringComparer.OrdinalIgnoreCase);
    public ProcessAutomationTests() => _engine = new(_events.Add);

    private void Tick(double seconds, FakeProcess process, int foreground = 0, double? system = 95) =>
        _engine.Tick([process], foreground, system, TimeSpan.FromSeconds(seconds), _settings, _saved);

    [Fact]
    public void ForegroundBoost_RestoresOnFocusChangeAndDisable()
    {
        var process = new FakeProcess();
        _settings.ForegroundBoostEnabled = true;
        Tick(0, process, process.Identity.Pid);
        Assert.Equal(ProcessPriorityClass.AboveNormal, process.Priority);
        Tick(1, process);
        Assert.Equal(ProcessPriorityClass.Normal, process.Priority);
        Tick(2, process, process.Identity.Pid);
        _settings.ForegroundBoostEnabled = false;
        Tick(3, process, process.Identity.Pid);
        Assert.Equal(ProcessPriorityClass.Normal, process.Priority);
        Assert.Equal(0, _engine.ActiveBoosts);
    }

    [Theory]
    [InlineData(ProcessPriorityClass.High)]
    [InlineData(ProcessPriorityClass.RealTime)]
    [InlineData(ProcessPriorityClass.AboveNormal)]
    [InlineData(ProcessPriorityClass.Idle)]
    [InlineData(ProcessPriorityClass.BelowNormal)]
    public void AutomaticPriority_DoesNotOverrideNonNormalPriority(ProcessPriorityClass priority)
    {
        var process = new FakeProcess { Priority = priority };
        _settings.ForegroundBoostEnabled = _settings.BalanceEnabled = true;
        Tick(0, process, process.Identity.Pid);
        Tick(1, process);
        Tick(10, process);
        Assert.Equal(priority, process.Priority);
    }

    [Fact]
    public void Balance_RequiresSustainedLoadAndQuietPeriod()
    {
        var process = new FakeProcess();
        _settings.BalanceEnabled = true;
        Tick(0, process);
        Tick(2, process);
        Assert.Equal(ProcessPriorityClass.Normal, process.Priority);
        Tick(3, process);
        Assert.Equal(ProcessPriorityClass.BelowNormal, process.Priority);
        Tick(4, process, system: 75); // Between activation and release thresholds.
        Assert.Equal(ProcessPriorityClass.BelowNormal, process.Priority);
        Tick(5, process, system: 30);
        Tick(7, process, system: 30);
        Assert.Equal(ProcessPriorityClass.BelowNormal, process.Priority);
        Tick(8, process, system: 30);
        Assert.Equal(ProcessPriorityClass.Normal, process.Priority);
    }

    [Fact]
    public void Balance_ForegroundIsRestoredImmediatelyThenBoosted()
    {
        var process = new FakeProcess();
        _settings.BalanceEnabled = _settings.ForegroundBoostEnabled = true;
        Tick(0, process);
        Tick(3, process);
        Assert.Equal(ProcessPriorityClass.BelowNormal, process.Priority);
        Tick(4, process, process.Identity.Pid);
        Assert.Equal(ProcessPriorityClass.AboveNormal, process.Priority);
        _engine.RestoreAll([process]);
        Assert.Equal(ProcessPriorityClass.Normal, process.Priority);
    }

    [Fact]
    public void ExclusionsAndSavedPriorityTakePrecedence()
    {
        var process = new FakeProcess();
        _settings.ForegroundBoostEnabled = _settings.BalanceEnabled = true;
        _settings.Rules.Add(new() { ProcessName = "APP.EXE", ExcludeForegroundBoost = true, ExcludeBalance = true });
        Tick(0, process, process.Identity.Pid);
        Tick(5, process);
        Tick(10, process);
        Assert.Equal(ProcessPriorityClass.Normal, process.Priority);
        _settings.Rules.Clear();
        _saved[process.Name] = new() { ProcessName = process.Name, Property = "High" };
        Tick(15, process, process.Identity.Pid);
        Assert.Equal(ProcessPriorityClass.High, process.Priority);
    }

    [Fact]
    public void SavedEfficiency_PreventsBoostAndBalance()
    {
        var process = new FakeProcess();
        _settings.ForegroundBoostEnabled = _settings.BalanceEnabled = true;
        _saved[process.Name] = new() { ProcessName = process.Name, Efficiency = "Enabled" };
        Tick(0, process, process.Identity.Pid);
        Tick(1, process);
        Tick(9, process);
        Assert.Equal(ProcessPriorityClass.Normal, process.Priority); // Fake does not implement EcoQoS.
        Assert.Equal(0, _engine.ActiveBoosts + _engine.ActiveBalances);
    }

    [Fact]
    public void ExternalPriorityChange_IsNotOverwrittenOrRestored()
    {
        var process = new FakeProcess();
        _settings.ForegroundBoostEnabled = true;
        Tick(0, process, process.Identity.Pid);
        process.Priority = ProcessPriorityClass.High;
        Tick(1, process, process.Identity.Pid);
        _engine.RestoreAll([process]);
        Assert.Equal(ProcessPriorityClass.High, process.Priority);
    }

    private void AddLimit() => _settings.Rules.Add(new() { ProcessName = "app", CpuLimit = new()
        { ThresholdPercent = 20, OverSeconds = 3, CoreCount = 2, DurationSeconds = 10 } });

    [Fact]
    public void Limiter_UsesOriginalAllowedCoresAndRestoresAfterDuration()
    {
        var process = new FakeProcess { Affinity = 0b10101100 };
        AddLimit();
        Tick(0, process);
        Tick(2, process);
        Assert.Equal(0b10101100UL, process.Affinity);
        Tick(3, process);
        Assert.Equal(0b00001100UL, process.Affinity);
        process.CpuPercent = 0; // Constrained CPU is not evidence that work is finished.
        Tick(4, process);
        Assert.Equal(0b00001100UL, process.Affinity);
        Tick(13, process);
        Assert.Equal(0b10101100UL, process.Affinity);
        process.CpuPercent = 90;
        Tick(14, process);
        Tick(16, process);
        Assert.Equal(0b10101100UL, process.Affinity);
        Tick(17, process);
        Assert.Equal(0b00001100UL, process.Affinity);
    }

    [Fact]
    public void Limiter_BurstsAndMissingSamplesDoNotTrigger()
    {
        var process = new FakeProcess();
        AddLimit();
        Tick(0, process);
        process.CpuPercent = null;
        Tick(2, process);
        process.CpuPercent = 90;
        Tick(3, process);
        Tick(5, process);
        Assert.Equal(0xFUL, process.Affinity);
    }

    [Fact]
    public void RemovingLimiterRule_RestoresAffinityImmediately()
    {
        var process = new FakeProcess();
        AddLimit();
        Tick(0, process);
        Tick(3, process);
        _settings.Rules.Clear();
        Tick(4, process);
        Assert.Equal(0xFUL, process.Affinity);
    }

    [Fact]
    public void ExternalAffinityChange_IsPreservedWhileLoadStaysHigh()
    {
        var process = new FakeProcess();
        AddLimit();
        Tick(0, process);
        Tick(3, process);
        process.Affinity = 0b1100;
        Tick(4, process);
        Tick(20, process);
        _engine.RestoreAll([process]);
        Assert.Equal(0b1100UL, process.Affinity);
    }

    [Fact]
    public void ReusedPid_DoesNotInheritOldRestorationState()
    {
        var old = new FakeProcess();
        AddLimit();
        _settings.ForegroundBoostEnabled = true;
        Tick(0, old, old.Identity.Pid);
        Tick(3, old, old.Identity.Pid);
        var replacement = new FakeProcess { Identity = new(old.Identity.Pid, 999), Affinity = 0b1000, Priority = ProcessPriorityClass.High };
        Tick(4, replacement);
        _engine.RestoreAll([replacement]);
        Assert.Equal(0b1000UL, replacement.Affinity);
        Assert.Equal(ProcessPriorityClass.High, replacement.Priority);
    }

    [Fact]
    public void FailedMutation_DoesNotClaimAnActiveBoost()
    {
        var process = new FakeProcess { RejectPriority = true };
        _settings.ForegroundBoostEnabled = true;
        Tick(0, process, process.Identity.Pid);
        Tick(1, process, process.Identity.Pid);
        Assert.Equal(0, _engine.ActiveBoosts);
        Assert.Single(_events); // Repeated access errors are throttled.
    }

    [Fact]
    public void FailedRestoration_IsRetried()
    {
        var process = new FakeProcess();
        _settings.ForegroundBoostEnabled = true;
        Tick(0, process, process.Identity.Pid);
        process.RejectPriority = true;
        Tick(1, process);
        Assert.Equal(1, _engine.ActiveBoosts);
        process.RejectPriority = false;
        Tick(2, process);
        Assert.Equal(ProcessPriorityClass.Normal, process.Priority);
    }

    [Fact]
    public void SavedAffinity_DoesNotFightActiveLimiter()
    {
        var process = new FakeProcess();
        AddLimit();
        _saved[process.Name] = new() { ProcessName = process.Name, Affinity = "15" };
        Tick(0, process);
        Tick(3, process);
        Tick(5, process);
        Assert.Equal(3UL, process.Affinity);
        Tick(13, process);
        Assert.Equal(15UL, process.Affinity);
    }

    [Fact]
    public void SavedRules_AreRepeatedAndAppliedToNewInstances()
    {
        var process = new FakeProcess();
        _saved[process.Name] = new() { ProcessName = process.Name, Property = "BelowNormal" };
        Tick(0, process);
        process.Priority = ProcessPriorityClass.Normal;
        Tick(5, process);
        Assert.Equal(ProcessPriorityClass.BelowNormal, process.Priority);
        var next = new FakeProcess { Identity = new(25, 900) };
        Tick(6, next);
        Assert.Equal(ProcessPriorityClass.BelowNormal, next.Priority);
        Assert.Equal(2, process.SavedApplications);
    }

    [Fact]
    public void Limiter_SupportsHighBitAndNeverExpandsAnExistingMask()
    {
        Assert.Equal(1UL << 63, ProcessAutomationEngine.LimitAffinity(1UL << 63, 2));
        Assert.Equal(0x8000000000000001UL, ProcessAutomationEngine.LimitAffinity(0x8000000000000001, 64));
        Assert.Equal(0UL, ProcessAutomationEngine.LimitAffinity(0, 1));
    }

    [Fact]
    public void UnavailableAffinity_DoesNotBlockSavedPriority()
    {
        var process = new FakeProcess { RejectAffinity = true };
        AddLimit();
        _saved[process.Name] = new() { ProcessName = process.Name, Property = "BelowNormal" };
        Tick(0, process);
        process.Priority = ProcessPriorityClass.Normal;
        Tick(5, process);
        Assert.Equal(ProcessPriorityClass.BelowNormal, process.Priority);
        Assert.Single(_events);
    }

    private sealed class FakeProcess : IAutomationProcess
    {
        private ProcessPriorityClass _priority = ProcessPriorityClass.Normal;
        public ProcessIdentity Identity { get; set; } = new(20, 100);
        public string Name => "app";
        public double? CpuPercent { get; set; } = 90;
        public bool RejectPriority { get; set; }
        public ProcessPriorityClass Priority
        {
            get => _priority;
            set { if (RejectPriority) throw new InvalidOperationException("Access denied"); _priority = value; }
        }
        private ulong _affinity = 15;
        public bool RejectAffinity { get; set; }
        public ulong Affinity
        {
            get => RejectAffinity ? throw new NotSupportedException("Multiple processor groups") : _affinity;
            set => _affinity = value;
        }
        public int SavedApplications { get; private set; }
        public void ApplySaved(ProcessData settings, bool applyPriority, bool applyAffinity, Action<string> report)
        {
            SavedApplications++;
            if (applyPriority && Enum.TryParse<ProcessPriorityClass>(settings.Property, out var priority)) Priority = priority;
            if (applyAffinity && ulong.TryParse(settings.Affinity, out ulong mask)) Affinity = mask;
        }
    }
}
