using PLimit.Utils;
using Xunit;

namespace PLimit.Tests;

public class SettingStateTests
{
    [Theory]
    [InlineData("Enabled", true)]
    [InlineData("enabled", true)]
    [InlineData("True", true)]
    [InlineData("Disabled", false)]
    [InlineData("disabled", false)]
    [InlineData("False", false)]
    public void TryParse_AcceptsCanonicalAndLegacyValues(string value, bool expected)
    {
        Assert.True(SettingState.TryParse(value, out bool actual));
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Unknown")]
    public void TryParse_RejectsUnsetOrUnknownValues(string? value)
    {
        Assert.False(SettingState.TryParse(value, out _));
    }

    [Theory]
    [InlineData(true, SettingState.Enabled)]
    [InlineData(false, SettingState.Disabled)]
    public void FromBoolean_ReturnsCanonicalValue(bool value, string expected)
    {
        Assert.Equal(expected, SettingState.FromBoolean(value));
    }
}
