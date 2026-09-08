using PLimit.Utils;
using Xunit;

namespace PLimit.Tests;

public class AffinityTests
{
    [Fact]
    public void ContextMenu_OpeningOnProtectedProcess_LeavesOtherActionsAvailable() => WindowsTest.Run(() =>
    {
        using var form = new MainForm();
        var list = Assert.IsType<DoubleBufferedListView>(Assert.Single(form.Controls.Find("processesListBox", true)));
        _ = list.Handle;
        list.Items.Add(new ListViewItem(new[] { "Idle", "0" })).Selected = true;
        Assert.Single(list.SelectedItems);
        var menu = Assert.IsType<ContextMenuStrip>(list.ContextMenuStrip);
        var args = new System.ComponentModel.CancelEventArgs();

        typeof(ContextMenuStrip).GetMethod("OnOpening", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(menu, new object[] { args });

        Assert.False(args.Cancel);
        var affinity = Assert.IsType<ToolStripMenuItem>(Assert.Single(menu.Items.Find("afinityToolStripMenuItem", true)));
        Assert.False(Assert.Single(affinity.DropDownItems.Cast<ToolStripItem>()).Enabled);
        Assert.True(Assert.Single(menu.Items.Find("idleToolStripMenuItem", true)).Enabled);
    });

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void PopulateMenu_UnavailableProcess_ClearsOldTargetAndDisposesItems(int pid) => WindowsTest.Run(() =>
    {
        using var menu = new ToolStripMenuItem { Tag = 123 };
        var oldItem = new ToolStripMenuItem("Core 0");
        menu.DropDownItems.Add(oldItem);

        Affinity.PopulateMenu(menu, pid, (_, _) => { });

        Assert.Null(menu.Tag);
        Assert.True(oldItem.IsDisposed);
        var item = Assert.Single(menu.DropDownItems.Cast<ToolStripItem>());
        Assert.False(item.Enabled);
    });

    [Fact]
    public void PopulateMenu_RunningProcess_ShowsItsAffinity() => WindowsTest.Run(() => WindowsTest.WithProcess(process =>
    {
        using var menu = new ToolStripMenuItem();

        Affinity.PopulateMenu(menu, process.Id, (_, _) => { });

        Assert.Equal(process.Id, menu.Tag);
        long actualMask = process.ProcessorAffinity.ToInt64();
        Assert.NotEmpty(menu.DropDownItems);
        foreach (ToolStripMenuItem item in menu.DropDownItems)
            Assert.Equal((actualMask & (1L << (int)item.Tag!)) != 0, item.Checked);
    }));

    [Fact]
    public void SetAffinity_LastCore_RemainsChecked() => WindowsTest.Run(() => WindowsTest.WithProcess(process =>
    {
        using var menu = new ToolStripMenuItem { Tag = process.Id };
        using var core = new ToolStripMenuItem { Tag = 0, Checked = false };
        menu.DropDownItems.Add(core);
        long originalMask = process.ProcessorAffinity.ToInt64();

        new Affinity().SetAffinity(null!, null!, menu, null!, null!, core, isStartUp: true);

        Assert.True(core.Checked);
        process.Refresh();
        Assert.Equal(originalMask, process.ProcessorAffinity.ToInt64());
    }));
}
