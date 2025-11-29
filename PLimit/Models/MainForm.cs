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


        private void LoadCoresMenu()
        {
            var processorsCount = Environment.ProcessorCount;
            for (int i = 0; i < processorsCount; i++)
            {
                var coreToolStripMenuItem = new ToolStripMenuItem
                {
                    Text = $"Core {i}",
                    CheckOnClick = true,
                    Checked = true,
                };
                coreToolStripMenuItem.Click += CoreToolStripMenuItem_Click;
                afinityToolStripMenuItem.DropDownItems.Add(coreToolStripMenuItem);
            }
        }

        private void CoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
        
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
                LoadCoresMenu();
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
            RefreshProcessList();
            SearchProcess();
        }

        /// <summary>
        /// Disable priority boost on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void processesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void disableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var processId = processesListBox.SelectedItems[0].SubItems[1].Text;
            var setBoost = new ProcessesManage();
            setBoost.SetBoost(false, int.Parse(processId));
            RefreshProcessList();
            SearchProcess();
        }


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
            RefreshProcessList();
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
            RefreshProcessList();
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
            RefreshProcessList();
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
            RefreshProcessList();
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
            RefreshProcessList();
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
            RefreshProcessList();
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
            RefreshProcessList();
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
            RefreshProcessList();
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
            RefreshProcessList();
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
            RefreshProcessList();
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
            RefreshProcessList();
            SearchProcess();
        }
        #endregion
    }
}
