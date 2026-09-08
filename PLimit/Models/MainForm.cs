/*
      Description: A lightweight Windows utility to manage and fine-tune running process priorities,
      CPU affinity, I/O priority, process priority boost, thread priority boost, and efficiency mode.

      This app is distributed under the MIT License.
      Copyright (c) 2026 x_coding. All rights reserved.

      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
      IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
      FITNESS FOR A PARTICULAR PURPOSE AND NON INFRINGEMENT. IN NO EVENT SHALL THE
      AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
      LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
      OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
      SOFTWARE.
*/

using PLimit.Utils;
using System.Reflection;

namespace PLimit
{
    public partial class MainForm : Form
    {
        private static readonly int[] MinimumProcessColumnWidths =
            { 150, 55, 75, 60, 70, 85, 90, 85, 65, 80 };
        private static readonly int[] PreferredProcessColumnWidths =
            { 250, 119, 120, 120, 120, 105, 120, 120, 90, 130 };

        private readonly SemaphoreSlim _refreshGate = new(1, 1);
        private readonly CancellationTokenSource _refreshCancellation = new();
        private IReadOnlyList<ProcessSnapshot> _processSnapshots = Array.Empty<ProcessSnapshot>();
        private int? _lastPid;
        private int _lastOffsetFromTop; // selectedIndex - topIndex
        private int _lastTopIndex;
        private bool _isPointerOverProcessList;
        private bool _isClosing;
        private bool _refreshRequested;

        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Main form load event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainForm_Load(object sender, EventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Text = version != null ? $"PLimit - Process Limiter v{version.Major}.{version.Minor}.{version.Build}" : "PLimit - Process Limiter v1.0";
            checkBox1.Checked = Properties.Settings.Default.isLoadingSettings;
            SaveSettingsCkb.Checked = Properties.Settings.Default.isSaveingSettings;

            DarkTheme.Apply(this, actionMenuStrip);
            searchProcessTxt.SetWatermark("Enter process name or PID...");
            UpdateProcessListLayout();

            await RefreshProcessListAsync(showBusyState: true);
            if (!_isClosing)
                LoadSettings();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
                UpdateProcessListLayout();
        }

        private void UpdateProcessListLayout()
        {
            const int leftMargin = 12;
            const int rightMargin = 15;
            const int monitorHeight = 76;
            const int monitorBottomMargin = 28;
            const int listGap = 6;

            int contentWidth = Math.Max(300, ClientSize.Width - leftMargin - rightMargin);
            int monitorTop = Math.Max(
                processesListBox.Top + 120 + listGap,
                ClientSize.Height - monitorBottomMargin - monitorHeight);

            systemMonitorPanel.SetBounds(leftMargin, monitorTop, contentWidth, monitorHeight);
            processesListBox.SetBounds(
                leftMargin,
                processesListBox.Top,
                contentWidth,
                Math.Max(120, monitorTop - processesListBox.Top - listGap));

            ResizeProcessColumns();
        }

        private void ResizeProcessColumns()
        {
            if (processesListBox.Columns.Count != MinimumProcessColumnWidths.Length)
                return;

            int availableWidth = Math.Max(1, processesListBox.ClientSize.Width - 4);
            int minimumTotal = MinimumProcessColumnWidths.Sum();
            int preferredGrowth = PreferredProcessColumnWidths
                .Select((width, index) => width - MinimumProcessColumnWidths[index])
                .Sum();

            processesListBox.BeginUpdate();
            try
            {
                if (availableWidth <= minimumTotal)
                {
                    for (int index = 0; index < processesListBox.Columns.Count; index++)
                        processesListBox.Columns[index].Width = MinimumProcessColumnWidths[index];
                    return;
                }

                int extraWidth = availableWidth - minimumTotal;
                int assignedWidth = 0;
                for (int index = 0; index < processesListBox.Columns.Count - 1; index++)
                {
                    int growthWeight = PreferredProcessColumnWidths[index] - MinimumProcessColumnWidths[index];
                    int width = MinimumProcessColumnWidths[index]
                        + extraWidth * growthWeight / preferredGrowth;
                    processesListBox.Columns[index].Width = width;
                    assignedWidth += width;
                }

                processesListBox.Columns[^1].Width = Math.Max(
                    MinimumProcessColumnWidths[^1],
                    availableWidth - assignedWidth);
            }
            finally
            {
                processesListBox.EndUpdate();
            }
        }

        /// <summary>
        /// Load settings method. This method checks if the isLoadingSettings setting is enabled, and if so, it creates an instance of the ReadSettings class and calls its methods to read and apply the saved settings for boost, efficiency, affinity, priority, IO priority, and Wdptb (Windows Defender Process Threat Detection) for the processes displayed in the processesListBox. 
        /// The settings are applied to the processes list box, the count of processes label, and the search process text box in the main form.
        /// </summary>
        private void LoadSettings()
        {
            if (Properties.Settings.Default.isLoadingSettings)
            {
                var readSettings = new ReadSettings();
                readSettings.ReadSettingsBoost(processesListBox, countProcessesLbl, searchProcessTxt, this, "", true);
                readSettings.ReadSettingsEfficiency(processesListBox, countProcessesLbl, searchProcessTxt, this, "", true);
                readSettings.ReadSettingsAffinity(processesListBox, countProcessesLbl, searchProcessTxt, this, "", true);
                readSettings.ReadSettingsPriority(processesListBox, countProcessesLbl, searchProcessTxt, this, "", true);
                readSettings.ReadSettingsIOPriority(processesListBox, countProcessesLbl, searchProcessTxt, this, "", true);
                readSettings.ReadWdptbSettings(processesListBox, countProcessesLbl, searchProcessTxt, this, "", true);
                this.BeginInvoke(new Action(() =>
                {
                    var utils = new Utils.Utils();
                    utils.RefreshProcessList(this, processesListBox, countProcessesLbl);
                    utils.SearchProcess(searchProcessTxt, processesListBox);
                }));
            }
        }

        /// <summary>
        /// Load settings method with PID parameter. 
        /// This method checks if the provided PID is not null or empty, and if so, it creates an instance of the ReadSettings class and calls its methods to read and apply the saved settings for boost, efficiency, affinity, priority, IO priority, and Wdptb (Windows Defender Process Threat Detection) for the process with the specified PID.
        /// </summary>
        /// <param name="pid"></param>
        private void LoadSettings(string pid)
        {
            if (!string.IsNullOrEmpty(pid))
            {
                var processManage = new ProcessesManage();
                if (!processManage.IsPidValid(pid))
                {
                    MessageBox.Show("Invalid PID. Refresh process list!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var readSettings = new ReadSettings();
                readSettings.ReadSettingsBoost(processesListBox, countProcessesLbl, searchProcessTxt, this, pid, true);
                readSettings.ReadSettingsEfficiency(processesListBox, countProcessesLbl, searchProcessTxt, this, pid, true);
                readSettings.ReadSettingsAffinity(processesListBox, countProcessesLbl, searchProcessTxt, this, pid, true);
                readSettings.ReadSettingsPriority(processesListBox, countProcessesLbl, searchProcessTxt, this, pid, true);
                readSettings.ReadSettingsIOPriority(processesListBox, countProcessesLbl, searchProcessTxt, this, pid, true);
                readSettings.ReadWdptbSettings(processesListBox, countProcessesLbl, searchProcessTxt, this, pid, true);
                this.BeginInvoke(new Action(() =>
                {
                    var utils = new Utils.Utils();
                    utils.RefreshProcessList(this, processesListBox, countProcessesLbl);
                    utils.SearchProcess(searchProcessTxt, processesListBox);
                }));
            }
        }

        /// <summary>
        /// Save ListView position (top index and selected item).
        /// </summary>
        private void SaveListViewPosition()
        {
            _lastTopIndex = ListViewScroll.SaveTopIndex(processesListBox);

            int anchorIndex =
                processesListBox.SelectedIndices.Count > 0 ? processesListBox.SelectedIndices[0] :
                processesListBox.FocusedItem != null ? processesListBox.FocusedItem.Index :
                _lastTopIndex;

            _lastOffsetFromTop = anchorIndex - _lastTopIndex;

            // PID is in SubItems[1] in your code
            if (processesListBox.Items.Count > 0 && anchorIndex >= 0 && anchorIndex < processesListBox.Items.Count &&
                int.TryParse(processesListBox.Items[anchorIndex].SubItems[1].Text, out int pid))
                _lastPid = pid;
            else
                _lastPid = null;
        }

        /// <summary>
        /// Restore ListView position (top index and selected item).
        /// </summary>
        private void RestoreListViewPosition()
        {
            if (processesListBox.Items.Count == 0)
                return;

            // Try to restore by PID (best)
            if (_lastPid.HasValue)
            {
                string pidText = _lastPid.Value.ToString();
                ListViewItem? found = null;

                foreach (ListViewItem it in processesListBox.Items)
                {
                    if (it.SubItems.Count > 1 && it.SubItems[1].Text == pidText)
                    {
                        found = it;
                        break;
                    }
                }

                if (found != null)
                {
                    int desiredTop = Math.Max(0, found.Index - _lastOffsetFromTop);

                    ListViewScroll.RestoreTopIndex(processesListBox, desiredTop);

                    found.Selected = true;
                    found.Focused = true;
                    processesListBox.EnsureVisible(found.Index);
                    return;
                }
            }

            // Fallback: restore old top index (may be different item)
            ListViewScroll.RestoreTopIndex(processesListBox, _lastTopIndex);
        }

        /// <summary>
        /// Refresh process list button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void refreshProcessListBtn_Click(object sender, EventArgs e)
        {
            await RefreshProcessListAsync(showBusyState: true);
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

        private void SearchProcess(bool isMessage = true)
        {
            if (searchProcessTxt.Text.Length == 0)
                return;
            var searchProcess = new ProcessesManage();
            var search = searchProcessTxt.Text;
            searchProcess.SearchProcess(processesListBox, search, isMessage);
        }

        /// <summary>
        /// Captures process state off the UI thread and then performs one short UI update.
        /// </summary>
        private async Task RefreshProcessListAsync(bool showBusyState)
        {
            if (_isClosing)
                return;

            if (!await _refreshGate.WaitAsync(0))
            {
                if (showBusyState)
                    _refreshRequested = true;
                return;
            }

            string originalButtonText = refreshProcessListBtn.Text;
            try
            {
                do
                {
                    _refreshRequested = false;
                    if (showBusyState)
                    {
                        refreshProcessListBtn.Enabled = false;
                        refreshProcessListBtn.Text = "Refreshing...";
                    }

                    var manager = new ProcessesManage();
                    var snapshots = await Task.Run(
                        () => manager.CaptureProcesses(_refreshCancellation.Token),
                        _refreshCancellation.Token);

                    if (_isClosing || _refreshCancellation.IsCancellationRequested)
                        return;

                    // The user may open a menu while the background capture is running.
                    // Keep its target rows stable until the next refresh.
                    if (actionMenuStrip.Visible ||
                        (!showBusyState && !_refreshRequested &&
                         (_isPointerOverProcessList || WindowState == FormWindowState.Minimized)))
                        return;

                    _processSnapshots = snapshots;
                    ApplyCurrentFilter();
                }
                while (_refreshRequested && !_isClosing);
            }
            catch (OperationCanceledException) when (_refreshCancellation.IsCancellationRequested)
            {
                // The form is closing.
            }
            catch (Exception ex)
            {
                countProcessesLbl.Text = $"Unable to refresh processes: {ex.Message}";
            }
            finally
            {
                if (!_isClosing && !refreshProcessListBtn.IsDisposed)
                {
                    refreshProcessListBtn.Enabled = true;
                    refreshProcessListBtn.Text = originalButtonText;
                }
                _refreshGate.Release();
            }
        }

        internal void RequestProcessRefresh()
        {
            if (_isClosing)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(RequestProcessRefresh);
                return;
            }

            _refreshRequested = true;
            _ = RefreshProcessListAsync(showBusyState: true);
        }

        private void ApplyCurrentFilter()
        {
            SaveListViewPosition();

            string query = searchProcessTxt.Text.Trim();
            var visibleProcesses = query.Length == 0
                ? _processSnapshots
                : _processSnapshots.Where(process => process.Matches(query)).ToArray();

            var manager = new ProcessesManage();
            manager.DisplayProcesses(processesListBox, visibleProcesses);
            ResizeProcessColumns();

            countProcessesLbl.Text = query.Length == 0
                ? $"Processes running: {_processSnapshots.Count}"
                : $"Showing {visibleProcesses.Count} of {_processSnapshots.Count} processes";

            RestoreListViewPosition();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _isClosing = true;
            reloadProcess.Stop();
            _refreshCancellation.Cancel();
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Activate or deactivate search button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchProcessTxt_TextChanged(object sender, EventArgs e)
        {
            searchProcessBtn.Enabled = !string.IsNullOrWhiteSpace(searchProcessTxt.Text);
            if (_processSnapshots.Count > 0)
                ApplyCurrentFilter();
        }

        /// <summary>
        /// Load context menu on right click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void processesListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var clickedItem = processesListBox.GetItemAt(e.X, e.Y);
                if (clickedItem == null)
                {
                    processesListBox.SelectedItems.Clear();
                    return;
                }

                clickedItem.Selected = true;
                clickedItem.Focused = true;
            }
        }

        #region Boost Priority Menu Events
        /// <summary>
        /// Disable priority boost on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void enableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var boost = new Boost();
            boost.SetBoost(this, processesListBox, countProcessesLbl, searchProcessTxt, true);
        }

        /// <summary>
        /// Disable priority boost on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void disableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var boost = new Boost();
            boost.SetBoost(this, processesListBox, countProcessesLbl, searchProcessTxt, false);
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
            var ioPriority = new IOPriority();
            ioPriority.IOVeryLowPriority(this, processesListBox, countProcessesLbl, searchProcessTxt);
        }

        /// <summary>
        /// Set IO priority to Low on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ioPriority = new IOPriority();
            ioPriority.IOLowPriority(this, processesListBox, countProcessesLbl, searchProcessTxt);
        }

        /// <summary>
        /// Set IO priority to Normal on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ioPriority = new IOPriority();
            ioPriority.IONormalPriority(this, processesListBox, countProcessesLbl, searchProcessTxt);
        }


        /// <summary>
        /// Set IO priority to High on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void highToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ioPriority = new IOPriority();
            ioPriority.IOHighPriority(this, processesListBox, countProcessesLbl, searchProcessTxt);
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
            var processPriorty = new PriorityProcess();
            processPriorty.HighPriority(this, processesListBox, countProcessesLbl, searchProcessTxt);
        }

        /// <summary>
        /// Set process priority to Real Time on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void realTimedangerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var processPriorty = new PriorityProcess();
            processPriorty.RealTimePriority(this, processesListBox, countProcessesLbl, searchProcessTxt);
        }

        /// <summary>
        /// Set process priority to Above Normal on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboveNormalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var processPriorty = new PriorityProcess();
            processPriorty.AboveNormalPriority(this, processesListBox, countProcessesLbl, searchProcessTxt);
        }

        /// <summary>
        /// Set process priority to Normal on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void normalToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var processPriorty = new PriorityProcess();
            processPriorty.NormalPriority(this, processesListBox, countProcessesLbl, searchProcessTxt);
        }

        /// <summary>
        /// Set process priority to Below Normal on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void belowNormalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var processPriorty = new PriorityProcess();
            processPriorty.BelowNormalPriority(this, processesListBox, countProcessesLbl, searchProcessTxt);
        }

        private void idleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var processPriority = new PriorityProcess();
            processPriority.IdlePriority(this, processesListBox, countProcessesLbl, searchProcessTxt);
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
            var efficiency = new Efficiency();
            efficiency.EnableEfficiency(this, processesListBox, countProcessesLbl, searchProcessTxt);
        }


        /// <summary>
        /// Disable efficiency mode on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void disableToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var efficiency = new Efficiency();
            efficiency.DisableEfficiency(this, processesListBox, countProcessesLbl, searchProcessTxt);
        }
        #endregion

        /// <summary>
        /// Populate affinity choices when the context menu opens, including keyboard navigation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void actionMenuStrip_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (processesListBox.SelectedItems.Count == 0 ||
                processesListBox.SelectedItems[0].SubItems.Count < 2 ||
                !int.TryParse(processesListBox.SelectedItems[0].SubItems[1].Text, out int pid))
            {
                e.Cancel = true;
                return;
            }

            Affinity.PopulateMenu(afinityToolStripMenuItem, pid, CoreToolStripMenuItem_Click);
        }

        /// <summary>
        /// Set processor affinity (enable/disable cores) event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CoreToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            var affinity = new Affinity();
            affinity.SetAffinity(this, processesListBox, afinityToolStripMenuItem, countProcessesLbl, searchProcessTxt, sender);
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

        /// <summary>
        /// Hotkey process event.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.R:
                    if (!searchProcessTxt.Focused)
                        _ = RefreshProcessListAsync(showBusyState: true);
                    else
                        break;
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Reload process list timer tick event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void reloadProcess_Tick(object sender, EventArgs e)
        {
            if (_isPointerOverProcessList || actionMenuStrip.Visible || WindowState == FormWindowState.Minimized)
                return;

            await RefreshProcessListAsync(showBusyState: false);
        }

        /// <summary>
        /// Avoid replacing rows while the user is interacting with the process list.
        /// </summary>
        private void processesListBox_MouseEnter(object sender, EventArgs e) =>
            _isPointerOverProcessList = true;

        private void processesListBox_MouseLeave(object sender, EventArgs e) =>
            _isPointerOverProcessList = false;

        /// <summary>
        /// Handles the CheckedChanged event of checkBox1 by updating the application's loading settings preference.
        /// </summary>
        /// <remarks>This method synchronizes the value of the isLoadingSettings setting with the current
        /// checked state of checkBox1 and saves the updated setting. This ensures that the user's preference is
        /// persisted across application sessions.</remarks>
        /// <param name="sender">The source of the event, typically the CheckBox control whose checked state has changed.</param>
        /// <param name="e">An EventArgs object that contains the event data.</param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.isLoadingSettings = checkBox1.Checked;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Delete saved settings on selected process event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteSavedSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var deleteSettings = new DeleteSettings();
            deleteSettings.DeleteSettingsApp(this, processesListBox, countProcessesLbl, searchProcessTxt, GlobalVars.LogFilePath);
        }

        /// <summary>
        /// Show saved settings on selected process event. This method is currently empty and can be implemented to display the saved settings for the selected process when the corresponding menu item is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showSavedSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var readSettings = new ReadSettings();
            readSettings.ShowSettings(processesListBox, countProcessesLbl, searchProcessTxt, this, GlobalVars.LogFilePath);
        }

        /// <summary>
        /// Kill selected process event.
        /// This method creates an instance of the ProcessesManage class and calls its KillProcess method, passing the current form, the processes list box, the label for counting processes, and the search process text box as parameters.
        /// This allows the user to terminate the selected process from the context menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void killProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var processesManage = new ProcessesManage();
            processesManage.KillProcess(this, processesListBox, countProcessesLbl, searchProcessTxt);
        }

        /// <summary>
        /// Save settings on selected process event. This method updates the isSaveingSettings setting based on the checked state of the SaveSettingsCkb CheckBox control and saves the updated setting. 
        /// This allows the user to enable or disable the saving of settings for processes, and ensures that their preference is persisted across application sessions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveSettingsCkb_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.isSaveingSettings = SaveSettingsCkb.Checked;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Eable or disable priority boost on selected process event. 
        /// This method creates an instance of the PriorityProcess class and calls its SetThreadPriorityBoost method, passing the current form, the processes list box, the label for counting processes, the search process text box, and a boolean value indicating whether to enable or disable the priority boost as parameters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void enableToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            var prirityBoost = new PriorityProcess();
            prirityBoost.SetThreadPriorityBoost(this, processesListBox, countProcessesLbl, searchProcessTxt, true);
        }

        /// <summary>
        /// Disable priority boost on selected process event. 
        /// This method creates an instance of the PriorityProcess class and calls its SetThreadPriorityBoost method, passing the current form, the processes list box, the label for counting processes, the search process text box, and a boolean value indicating that the priority boost should be disabled as parameters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void disableToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            var prirityBoost = new PriorityProcess();
            prirityBoost.SetThreadPriorityBoost(this, processesListBox, countProcessesLbl, searchProcessTxt, false);
        }

        /// <summary>
        /// Load saved settings on selected process event. This method is currently empty and can be implemented to load the saved settings for the selected process when the corresponding menu item is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadSavedSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Invoke(delegate
            {
                var selectedProcesses = processesListBox.SelectedItems;
                if (selectedProcesses.Count > 0)
                {
                    var pid = selectedProcesses[0].SubItems[1].Text;
                    LoadSettings(pid);
                }
            });
        }

        private void aboutButton_Click(object sender, EventArgs e)
        {
            using var about = new AboutForm();
            about.ShowDialog(this);
        }
    }
}
