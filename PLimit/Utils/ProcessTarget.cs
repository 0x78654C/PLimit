namespace PLimit.Utils
{
    /// <summary>
    /// Resolves either the current UI selection or a PID supplied while loading
    /// saved settings. The process name is present only for direct user actions.
    /// </summary>
    internal readonly record struct ProcessTarget(int ProcessId, string? ProcessName)
    {
        public bool IsUserAction => ProcessName != null;

        public static bool TryResolve(
            DoubleBufferedListView processesListBox,
            string pid,
            out ProcessTarget target)
        {
            string processIdText;
            string? processName = null;

            if (string.IsNullOrWhiteSpace(pid))
            {
                if (processesListBox.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Select a process first.", "Process Limiter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    target = default;
                    return false;
                }

                var selectedItem = processesListBox.SelectedItems[0];
                if (selectedItem.SubItems.Count < 2)
                {
                    MessageBox.Show("The selected process data is incomplete. Refresh the process list.", "Process Limiter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    target = default;
                    return false;
                }

                processName = selectedItem.SubItems[0].Text;
                processIdText = selectedItem.SubItems[1].Text;
            }
            else
            {
                processIdText = pid;
            }

            if (!int.TryParse(processIdText, out int processId) || processId <= 0)
            {
                MessageBox.Show("Invalid PID. Refresh the process list!", "Process Limiter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                target = default;
                return false;
            }

            target = new ProcessTarget(processId, processName);
            return true;
        }
    }
}
