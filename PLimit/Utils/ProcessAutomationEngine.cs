using System.Diagnostics;
using System.Numerics;

namespace PLimit.Utils;

internal readonly record struct ProcessIdentity(int Pid, long StartTime);

internal interface IAutomationProcess
{
    ProcessIdentity Identity { get; }
    string Name { get; }
    double? CpuPercent { get; }
    ProcessPriorityClass Priority { get; set; }
    ulong Affinity { get; set; }
    void ApplySaved(ProcessData settings, bool applyPriority, bool applyAffinity, Action<string> report);
}

/// <summary>Single-threaded policy engine. The host owns sampling, handles and synchronization.</summary>
internal sealed class ProcessAutomationEngine(Action<string> log)
{
    private sealed class State
    {
        public ProcessPriorityClass? OriginalPriority;
        public ProcessPriorityClass? AppliedPriority;
        public string PriorityReason = "";
        public ulong? OriginalAffinity;
        public ulong? AppliedAffinity;
        public CpuLimitRule? ActiveLimit;
        public TimeSpan LimitUntil;
        public TimeSpan? BusySince;
        public TimeSpan? BalanceSince;
        public TimeSpan? QuietSince;
        public TimeSpan NextSaved;
        public bool PriorityOverridden;
        public bool AffinityOverridden;
    }

    private readonly Dictionary<ProcessIdentity, State> _states = new();
    private readonly Dictionary<string, TimeSpan> _lastErrors = new();
    public int ActiveBoosts => _states.Values.Count(s => s.PriorityReason == "Foreground boost");
    public int ActiveBalances => _states.Values.Count(s => s.PriorityReason == "Background balance");
    public int ActiveLimits => _states.Values.Count(s => s.OriginalAffinity.HasValue);

    public void Tick(IReadOnlyList<IAutomationProcess> processes, int foregroundPid, double? systemCpu,
        TimeSpan now, AutomationSettings settings, IReadOnlyDictionary<string, ProcessData> saved)
    {
        var live = processes.Select(p => p.Identity).ToHashSet();
        foreach (var key in _states.Keys.Where(k => !live.Contains(k)).ToArray())
            _states.Remove(key);
        foreach (var key in _lastErrors.Where(pair => now - pair.Value > TimeSpan.FromMinutes(5)).Select(pair => pair.Key).ToArray())
            _lastErrors.Remove(key);

        foreach (var process in processes)
        {
            if (!_states.TryGetValue(process.Identity, out var state))
                _states[process.Identity] = state = new();
            try
            {
                saved.TryGetValue(process.Name, out var persistent);
                var rule = settings.FindRule(process.Name);
                bool hasPriority = persistent != null && Enum.TryParse<ProcessPriorityClass>(persistent.Property, out var priority)
                    && Enum.IsDefined(priority);
                bool efficiency = persistent != null && SettingState.TryParse(persistent.Efficiency, out bool eco) && eco;
                bool foreground = process.Identity.Pid == foregroundPid;

                // An explicit priority/efficiency rule takes precedence over automatic priority changes.
                bool boost = settings.ForegroundBoostEnabled && foreground && rule?.ExcludeForegroundBoost != true
                    && !hasPriority && !efficiency;
                bool balanceAllowed = settings.BalanceEnabled && !foreground && rule?.ExcludeBalance != true
                    && !hasPriority && !efficiency;
                bool busy = balanceAllowed && systemCpu >= settings.BalanceSystemThreshold
                    && process.CpuPercent >= settings.BalanceProcessThreshold;
                state.BalanceSince = busy ? state.BalanceSince ?? now : null;
                bool quiet = !balanceAllowed || systemCpu == null || process.CpuPercent == null
                    || systemCpu < settings.BalanceSystemThreshold - 10 || process.CpuPercent < settings.BalanceProcessThreshold / 2.0;
                state.QuietSince = quiet ? state.QuietSince ?? now : null;
                bool balance = balanceAllowed && ((state.BalanceSince.HasValue
                    && now - state.BalanceSince.Value >= TimeSpan.FromSeconds(settings.BalanceOverSeconds))
                    || (state.PriorityReason == "Background balance"
                        && (!state.QuietSince.HasValue || now - state.QuietSince.Value < TimeSpan.FromSeconds(3))));

                void Attempt(Action action)
                {
                    try { action(); }
                    catch (Exception ex) { ReportError(process, ex.Message, now); }
                }
                Attempt(() => UpdatePriority(process, state, boost ? "Foreground boost" : balance ? "Background balance" : ""));
                Attempt(() => UpdateLimit(process, state, rule?.CpuLimit, now));

                if (persistent != null && now >= state.NextSaved)
                {
                    state.NextSaved = now + TimeSpan.FromSeconds(5);
                    Attempt(() => process.ApplySaved(persistent, !state.OriginalPriority.HasValue, !state.OriginalAffinity.HasValue,
                        message => ReportError(process, message, now)));
                }
            }
            catch (Exception ex)
            {
                ReportError(process, ex.Message, now);
            }
        }
    }

    private void ReportError(IAutomationProcess process, string message, TimeSpan now)
    {
        string key = $"{process.Identity}: {message}";
        if (!_lastErrors.TryGetValue(key, out var last) || now - last >= TimeSpan.FromSeconds(30))
        {
            _lastErrors[key] = now;
            log($"{process.Name} ({process.Identity.Pid}): {message}");
        }
    }

    private void UpdatePriority(IAutomationProcess process, State state, string reason)
    {
        if (state.OriginalPriority.HasValue && process.Priority != state.AppliedPriority)
        {
            log($"{process.Name} ({process.Identity.Pid}): priority changed externally; automatic adjustment released.");
            state.OriginalPriority = null;
            state.AppliedPriority = null;
            state.PriorityReason = "";
            state.PriorityOverridden = true;
        }
        if (state.OriginalPriority.HasValue && state.PriorityReason != reason)
            RestorePriority(process, state);
        if (reason.Length == 0)
            state.PriorityOverridden = false;
        if (reason.Length == 0 || state.OriginalPriority.HasValue || state.PriorityOverridden)
            return;
        // Do not boost Idle/BelowNormal work or lower High/RealTime applications.
        if (process.Priority != ProcessPriorityClass.Normal)
            return;
        var desired = reason == "Foreground boost" ? ProcessPriorityClass.AboveNormal : ProcessPriorityClass.BelowNormal;
        process.Priority = desired;
        state.OriginalPriority = ProcessPriorityClass.Normal;
        state.AppliedPriority = desired;
        state.PriorityReason = reason;
        log($"{process.Name} ({process.Identity.Pid}): {reason} → {desired}.");
    }

    private void RestorePriority(IAutomationProcess process, State state)
    {
        if (state.OriginalPriority.HasValue && process.Priority == state.AppliedPriority)
        {
            process.Priority = state.OriginalPriority.Value;
            log($"{process.Name} ({process.Identity.Pid}): restored priority {state.OriginalPriority}.");
        }
        state.OriginalPriority = null;
        state.AppliedPriority = null;
        state.PriorityReason = "";
    }

    private void UpdateLimit(IAutomationProcess process, State state, CpuLimitRule? rule, TimeSpan now)
    {
        if (state.OriginalAffinity.HasValue)
        {
            if (process.Affinity != state.AppliedAffinity)
            {
                state.OriginalAffinity = null;
                state.AppliedAffinity = null;
                state.AffinityOverridden = true;
                log($"{process.Name} ({process.Identity.Pid}): affinity changed externally; CPU limiter released.");
            }
            else if (now >= state.LimitUntil || !SameLimit(rule, state.ActiveLimit))
            {
                RestoreAffinity(process, state);
                state.BusySince = null;
                return; // Require a fresh sustained load interval before reapplying.
            }
            else return;
        }
        if (rule == null || !rule.IsValid || process.CpuPercent == null || process.CpuPercent < rule.ThresholdPercent)
        {
            state.BusySince = null;
            state.AffinityOverridden = false;
            return;
        }
        if (state.AffinityOverridden)
            return;
        state.BusySince ??= now;
        if (now - state.BusySince.Value < TimeSpan.FromSeconds(rule.OverSeconds))
            return;
        ulong original = process.Affinity;
        ulong limited = LimitAffinity(original, rule.CoreCount);
        if (limited == 0 || limited == original)
            return;
        process.Affinity = limited;
        state.OriginalAffinity = original;
        state.AppliedAffinity = limited;
        state.ActiveLimit = new CpuLimitRule { ThresholdPercent = rule.ThresholdPercent, OverSeconds = rule.OverSeconds,
            CoreCount = rule.CoreCount, DurationSeconds = rule.DurationSeconds };
        state.LimitUntil = now + TimeSpan.FromSeconds(rule.DurationSeconds);
        log($"{process.Name} ({process.Identity.Pid}): CPU limiter → {BitOperations.PopCount(limited)} cores for {rule.DurationSeconds}s.");
    }

    private static bool SameLimit(CpuLimitRule? left, CpuLimitRule? right) => left != null && right != null
        && left.ThresholdPercent == right.ThresholdPercent && left.OverSeconds == right.OverSeconds
        && left.CoreCount == right.CoreCount && left.DurationSeconds == right.DurationSeconds;

    internal static ulong LimitAffinity(ulong original, int cores)
    {
        if (cores < 1) throw new ArgumentOutOfRangeException(nameof(cores));
        ulong result = 0;
        for (int bit = 0; bit < 64 && cores > 0; bit++)
        {
            ulong flag = 1UL << bit;
            if ((original & flag) == 0) continue;
            result |= flag;
            cores--;
        }
        return result;
    }

    private void RestoreAffinity(IAutomationProcess process, State state)
    {
        if (state.OriginalAffinity.HasValue && process.Affinity == state.AppliedAffinity)
        {
            process.Affinity = state.OriginalAffinity.Value;
            log($"{process.Name} ({process.Identity.Pid}): restored CPU affinity.");
        }
        state.OriginalAffinity = null;
        state.AppliedAffinity = null;
        state.ActiveLimit = null;
    }

    public void RestoreAll(IReadOnlyList<IAutomationProcess> processes)
    {
        foreach (var process in processes)
        {
            if (!_states.TryGetValue(process.Identity, out var state)) continue;
            try { RestorePriority(process, state); }
            catch (Exception ex) { log($"{process.Name}: priority restoration failed: {ex.Message}"); }
            try { RestoreAffinity(process, state); }
            catch (Exception ex) { log($"{process.Name}: affinity restoration failed: {ex.Message}"); }
        }
    }
}
