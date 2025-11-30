using PLimit.Utils;
using System.ComponentModel;
using System.Diagnostics;

namespace PLimit
{
    public partial class MainForm : Form
    {
        private BackgroundWorker? _backGroundWorker;
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Main form load event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            _backGroundWorker = new BackgroundWorker();
            _backGroundWorker.DoWork += _backGroundWorker_DoWork;
            _backGroundWorker.RunWorkerAsync();
        }


        /// <summary>
        /// Background worker for load runing processes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _backGroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            this.Invoke(delegate
            {
                var getProcesses = new ProcessesManage();
                getProcesses.GetProcesses(ref processesListBox);
                searchProcessTxt.SetWatermark("Enter process name or PID...");
            });
        }

        /// <summary>
        /// Refresh process list button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshProcessListBtn_Click(object sender, EventArgs e)
        {
            RefreshProcessList();
        }

        /// <summary>
        /// Search process button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchProcessBtn_Click(object sender, EventArgs e)
        {
            SearchProcess();
        }

        private void SearchProcess()
        {
            var searchProcess = new ProcessesManage();
            var search = searchProcessTxt.Text;
            searchProcess.SearchProcess(ref processesListBox, search);
        }

        private void RefreshProcessList()
        {
            _backGroundWorker = new BackgroundWorker();
            _backGroundWorker.DoWork += _backGroundWorker_DoWork;
            _backGroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Activate or deactivate search button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchProcessTxt_TextChanged(object sender, EventArgs e) =>
            searchProcessBtn.Enabled = (string.IsNullOrEmpty(searchProcessTxt.Text)) ? false : true;

        /// <summary>
        /// Load context menu on right click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void processesListBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var focusedItem = processesListBox.FocusedItem;
                if (focusedItem != null)
                    actionMenuStrip.Show(Cursor.Position);
            }
        }

        #region Boost Priority Menu Events
        /// <summary>
        /// Disable priority boost on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void enableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var processId = processesListBox.SelectedItems[0].SubItems[1].Text;
            var setBoost = new ProcessesManage();
            setBoost.SetBoost(true, int.Parse(processId));
            this.Invoke(delegate
            {
                RefreshProcessList();
            });
            SearchProcess();
        }

        /// <summary>
        /// Disable priority boost on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void disableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var processId = processesListBox.SelectedItems[0].SubItems[1].Text;
            var setBoost = new ProcessesManage();
            setBoost.SetBoost(false, int.Parse(processId));
            this.Invoke(delegate
            {
                RefreshProcessList();
            });
            SearchProcess();
        }

        #endregion

        #region IO Priority Menu Events
        /// <summary>
        /// Set IO priority to Very Low on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void veryLowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var processId = processesListBox.SelectedItems[0].SubItems[1].Text;
            var setIoPriority = new ProcessesManage();
            setIoPriority.SetIoPriorityAllThreads(int.Parse(processId), ProcessesManage.IO_PRIORITY_HINT.VeryLow);
            this.Invoke(delegate
            {
                RefreshProcessList();
            });
            SearchProcess();
        }

        /// <summary>
        /// Set IO priority to Low on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var processId = processesListBox.SelectedItems[0].SubItems[1].Text;
            var setIoPriority = new ProcessesManage();
            setIoPriority.SetIoPriorityAllThreads(int.Parse(processId), ProcessesManage.IO_PRIORITY_HINT.Low);
            this.Invoke(delegate
            {
                RefreshProcessList();
            });
            SearchProcess();
        }

        /// <summary>
        /// Set IO priority to Normal on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var processId = processesListBox.SelectedItems[0].SubItems[1].Text;
            var setIoPriority = new ProcessesManage();
            setIoPriority.SetIoPriorityAllThreads(int.Parse(processId), ProcessesManage.IO_PRIORITY_HINT.Normal);
            this.Invoke(delegate
            {
                RefreshProcessList();
            });
            SearchProcess();
        }


        /// <summary>
        /// Set IO priority to High on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void highToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var processId = processesListBox.SelectedItems[0].SubItems[1].Text;
            var setIoPriority = new ProcessesManage();
            setIoPriority.SetIoPriorityAllThreads(int.Parse(processId), ProcessesManage.IO_PRIORITY_HINT.High);
            this.Invoke(delegate
            {
                RefreshProcessList();
            });
            SearchProcess();
        }
        #endregion


        #region Priority Class Menu Events

        /// <summary>
        /// Set process priority to High on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void highToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var processId = processesListBox.SelectedItems[0].SubItems[1].Text;
            var setPriority = new ProcessesManage();
            setPriority.SetPriorityClass(ProcessPriorityClass.High, int.Parse(processId));
            this.Invoke(delegate
            {
                RefreshProcessList();
            });
            SearchProcess();
        }

        /// <summary>
        /// Set process priority to Real Time on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void realTimedangerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var processId = processesListBox.SelectedItems[0].SubItems[1].Text;
            var setPriority = new ProcessesManage();
            setPriority.SetPriorityClass(ProcessPriorityClass.RealTime, int.Parse(processId));
            this.Invoke(delegate
            {
                RefreshProcessList();
            });
            SearchProcess();
        }

        /// <summary>
        /// Set process priority to Above Normal on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboveNormalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var processId = processesListBox.SelectedItems[0].SubItems[1].Text;
            var setPriority = new ProcessesManage();
            setPriority.SetPriorityClass(ProcessPriorityClass.AboveNormal, int.Parse(processId));
            this.Invoke(delegate
            {
                RefreshProcessList();
            });
            SearchProcess();
        }

        /// <summary>
        /// Set process priority to Normal on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void normalToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var processId = processesListBox.SelectedItems[0].SubItems[1].Text;
            var setPriority = new ProcessesManage();
            setPriority.SetPriorityClass(ProcessPriorityClass.Normal, int.Parse(processId));
            this.Invoke(delegate
            {
                RefreshProcessList();
            });
            SearchProcess();
        }

        /// <summary>
        /// Set process priority to Below Normal on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void belowNormalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var processId = processesListBox.SelectedItems[0].SubItems[1].Text;
            var setPriority = new ProcessesManage();
            setPriority.SetPriorityClass(ProcessPriorityClass.BelowNormal, int.Parse(processId));
            this.Invoke(delegate
            {
                RefreshProcessList();
            });
            SearchProcess();
        }
        #endregion

        #region Efificiency Mode Menu Events

        /// <summary>
        /// Enable efficiency mode on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void enableToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var processId = processesListBox.SelectedItems[0].SubItems[1].Text;
            var setEfficiencyMode = new EfficiencyModeHelper();
            setEfficiencyMode.EnableEfficiencyMode(int.Parse(processId));
            this.Invoke(delegate
            {
                RefreshProcessList();
            });
            SearchProcess();
        }


        /// <summary>
        /// Disable efficiency mode on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void disableToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var processId = processesListBox.SelectedItems[0].SubItems[1].Text;
            var setEfficiencyMode = new EfficiencyModeHelper();
            setEfficiencyMode.DisableEfficiencyMode(int.Parse(processId));
            this.Invoke(delegate
            {
                RefreshProcessList();
            });
            SearchProcess();
        }
        #endregion

        /// <summary>
        /// Load processor affinity submenu event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void afinityToolStripMenuItem_MouseHover(object sender, EventArgs e)
        {
            afinityToolStripMenuItem.DropDownItems.Clear();

            if (processesListBox.SelectedItems.Count == 0)
                return;

            if (!int.TryParse(processesListBox.SelectedItems[0].SubItems[1].Text, out int pid))
                return;

            Process p;
            try { p = Process.GetProcessById(pid); }
            catch { return; }

            afinityToolStripMenuItem.Tag = pid; // store PID for click handler

            long mask = (long)p.ProcessorAffinity;     // bitmask
            int coreCount = Environment.ProcessorCount; // how many logical CPU cores Windows reports

            for (int core = 0; core < coreCount; core++)
            {
                bool isSet = (mask & (1L << core)) != 0;

                var coreItem = new ToolStripMenuItem($"Core {core}")
                {
                    CheckOnClick = true,
                    Checked = isSet,
                    Tag = core // store core index
                };

                coreItem.Click += CoreToolStripMenuItem_Click;
                afinityToolStripMenuItem.DropDownItems.Add(coreItem);
            }
        }

        /// <summary>
        /// Set processor affinity (enable/disable cores) event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (afinityToolStripMenuItem.Tag is not int pid)
                return;

            Process p;
            try { p = Process.GetProcessById(pid); }
            catch { return; }

            long newMask = 0;

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

            try
            {
                p.ProcessorAffinity = (IntPtr)newMask; // apply enable/disable cores
            }
            catch
            {
                // access denied / process exited / 32-bit limitations / etc.
                // Optional: MessageBox.Show("Couldn't change affinity.");
            }
            this.Invoke(delegate
            {
                RefreshProcessList();
            });
            SearchProcess();
        }

        /// <summary>
        /// Search event on Enter key press.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchProcessTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SearchProcess();
            }
        }
    }
}
