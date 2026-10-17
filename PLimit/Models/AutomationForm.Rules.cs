using PLimit.Utils;

namespace PLimit;

internal sealed partial class AutomationForm
{
    private readonly ListView _rules = new()
    {
        Name = "rulesList", View = View.Details, FullRowSelect = true, MultiSelect = false,
        Dock = DockStyle.Fill, HideSelection = false, CheckBoxes = true
    };
    private readonly TextBox _ruleSearch = new() { Name = "ruleSearch", Width = 220, PlaceholderText = "Filter rules…" };
    private readonly TextBox _matchPreview = new()
    {
        Name = "matchPreview", Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
        Dock = DockStyle.Fill
    };
    private readonly Label _ruleSummary = new() { AutoSize = true, Margin = new Padding(12, 8, 0, 0) };
    private readonly Dictionary<string, Button> _ruleButtons = new();
    private IReadOnlyList<RulePreviewProcess> _previewProcesses = Array.Empty<RulePreviewProcess>();
    private bool _refreshingRules;

    private AutomationRule? SelectedRule => _rules.SelectedItems.Count == 0 ? null : _rules.SelectedItems[0].Tag as AutomationRule;

    private void InitializeRulesTab(TabPage page)
    {
        _rules.Columns.Add("Order / Enabled", 110);
        _rules.Columns.Add("Process pattern", 180);
        _rules.Columns.Add("Exclusions", 140);
        _rules.Columns.Add("CPU limiter", 420);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 115));
        var help = Description("First enabled matching rule wins. Use * for any characters and ? for one character. Uncheck to pause a rule; Move up/down changes precedence. Save applies all edits.");
        help.MaximumSize = new Size(900, 0);
        layout.Controls.Add(help, 0, 0);
        var search = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        search.Controls.AddRange([_ruleSearch, _ruleSummary]);
        layout.Controls.Add(search, 0, 1);
        layout.Controls.Add(_rules, 0, 2);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        void Add(string name, string text, Action action)
        {
            var button = new Button { Name = name, Text = text, AutoSize = true, MinimumSize = new Size(75, 28) };
            button.Click += (_, _) => action();
            _ruleButtons[name] = button;
            actions.Controls.Add(button);
        }
        Add("addRule", "Add…", () => EditRule(null));
        Add("editRule", "Edit…", () => { if (SelectedRule is { } rule) EditRule(rule); });
        Add("removeRule", "Remove", () =>
        {
            if (SelectedRule is not { } rule) return;
            _settings.Rules.Remove(rule);
            RefreshRules();
        });
        Add("moveRuleUp", "Move up", () => MoveRule(-1));
        Add("moveRuleDown", "Move down", () => MoveRule(1));
        Add("importRules", "Import…", ImportRules);
        Add("exportRules", "Export…", ExportRules);
        Add("refreshMatches", "Refresh matches", () => { _previewProcesses = RulePreviewProcess.Capture(); UpdateRuleSelection(); });
        layout.Controls.Add(actions, 0, 3);
        layout.Controls.Add(_matchPreview, 0, 4);
        page.Controls.Add(layout);
        _rules.DoubleClick += (_, _) => { if (SelectedRule is { } rule) EditRule(rule); };
        _rules.SelectedIndexChanged += (_, _) => { if (!_refreshingRules) UpdateRuleSelection(); };
        _rules.ItemChecked += (_, e) =>
        {
            if (_refreshingRules || e.Item.Tag is not AutomationRule rule) return;
            rule.Enabled = e.Item.Checked;
            UpdateRuleSelection();
        };
        _ruleSearch.TextChanged += (_, _) => RefreshRules();
        _previewProcesses = RulePreviewProcess.Capture();
        RefreshRules();
    }

    private void EditRule(AutomationRule? existing)
    {
        // Validate against the entire list before the editor closes; a duplicate never discards typed changes.
        using var editor = new ProcessRuleForm(existing ?? new AutomationRule(), candidate =>
        {
            try { _settings.SetRule(candidate, existing); return null; }
            catch (InvalidDataException ex) { return ex.Message; }
        });
        if (editor.ShowDialog(this) != DialogResult.OK) return;
        _ruleSearch.Clear();
        RefreshRules(editor.Rule);
    }

    private void MoveRule(int direction)
    {
        if (SelectedRule is not { } rule || _ruleSearch.TextLength != 0) return;
        int index = _settings.Rules.IndexOf(rule), next = index + direction;
        if (next < 0 || next >= _settings.Rules.Count) return;
        (_settings.Rules[index], _settings.Rules[next]) = (_settings.Rules[next], _settings.Rules[index]);
        RefreshRules(rule);
    }

    private void RefreshRules(AutomationRule? selected = null)
    {
        selected ??= SelectedRule;
        _refreshingRules = true;
        _rules.BeginUpdate();
        try
        {
            _rules.Items.Clear();
            for (int index = 0; index < _settings.Rules.Count; index++)
            {
                var rule = _settings.Rules[index];
                if (!rule.ProcessName.Contains(_ruleSearch.Text.Trim(), StringComparison.OrdinalIgnoreCase)) continue;
                string exclusions = string.Join(", ", new[] { rule.ExcludeForegroundBoost ? "Boost" : null, rule.ExcludeBalance ? "Balance" : null }.Where(s => s != null));
                string limit = rule.CpuLimit is { } cpu
                    ? $"≥{cpu.ThresholdPercent}% / {cpu.OverSeconds}s → {cpu.CoreCount} cores / {cpu.DurationSeconds}s; rest {cpu.CooldownSeconds}s{(cpu.OnlyWhenBackground ? "; background" : "")}" : "Off";
                var item = new ListViewItem([(index + 1).ToString(), rule.ProcessName, exclusions, limit])
                    { Tag = rule, Checked = rule.Enabled };
                _rules.Items.Add(item);
                if (rule == selected) { item.Selected = true; item.Focused = true; }
            }
        }
        finally { _rules.EndUpdate(); _refreshingRules = false; }
        UpdateRuleSelection();
    }

    private void UpdateRuleSelection()
    {
        var rule = SelectedRule;
        _ruleSummary.Text = $"{_settings.Rules.Count(r => r.Enabled)} enabled / {_settings.Rules.Count} rules";
        _ruleButtons["editRule"].Enabled = _ruleButtons["removeRule"].Enabled = rule != null;
        int index = rule == null ? -1 : _settings.Rules.IndexOf(rule);
        _ruleButtons["moveRuleUp"].Enabled = _ruleSearch.TextLength == 0 && index > 0;
        _ruleButtons["moveRuleDown"].Enabled = _ruleSearch.TextLength == 0 && index >= 0 && index < _settings.Rules.Count - 1;
        if (rule == null)
        {
            _matchPreview.Text = "Select a rule to preview running matches and precedence. Refresh matches updates the process snapshot.\r\nPreview includes protected processes; automation still respects access and session restrictions.";
            return;
        }
        var matches = _previewProcesses.Where(p => rule.Matches(p.Name)).ToArray();
        int effective = matches.Count(p => _settings.ResolveRule(p.Name) == rule);
        var lines = matches.Take(60).Select(p =>
        {
            var winner = _settings.ResolveRule(p.Name);
            string state = !rule.Enabled ? "rule paused" : winner == rule ? "this rule wins" : $"overridden by '{winner?.ProcessName}'";
            return $"{p.Name} ({p.Pid}) — {state}";
        });
        _matchPreview.Text = $"{matches.Length} running matches; {effective} use this rule. Access/session restrictions still apply.\r\n"
            + (matches.Length == 0 ? "No current matches. The rule will also match future instances." : string.Join("\r\n", lines))
            + (matches.Length > 60 ? $"\r\n…and {matches.Length - 60} more." : "");
    }

    private void ImportRules()
    {
        using var dialog = new OpenFileDialog { Filter = "PLimit rules (*.json)|*.json", Title = "Import rules" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var result = _settings.ImportRules(dialog.FileName);
            _ruleSearch.Clear();
            RefreshRules();
            MessageBox.Show(this, $"Added {result.Added} rules; skipped {result.Skipped} existing patterns. Existing rules retain precedence. Click Save to apply.", "Import rules");
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Unable to import rules", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void ExportRules()
    {
        using var dialog = new SaveFileDialog { Filter = "PLimit rules (*.json)|*.json", FileName = "plimit-rules.json", DefaultExt = "json", Title = "Export rules" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { _settings.ExportRules(dialog.FileName); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Unable to export rules", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
