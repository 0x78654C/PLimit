using PLimit.Utils;
using Xunit;

namespace PLimit.Tests;

public class ProcessSnapshotTests
{
    private static ProcessSnapshot Snapshot(string name = "notepad", int pid = 1234) => new(
        name,
        pid,
        "Normal",
        8,
        "Normal",
        "Enabled",
        "Disabled",
        false,
        "Enabled",
        "user");

    [Theory]
    [InlineData("note")]
    [InlineData("NOTEPAD")]
    [InlineData("1234")]
    [InlineData("23")]
    [InlineData("  note  ")]
    public void Matches_NameOrPid_ReturnsTrue(string query) =>
        Assert.True(Snapshot().Matches(query));

    [Fact]
    public void Matches_UnrelatedQuery_ReturnsFalse() =>
        Assert.False(Snapshot().Matches("explorer"));

    [Fact]
    public void ToListViewItem_MapsAllColumns()
    {
        var item = Snapshot().ToListViewItem();

        Assert.Equal(10, item.SubItems.Count);
        Assert.Equal("notepad", item.SubItems[0].Text);
        Assert.Equal("1234", item.SubItems[1].Text);
        Assert.Equal("No", item.SubItems[7].Text);
    }
}
