using PLimit.Utils;
using System.ComponentModel;

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
            });
        }

        /// <summary>
        /// Refresh process list button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshProcessListBtn_Click(object sender, EventArgs e)
        {
            _backGroundWorker = new BackgroundWorker();
            _backGroundWorker.DoWork += _backGroundWorker_DoWork;
            _backGroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Search process button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchProcessBtn_Click(object sender, EventArgs e)
        {
            var searchProcess = new ProcessesManage();
            var search = searchProcessTxt.Text;
            searchProcess.SearchProcess(ref processesListBox, search);
        }

        /// <summary>
        /// Activate or deactivate search button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchProcessTxt_TextChanged(object sender, EventArgs e) =>
            searchProcessBtn.Enabled = (string.IsNullOrEmpty(searchProcessTxt.Text)) ? false : true;
    }
}
