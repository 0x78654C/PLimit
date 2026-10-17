using System.Diagnostics;
using PLimit.Utils;
using Xunit;

namespace PLimit.Tests;

public class AutomationWindowsTests
{
    [Fact]
    public void NativeHandle_PriorityAndAffinityRoundTrip() => WindowsTest.WithProcess(process =>
    {
        using var target = WindowsAutomationProcess.TryOpen(process, process.SessionId);
        Assert.NotNull(target);
        Assert.True(target.IsAlive);
        Assert.Equal(process.StartTime.ToUniversalTime().ToFileTimeUtc(), target.Identity.StartTime);
        var priority = target.Priority;
        target.Priority = ProcessPriorityClass.BelowNormal;
        process.Refresh();
        Assert.Equal(ProcessPriorityClass.BelowNormal, process.PriorityClass);
        target.Priority = priority;
        if (WindowsAutomationProcess.SupportsAffinity)
        {
            ulong original = target.Affinity;
            ulong limited = ProcessAutomationEngine.LimitAffinity(original, 1);
            target.Affinity = limited;
            process.Refresh();
            Assert.Equal(limited, unchecked((ulong)process.ProcessorAffinity.ToInt64()));
            target.Affinity = original;
        }
        target.Sample(TimeSpan.Zero);
        Assert.Null(target.CpuPercent);
        target.Sample(TimeSpan.FromSeconds(1));
        Assert.InRange(target.CpuPercent!.Value, 0, 100);
    });

    [Fact]
    public void NativeRules_ApplyBoostAndIoWithoutUi() => WindowsTest.WithProcess(process =>
    {
        using var target = WindowsAutomationProcess.TryOpen(process, process.SessionId);
        Assert.NotNull(target);
        var failures = new List<string>();
        target.ApplySaved(new() { Boosted = "Disabled", IOProperty = "Low" }, true, true, failures.Add);
        Assert.Empty(failures);
        process.Refresh();
        Assert.False(process.PriorityBoostEnabled);
        var manager = new ProcessesManage();
        Assert.False(manager.GetThreadBoost(process.Id));
        Assert.Equal(ProcessesManage.IO_PRIORITY_HINT.Low, ProcessesManage.GetIoPriority(process.Threads[0]));
        target.ApplySaved(new() { Boosted = "True", IOProperty = "Normal" }, true, true, failures.Add);
        Assert.Empty(failures);
        process.Refresh();
        Assert.True(process.PriorityBoostEnabled);
        Assert.True(manager.GetThreadBoost(process.Id));
        Assert.Equal(ProcessesManage.IO_PRIORITY_HINT.Normal, ProcessesManage.GetIoPriority(process.Threads[0]));
    });

    [Fact]
    public void ThreadBoostAction_UpdatesProcessWideDefault() => WindowsTest.WithProcess(process =>
    {
        var manager = new ProcessesManage();
        Assert.True(manager.SetThreadBoost(false, process.Id));
        process.Refresh();
        Assert.False(process.PriorityBoostEnabled);
        Assert.True(manager.SetThreadBoost(true, process.Id));
        process.Refresh();
        Assert.True(process.PriorityBoostEnabled);
    });

    [Fact]
    public void NativeRules_OneFailureDoesNotStopOtherSettings() => WindowsTest.WithProcess(process =>
    {
        using var target = WindowsAutomationProcess.TryOpen(process, process.SessionId);
        Assert.NotNull(target);
        var errors = new List<string>();
        target.ApplySaved(new() { Affinity = "-1", Boosted = "Disabled", Property = "BelowNormal" }, true, true, errors.Add);
        process.Refresh();
        Assert.False(process.PriorityBoostEnabled);
        Assert.Equal(ProcessPriorityClass.BelowNormal, process.PriorityClass);
        if (Environment.ProcessorCount < 64) Assert.NotEmpty(errors);
    });

    [Fact]
    public void NativeHandle_RefusesSelfAndOtherSessions()
    {
        using var current = Process.GetCurrentProcess();
        Assert.Null(WindowsAutomationProcess.TryOpen(current, current.SessionId));
        WindowsTest.WithProcess(process => Assert.Null(WindowsAutomationProcess.TryOpen(process, -1)));
    }
}
