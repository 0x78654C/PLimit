using PLimit.Utils;
using Xunit;

namespace PLimit.Tests;

public class ProcessSearchTests
{
    [Theory]
    [InlineData("  PAD  ")]
    [InlineData("234")]
    public void SearchProcess_SelectsSubstringMatchInNameOrPid(string query) => WindowsTest.Run(() =>
    {
        var list = new DoubleBufferedListView { View = View.Details };
        using (list)
        {
            list.Columns.Add("Name");
            list.Columns.Add("PID");
            list.CreateControl();
            var item = list.Items.Add(new ListViewItem(new[] { "notepad", "12345", "Normal" }));

            new ProcessesManage().SearchProcess(list, query, isMessage: false);

            Assert.True(item.Selected);
        }
    });

    [Fact]
    public void SearchProcess_EmptyList_DoesNotThrow() => WindowsTest.Run(() =>
    {
        var list = new DoubleBufferedListView();
        using (list)
            new ProcessesManage().SearchProcess(list, "missing", isMessage: false);
    });

    [Fact]
    public void SearchProcess_DoesNotMatchOtherColumns() => WindowsTest.Run(() =>
    {
        var list = new DoubleBufferedListView();
        using (list)
        {
            list.CreateControl();
            var item = list.Items.Add(new ListViewItem(new[] { "notepad", "12345", "Normal" }));

            new ProcessesManage().SearchProcess(list, "Normal", isMessage: false);

            Assert.False(item.Selected);
        }
    });
}
