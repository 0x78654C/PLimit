using System.Diagnostics;

namespace PLimit.Utils
{
    public class Utils
    {
        private int _lastTopIndex;
        private int _lastOffsetFromTop;
        private int? _lastPid;

        public Utils() { }
        public void SearchProcess(TextBox searchProcessTxt, DoubleBufferedListView processesListBox,  bool isMessage = true)
        {
            if (searchProcessTxt.Text.Length == 0)
                return;
            var searchProcess = new ProcessesManage();
            var search = searchProcessTxt.Text;
            searchProcess.SearchProcess(ref processesListBox, search, isMessage);
        }

        /// <summary>
        /// Refresh process list method.
        /// </summary>
        public void RefreshProcessList(Form form, DoubleBufferedListView processesListBox, Label countProcessesLbl)
        {
            form.Invoke(delegate
            {
                SaveListViewPosition(processesListBox);
                var getProcesses = new ProcessesManage();
                getProcesses.GetProcesses(ref processesListBox);
                countProcessesLbl.Text = $"Processes running: {processesListBox.Items.Count}";
                RestoreListViewPosition(processesListBox);
            });
        }

        /// <summary>
        /// Save ListView position (top index and selected item).
        /// </summary>
        public void SaveListViewPosition(DoubleBufferedListView processesListBox)
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
        private void RestoreListViewPosition(DoubleBufferedListView processesListBox)
        {
            if (processesListBox.Items.Count == 0)
                return;

            // Try to restore by PID (best)
            if (_lastPid.HasValue)
            {
                string pidText = _lastPid.Value.ToString();
                ListViewItem found = null;

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
    }
}
