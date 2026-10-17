using PLimit.Utils;

namespace PLimit;

internal sealed partial class AutomationForm : Form
{
    private readonly AutomationSettings _settings;
    private readonly CheckBox _boost = new() { Text = "Boost the foreground application to Above Normal", AutoSize = true };
    private readonly CheckBox _balance = new() { Text = "Balance CPU-heavy background applications", AutoSize = true };
    private readonly NumericUpDown _system = Number(20, 100);
    private readonly NumericUpDown _process = Number(1, 100);
    private readonly NumericUpDown _seconds = Number(1, 60);
    private readonly TextBox _activity = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Dock = DockStyle.Fill };
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1000 };

    public AutomationForm(ProcessAutomationService service)
    {
        string? configurationError = null;
        try { _settings = AutomationSettings.Load(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidDataException)
        {
            _settings = service.GetSettingsSnapshot();
            configurationError = $"Cannot read automation.json: {ex.Message}\nShowing the last valid configuration. Save replaces the invalid file.";
        }
        Text = "Automation & Rules";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(880, 590);
        ClientSize = new Size(990, 640);
        Font = new Font("Segoe UI", 10);
        var tabs = new TabControl { Dock = DockStyle.Fill };
        var general = new TabPage("Responsiveness") { Padding = new Padding(16), AutoScroll = true };
        var rules = new TabPage("Process rules") { Padding = new Padding(12) };
        var activity = new TabPage("Activity") { Padding = new Padding(12) };
        tabs.TabPages.AddRange([general, rules, activity]);
        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        var save = new Button { Text = "Save", AutoSize = true };
        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        footer.Controls.AddRange([save, cancel]);
        Controls.Add(tabs);
        Controls.Add(footer);
        if (configurationError != null)
        {
            var error = Description(configurationError);
            error.Dock = DockStyle.Top;
            error.Padding = new Padding(12);
            Controls.Add(error);
        }
        AcceptButton = save;
        CancelButton = cancel;

        var options = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
        options.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        options.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        AddWide(options, _boost);
        AddWide(options, Description("Temporarily raises Normal-priority apps while they have input focus. Restores priority when focus changes. Saved priorities and efficiency rules take precedence."));
        AddWide(options, _balance);
        AddWide(options, Description("Temporarily lowers busy background apps to Below Normal during sustained CPU pressure. Restores them after load subsides or they gain focus."));
        AddRow(options, "System CPU threshold (%)", _system);
        AddRow(options, "Per-process CPU threshold (% of total capacity)", _process);
        AddRow(options, "Sustained load before balancing (seconds)", _seconds);
        AddWide(options, Description("Automation operates on accessible applications in your session. PLimit, critical Windows processes, High/Real Time priorities and excluded apps are not automatically boosted or balanced. Both features start disabled."));
        AddWide(options, Description("Rules run while PLimit is open, including when minimized. Closing PLimit restores temporary priority and affinity changes. Explicit saved settings remain applied."));
        general.Controls.Add(options);
        _boost.Checked = _settings.ForegroundBoostEnabled;
        _balance.Checked = _settings.BalanceEnabled;
        _system.Value = _settings.BalanceSystemThreshold;
        _process.Value = _settings.BalanceProcessThreshold;
        _seconds.Value = _settings.BalanceOverSeconds;

        InitializeRulesTab(rules);

        activity.Controls.Add(_activity);
        var logNote = Description("Last 250 events from this session, newest first. Access failures are rate limited. Temporary changes and restorations are recorded here.");
        logNote.Dock = DockStyle.Top;
        activity.Controls.Add(logNote);
        void RefreshLog()
        {
            string log = string.Join(Environment.NewLine, service.GetLog());
            if (_activity.Text != log) _activity.Text = log;
        }
        _timer.Tick += (_, _) => RefreshLog();
        RefreshLog();
        _timer.Start();
        save.Click += (_, _) =>
        {
            _settings.ForegroundBoostEnabled = _boost.Checked;
            _settings.BalanceEnabled = _balance.Checked;
            _settings.BalanceSystemThreshold = (int)_system.Value;
            _settings.BalanceProcessThreshold = (int)_process.Value;
            _settings.BalanceOverSeconds = (int)_seconds.Value;
            try { _settings.Save(); DialogResult = DialogResult.OK; }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Unable to save automation", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        };
        DarkTheme.Apply(this);
        foreach (TabPage tab in tabs.TabPages) tab.BackColor = DarkTheme.Background;
    }

    internal static NumericUpDown Number(int min, int max) => new() { Minimum = min, Maximum = max, Width = 86 };
    internal static Label Description(string text) => new() { Text = text, AutoSize = true, MaximumSize = new Size(680, 0), Margin = new Padding(0, 8, 0, 18) };
    internal static void AddWide(TableLayoutPanel table, Control control)
    {
        int row = table.RowCount++;
        table.Controls.Add(control, 0, row);
        table.SetColumnSpan(control, 2);
    }
    internal static void AddRow(TableLayoutPanel table, string text, Control control)
    {
        int row = table.RowCount++;
        table.Controls.Add(new Label { Text = text, AutoSize = true, Margin = new Padding(0, 5, 10, 10) }, 0, row);
        table.Controls.Add(control, 1, row);
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) _timer.Dispose();
        base.Dispose(disposing);
    }
}

internal sealed class ProcessRuleForm : Form
{
    public AutomationRule Rule { get; private set; }

    public ProcessRuleForm(AutomationRule original, Func<AutomationRule, string?>? acceptRule = null)
    {
        Rule = original;
        Text = "Process automation rule";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(720, 760);
        MinimumSize = new Size(680, 580);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = MinimizeBox = false;
        Font = new Font("Segoe UI", 10);
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        var table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(18), ColumnCount = 2 };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        var name = new TextBox { Name = "processPattern", Text = original.ProcessName, Width = 210, MaxLength = 260 };
        var enabled = new CheckBox { Name = "ruleEnabled", Text = "Enable this rule", AutoSize = true, Checked = original.Enabled };
        var boost = new CheckBox { Text = "Exclude from foreground boost", AutoSize = true, Checked = original.ExcludeForegroundBoost };
        var balance = new CheckBox { Text = "Exclude from background balancing", AutoSize = true, Checked = original.ExcludeBalance };
        var limit = new CheckBox { Text = "Enable CPU limiter", AutoSize = true, Checked = original.CpuLimit != null };
        var config = original.CpuLimit ?? new CpuLimitRule();
        var threshold = AutomationForm.Number(1, 100); threshold.Value = config.ThresholdPercent;
        var over = AutomationForm.Number(1, 300); over.Value = config.OverSeconds;
        var cores = AutomationForm.Number(1, 64); cores.Value = config.CoreCount;
        var duration = AutomationForm.Number(1, 3600); duration.Value = config.DurationSeconds;
        var cooldown = AutomationForm.Number(0, 3600); cooldown.Value = config.CooldownSeconds;
        var background = new CheckBox { Name = "limitOnlyBackground", Text = "Limit only while in the background", AutoSize = true, Checked = config.OnlyWhenBackground };
        AutomationForm.AddWide(table, enabled);
        AutomationForm.AddRow(table, "Process name or pattern", name);
        AutomationForm.AddWide(table, AutomationForm.Description("Examples: chrome, game*.exe, worker?.exe. Matching ignores case and the .exe suffix. The first enabled matching rule in the list wins."));
        AutomationForm.AddWide(table, boost);
        AutomationForm.AddWide(table, balance);
        AutomationForm.AddWide(table, limit);
        AutomationForm.AddRow(table, "Trigger CPU (% of total capacity)", threshold);
        AutomationForm.AddRow(table, "Time above threshold (seconds)", over);
        AutomationForm.AddRow(table, "Keep this many of its allowed cores", cores);
        AutomationForm.AddRow(table, "Limit duration (seconds)", duration);
        AutomationForm.AddRow(table, "Cooldown after restoring (seconds)", cooldown);
        AutomationForm.AddWide(table, background);
        AutomationForm.AddWide(table, AutomationForm.Description("The limiter restores original affinity when its duration ends or a background-only app gains focus. After cooldown, a fresh sustained-load interval is required. This limits cores, not an exact CPU percentage."));
        if (!WindowsAutomationProcess.SupportsAffinity)
            AutomationForm.AddWide(table, AutomationForm.Description("CPU limiter is unavailable on this system's multiple processor groups. Rules can still be saved or exported for another system."));
        var matches = AutomationForm.Description("");
        matches.Name = "ruleMatches";
        var refresh = new Button { Text = "Refresh running matches", AutoSize = true };
        AutomationForm.AddWide(table, refresh);
        AutomationForm.AddWide(table, matches);
        var error = AutomationForm.Description("");
        error.Name = "ruleError";
        AutomationForm.AddWide(table, error);
        var actions = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Bottom, Padding = new Padding(8) };
        var save = new Button { Name = "acceptRule", Text = "OK", AutoSize = true };
        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        actions.Controls.AddRange([save, cancel]);
        scroll.Controls.Add(table);
        Controls.Add(scroll);
        Controls.Add(actions);
        AcceptButton = save;
        CancelButton = cancel;
        void UpdateEnabled() { threshold.Enabled = over.Enabled = cores.Enabled = duration.Enabled = cooldown.Enabled = background.Enabled = limit.Checked; }
        limit.CheckedChanged += (_, _) => UpdateEnabled();
        UpdateEnabled();
        var processes = RulePreviewProcess.Capture();
        void Preview()
        {
            try
            {
                ProcessRuleMatcher.Validate(name.Text);
                var found = processes.Where(p => ProcessRuleMatcher.Matches(name.Text, p.Name)).ToArray();
                matches.Text = $"{found.Length} running matches (before rule precedence and access checks):\n"
                    + (found.Length == 0 ? "No current matches; future instances can still match." : string.Join(", ", found.Take(6).Select(p => $"{p.Name} ({p.Pid})")))
                    + (found.Length > 6 ? $" …and {found.Length - 6} more." : "");
                error.Text = "";
            }
            catch (InvalidDataException ex) { matches.Text = ""; error.Text = ex.Message; }
        }
        name.TextChanged += (_, _) => Preview();
        refresh.Click += (_, _) => { processes = RulePreviewProcess.Capture(); Preview(); };
        Preview();
        save.Click += (_, _) =>
        {
            try
            {
                ProcessRuleMatcher.Validate(name.Text);
                var candidate = new AutomationRule
                {
                    ProcessName = ProcessRuleMatcher.NormalizePattern(name.Text), Enabled = enabled.Checked,
                    ExcludeForegroundBoost = boost.Checked, ExcludeBalance = balance.Checked,
                    CpuLimit = limit.Checked ? new CpuLimitRule
                    {
                        ThresholdPercent = (int)threshold.Value, OverSeconds = (int)over.Value,
                        CoreCount = (int)cores.Value, DurationSeconds = (int)duration.Value,
                        CooldownSeconds = (int)cooldown.Value, OnlyWhenBackground = background.Checked
                    } : null
                };
                string? validationError = acceptRule?.Invoke(candidate);
                if (validationError != null) { error.Text = validationError; return; }
                Rule = candidate;
                DialogResult = DialogResult.OK;
            }
            catch (InvalidDataException ex) { error.Text = ex.Message; }
        };
        DarkTheme.Apply(this);
        error.ForeColor = Color.Salmon;
    }
}
