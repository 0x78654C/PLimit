using System.ComponentModel;
using PLimit.Utils;
using Xunit;

namespace PLimit.Tests;

public class EfficiencyModeTests
{
    [Fact]
    public void EfficiencyMode_CanReadEnabledAndDisabledState() => WindowsTest.WithProcess(process =>
    {
        // Querying power throttling requires Windows 11 22H2 or later.
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22621))
            return;

        var helper = new EfficiencyModeHelper();
        Assert.True(helper.EnableEfficiencyMode(process.Id));
        Assert.True(helper.IsEfficiencyModeEnabled(process.Id));

        Assert.True(helper.DisableEfficiencyMode(process.Id));
        Assert.False(helper.IsEfficiencyModeEnabled(process.Id));
    });

    [Fact]
    public void IsEfficiencyModeEnabled_InvalidHandle_ThrowsInsteadOfReportingDisabled() =>
        Assert.Throws<Win32Exception>(() => new EfficiencyModeHelper().IsEfficiencyModeEnabled(IntPtr.Zero));
}
