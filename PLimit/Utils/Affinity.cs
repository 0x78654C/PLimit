using System.Diagnostics;
using System.Security.Cryptography;

namespace PLimit.Utils
{
    public class Affinity
    {

        public Affinity() { }

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
        public void SetAffinity(Form from, DoubleBufferedListView processesListBox, ToolStripMenuItem afinityToolStripMenuItem, Label label, TextBox searchBox, object sender, string mask = "", string pidId = "")
        {
            int pid;
            if (!string.IsNullOrWhiteSpace(pidId))
            {
                if (!int.TryParse(pidId, out pid))
                    return;
            }
            else
            {
                if (!int.TryParse(afinityToolStripMenuItem.Tag?.ToString(), out pid))
                    return;
            }


            Process p;
            try { p = Process.GetProcessById(pid); }
            catch { return; }

            long newMask = 0;
            if (string.IsNullOrEmpty(pidId))
            {
                foreach (ToolStripItem tsi in afinityToolStripMenuItem.DropDownItems)
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
            if (!string.IsNullOrEmpty(mask))
                newMask = Convert.ToInt64(mask);
            try
            {
                p.ProcessorAffinity = (IntPtr)newMask; // apply enable/disable cores
            }
            catch
            {
                // access denied / process exited / 32-bit limitations / etc.
                // Optional: MessageBox.Show("Couldn't change affinity.");
            }
            from.BeginInvoke(new Action(() =>
            {
                var utils = new Utils();
                utils.RefreshProcessList(from, processesListBox, label);
                utils.SearchProcess(searchBox, processesListBox);
            }));
            if (string.IsNullOrEmpty(mask))
            {
                var storeAffinity = new StoreSettings(); ;
                storeAffinity.UpdateSetting(StoreSettings.SettingType.Affinity, p.ProcessName, newMask.ToString());
            }
        }
    }
}
