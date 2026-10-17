using PLimit.Utils;

namespace PLimit;

public partial class MainForm
{
    private ProcessAutomationService? _automation;
    private readonly Label _automationStatus = new() { AutoSize = true, Margin = new Padding(12, 7, 0, 0) };
    private System.Windows.Forms.Timer? _automationStatusTimer;
    private bool _closeAfterAutomation;

    private void InitializeAutomation()
    {
        var bar = new FlowLayoutPanel
        {
            Name = "automationBar", Location = new Point(12, 42), Size = new Size(ClientSize.Width - 27, 32),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, WrapContents = false
        };
        var button = new Button { Text = "Automation & Rules…", UseMnemonic = false, AutoSize = true, Height = 28 };
        bar.Controls.AddRange([button, _automationStatus]);
        Controls.Add(bar);
        processesListBox.Top = 80;
        checkBox1.Text = "Enforce saved settings";
        boostToolStripMenuItem.Text = "Windows Dynamic Priority Boost";
        windowsDynamicThreadPriorityBoostWDTPBToolStripMenuItem.Visible = false; // Same process-wide Windows setting.
        wdtpb.Text = "Thread Boost";
        actionMenuStrip.Items.Insert(0, new ToolStripMenuItem("Automation rule…", null, (_, _) => EditSelectedAutomationRule()));
        button.Click += (_, _) =>
        {
            if (_automation == null) return;
            try { using var form = new AutomationForm(_automation); form.ShowDialog(this); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Unable to read automation settings", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        };
    }

    private void StartAutomation()
    {
        _automation = new ProcessAutomationService { EnforceSavedSettings = checkBox1.Checked };
        _automation.Start();
        _automationStatusTimer = new System.Windows.Forms.Timer(components) { Interval = 1000 };
        _automationStatusTimer.Tick += (_, _) => _automationStatus.Text = _automation.Status;
        _automationStatusTimer.Start();
    }

    private void EditSelectedAutomationRule()
    {
        if (processesListBox.SelectedItems.Count == 0) return;
        try
        {
            string name = processesListBox.SelectedItems[0].Text;
            var settings = AutomationSettings.Load();
            var rule = settings.FindRule(name);
            using var editor = new ProcessRuleForm(rule ?? new AutomationRule { ProcessName = name });
            if (editor.ShowDialog(this) != DialogResult.OK) return;
            if (rule != null) settings.Rules.Remove(rule);
            settings.Rules.Add(editor.Rule);
            settings.Save();
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Unable to save automation rule", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
