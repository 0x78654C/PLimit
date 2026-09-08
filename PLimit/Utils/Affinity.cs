using System.Diagnostics;

namespace PLimit.Utils
{
    public class Affinity
    {

        public Affinity() { }

        internal static void PopulateMenu(ToolStripMenuItem menu, int pid, EventHandler onCoreClick)
        {
            // Clear() removes items without disposing their handles and event handlers.
            while (menu.DropDownItems.Count > 0)
            {
                var item = menu.DropDownItems[0];
                item.Dispose();
            }
            menu.Tag = null;

            long mask;
            try
            {
                using var process = Process.GetProcessById(pid);
                mask = process.ProcessorAffinity.ToInt64();
            }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or ArgumentException or NotSupportedException)
            {
                menu.DropDownItems.Add(new ToolStripMenuItem("Affinity unavailable (process exited or access denied)")
                {
                    Enabled = false
                });
                return;
            }

            menu.Tag = pid;
            int coreCount = Math.Min(Environment.ProcessorCount, IntPtr.Size * 8);
            for (int core = 0; core < coreCount; core++)
            {
                var item = new ToolStripMenuItem($"Core {core}")
                {
                    CheckOnClick = true,
                    Checked = (mask & (1L << core)) != 0,
                    Tag = core,
                    BackColor = DarkTheme.Surface,
                    ForeColor = DarkTheme.TextPrimary
                };
                item.Click += onCoreClick;
                menu.DropDownItems.Add(item);
            }
        }

        /// <summary>
        /// Set affinity for a process based on the checked cores in the context menu. 
        /// The menu items should have their Tag set to the core index (0-based) and the parent menu's Tag set to the process ID.
        /// After changing affinity, it refreshes the process list and re-applies any search filter.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="processesListBox"></param>
        /// <param name="afinityToolStripMenuItem"></param>
        /// <param name="label"></param>
        /// <param name="searchBox"></param>
        /// <param name="sender"></param>
        public void SetAffinity(Form from, DoubleBufferedListView processesListBox, ToolStripMenuItem? afinityToolStripMenuItem, Label label, TextBox searchBox, object? sender, string mask = "", string pidId = "", bool isStartUp = false)
        {
            int pid;
            if (!string.IsNullOrWhiteSpace(pidId))
            {
                if (!int.TryParse(pidId, out pid))
                    return;
            }
            else
            {
                if (!int.TryParse(afinityToolStripMenuItem?.Tag?.ToString(), out pid))
                    return;
            }

            Process p;
            try
            {
                p = Process.GetProcessById(pid);
            }
            catch
            {
                RestoreCheckedState(sender);
                if (!isStartUp)
                    MessageBox.Show("The process is no longer running. Refresh the process list.", "Process Limiter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (p)
            {
                long newMask = 0;
                if (string.IsNullOrEmpty(pidId))
                {
                    foreach (ToolStripItem tsi in afinityToolStripMenuItem!.DropDownItems)
                    {
                        if (tsi is ToolStripMenuItem mi && mi.Tag is int core && mi.Checked)
                            newMask |= (1L << core);
                    }

                    // must keep at least 1 core enabled
                    if (newMask == 0)
                    {
                        if (sender is ToolStripMenuItem clicked)
                            clicked.Checked = true;
                        return;
                    }
                }
                if (!string.IsNullOrEmpty(mask) &&
                    (!long.TryParse(mask, out newMask) || newMask == 0))
                {
                    return;
                }

                string processName;
                try
                {
                    processName = p.ProcessName;
                    p.ProcessorAffinity = (IntPtr)newMask; // apply enable/disable cores
                }
                catch
                {
                    RestoreCheckedState(sender);
                    if (!isStartUp)
                        MessageBox.Show("Failed to change processor affinity! Try running the application as administrator.", "Process Limiter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrEmpty(pidId) && Properties.Settings.Default.isSaveingSettings)
                {
                    var storeAffinity = new StoreSettings();
                    storeAffinity.SaveAppliedSetting(from, StoreSettings.SettingType.Affinity, processName, newMask.ToString());
                }

                if (!isStartUp)
                {
                    from.BeginInvoke(new Action(() =>
                    {
                        var utils = new Utils();
                        utils.RefreshProcessList(from, processesListBox, label);
                        utils.SearchProcess(searchBox, processesListBox);
                    }));
                }
            }
        }

        private static void RestoreCheckedState(object? sender)
        {
            if (sender is ToolStripMenuItem clicked)
                clicked.Checked = !clicked.Checked;
        }
    }
}
