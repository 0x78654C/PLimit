using PLimit.Utils;
using System.Text.Json;
using Xunit;

namespace PLimit.Tests;

/// <summary>
/// Tests for StoreSettings. Each test uses an isolated temp directory so that
/// tests are independent of each other and of the real settings file.
/// </summary>
public class StoreSettingsTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _tempFile;
    private readonly string _savedDir;
    private readonly string _savedFile;

    public StoreSettingsTests()
    {
        // Redirect GlobalVars to a fresh temp directory for isolation.
        _savedDir  = GlobalVars.LogDirPath;
        _savedFile = GlobalVars.LogFilePath;

        _tempDir  = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _tempFile = Path.Combine(_tempDir, "processes.json");

        GlobalVars.LogDirPath  = _tempDir;
        GlobalVars.LogFilePath = _tempFile;
    }

    public void Dispose()
    {
        GlobalVars.LogDirPath  = _savedDir;
        GlobalVars.LogFilePath = _savedFile;

        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private static StoreSettings Sut() => new();

    [Fact]
    public void ReadSettingsPriority_AppliesSavedIdlePriority() => WindowsTest.Run(() => WindowsTest.WithProcess(process =>
    {
        Sut().UpdateSetting(StoreSettings.SettingType.Priority, process.ProcessName, "Idle");
        using var list = new DoubleBufferedListView();
        list.Items.Add(new ListViewItem(new[] { process.ProcessName, process.Id.ToString() }));

        new ReadSettings().ReadSettingsPriority(list, null!, null!, null!, process.Id.ToString(), isStartUp: true);

        process.Refresh();
        Assert.Equal(System.Diagnostics.ProcessPriorityClass.Idle, process.PriorityClass);
    }));

    // ── UpdateSetting — new process ───────────────────────────────────────

    [Fact]
    public void UpdateSetting_NewProcess_CreatesEntryInFile()
    {
        Sut().UpdateSetting(StoreSettings.SettingType.Boosted, "notepad", "True");

        var data = Json.JsonManage.ReadJsonFromFile<List<ProcessData>>(_tempFile);
        Assert.Single(data);
        Assert.Equal("notepad", data[0].ProcessName);
        Assert.Equal(SettingState.Enabled, data[0].Boosted);
    }

    [Theory]
    [InlineData(StoreSettings.SettingType.Boosted,    "True",     nameof(ProcessData.Boosted))]
    [InlineData(StoreSettings.SettingType.IOPriority, "High",     nameof(ProcessData.IOProperty))]
    [InlineData(StoreSettings.SettingType.Priority,   "RealTime", nameof(ProcessData.Property))]
    [InlineData(StoreSettings.SettingType.Affinity,   "15",       nameof(ProcessData.Affinity))]
    [InlineData(StoreSettings.SettingType.Efficiency, "Enabled",  nameof(ProcessData.Efficiency))]
    [InlineData(StoreSettings.SettingType.Wdptb,      "Disabled", nameof(ProcessData.Wdptb))]
    public void UpdateSetting_SetsCorrectField(
        StoreSettings.SettingType settingType, string value, string propertyName)
    {
        Sut().UpdateSetting(settingType, "proc", value);

        var data   = Json.JsonManage.ReadJsonFromFile<List<ProcessData>>(_tempFile);
        var actual = typeof(ProcessData).GetProperty(propertyName)!.GetValue(data[0]) as string;
        string expected = settingType == StoreSettings.SettingType.Boosted
            ? SettingState.Enabled
            : value;
        Assert.Equal(expected, actual);
    }

    // ── UpdateSetting — existing process ──────────────────────────────────

    [Fact]
    public void UpdateSetting_ExistingProcess_UpdatesValueInPlace()
    {
        var sut = Sut();
        sut.UpdateSetting(StoreSettings.SettingType.Boosted, "chrome", "False");
        sut.UpdateSetting(StoreSettings.SettingType.Boosted, "chrome", "True");

        var data = Json.JsonManage.ReadJsonFromFile<List<ProcessData>>(_tempFile);
        Assert.Single(data);              // still one entry (no duplicate)
        Assert.Equal(SettingState.Enabled, data[0].Boosted);
    }

    [Fact]
    public void UpdateSetting_NewProcess_WritesOnlyConfiguredSettings()
    {
        Sut().UpdateSetting(StoreSettings.SettingType.Boosted, "notepad", "False");

        using var document = JsonDocument.Parse(File.ReadAllText(_tempFile));
        var setting = Assert.Single(document.RootElement.EnumerateArray());

        Assert.Equal(2, setting.EnumerateObject().Count());
        Assert.Equal("notepad", setting.GetProperty(nameof(ProcessData.ProcessName)).GetString());
        Assert.Equal(SettingState.Disabled, setting.GetProperty(nameof(ProcessData.Boosted)).GetString());
        Assert.False(setting.TryGetProperty(nameof(ProcessData.IOProperty), out _));
        Assert.False(setting.TryGetProperty(nameof(ProcessData.Property), out _));
        Assert.False(setting.TryGetProperty(nameof(ProcessData.Affinity), out _));
        Assert.False(setting.TryGetProperty(nameof(ProcessData.Efficiency), out _));
        Assert.False(setting.TryGetProperty(nameof(ProcessData.Wdptb), out _));
    }

    [Fact]
    public void UpdateSetting_DisabledBoost_WritesDisabledInsteadOfNull()
    {
        Sut().UpdateSetting(StoreSettings.SettingType.Boosted, "game", "False");

        var data = Json.JsonManage.ReadJsonFromFile<List<ProcessData>>(_tempFile);
        var json = File.ReadAllText(_tempFile);

        Assert.Equal(SettingState.Disabled, data[0].Boosted);
        Assert.Contains("\"Boosted\": \"Disabled\"", json);
        Assert.DoesNotContain("\"Boosted\": null", json);
    }

    [Fact]
    public void UpdateSetting_ExistingLegacyNulls_AreOmittedOnRewrite()
    {
        Directory.CreateDirectory(_tempDir);
        File.WriteAllText(_tempFile, """
            [null,{"ProcessName":"game","Boosted":null,"IOProperty":null,"Property":null,"Affinity":null,"Efficiency":null,"Wdptb":null}]
            """);

        Sut().UpdateSetting(StoreSettings.SettingType.Boosted, "game", "Disabled");

        var data = Json.JsonManage.ReadJsonFromFile<List<ProcessData>>(_tempFile);
        var json = File.ReadAllText(_tempFile);

        Assert.Single(data);
        Assert.Equal(SettingState.Disabled, data[0].Boosted);
        Assert.DoesNotContain("null", json);
        Assert.DoesNotContain(nameof(ProcessData.IOProperty), json);
        Assert.DoesNotContain(nameof(ProcessData.Property), json);
        Assert.DoesNotContain(nameof(ProcessData.Affinity), json);
        Assert.DoesNotContain(nameof(ProcessData.Efficiency), json);
        Assert.DoesNotContain(nameof(ProcessData.Wdptb), json);
    }

    [Fact]
    public void UpdateSetting_ExistingEmptyValues_AreOmittedOnRewrite()
    {
        Directory.CreateDirectory(_tempDir);
        File.WriteAllText(_tempFile, """
            [{"ProcessName":"game","Boosted":"","IOProperty":"","Property":"","Affinity":"","Efficiency":"","Wdptb":""}]
            """);

        Sut().UpdateSetting(StoreSettings.SettingType.Priority, "game", "High");

        using var document = JsonDocument.Parse(File.ReadAllText(_tempFile));
        var setting = Assert.Single(document.RootElement.EnumerateArray());

        Assert.Equal(2, setting.EnumerateObject().Count());
        Assert.Equal("High", setting.GetProperty(nameof(ProcessData.Property)).GetString());
    }

    [Fact]
    public void UpdateSetting_MatchesProcessNamesCaseInsensitively()
    {
        var sut = Sut();
        sut.UpdateSetting(StoreSettings.SettingType.Boosted, "Game", "Enabled");
        sut.UpdateSetting(StoreSettings.SettingType.Boosted, "game", "Disabled");

        var data = Json.JsonManage.ReadJsonFromFile<List<ProcessData>>(_tempFile);

        Assert.Single(data);
        Assert.Equal(SettingState.Disabled, data[0].Boosted);
    }

    [Fact]
    public void UpdateSetting_TwoDistinctProcesses_BothPersist()
    {
        var sut = Sut();
        sut.UpdateSetting(StoreSettings.SettingType.Priority, "proc1", "High");
        sut.UpdateSetting(StoreSettings.SettingType.Priority, "proc2", "Normal");

        var data = Json.JsonManage.ReadJsonFromFile<List<ProcessData>>(_tempFile);
        Assert.Equal(2, data.Count);
    }

    // ── UpdateSetting — invalid type ──────────────────────────────────────

    [Fact]
    public void UpdateSetting_InvalidSettingType_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Sut().UpdateSetting((StoreSettings.SettingType)999, "proc", "val"));
    }

    // ── GetSetting ────────────────────────────────────────────────────────

    [Fact]
    public void GetSetting_ReturnsStoredValue()
    {
        Sut().UpdateSetting(StoreSettings.SettingType.IOPriority, "explorer", "Low");

        var result = Sut().GetSetting("explorer", StoreSettings.SettingType.IOPriority);

        Assert.Equal("Low", result);
    }

    [Fact]
    public void GetSetting_ReturnsEmptyString_WhenProcessNotFound()
    {
        Sut().UpdateSetting(StoreSettings.SettingType.Boosted, "other", "True");

        var result = Sut().GetSetting("missing", StoreSettings.SettingType.Boosted);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetSetting_ReturnsEmptyString_WhenFileDoesNotExist()
    {
        var result = Sut().GetSetting("missing", StoreSettings.SettingType.Boosted);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetSetting_ReturnsThreadBoostSetting()
    {
        Sut().UpdateSetting(StoreSettings.SettingType.Wdptb, "game", "Disabled");

        var result = Sut().GetSetting("game", StoreSettings.SettingType.Wdptb);

        Assert.Equal(SettingState.Disabled, result);
    }

    // ── DeleteSetting ─────────────────────────────────────────────────────

    [Fact]
    public void DeleteSetting_RemovesOnlyTargetProcess()
    {
        var sut = Sut();
        sut.UpdateSetting(StoreSettings.SettingType.Boosted, "keep",   "True");
        sut.UpdateSetting(StoreSettings.SettingType.Boosted, "remove", "True");

        sut.DeleteSetting("remove");

        var data = Json.JsonManage.ReadJsonFromFile<List<ProcessData>>(_tempFile);
        Assert.Single(data);
        Assert.Equal("keep", data[0].ProcessName);
    }

    [Fact]
    public void DeleteSetting_NoOp_WhenProcessDoesNotExist()
    {
        Sut().UpdateSetting(StoreSettings.SettingType.Boosted, "proc", "True");

        Sut().DeleteSetting("nonexistent");   // must not throw

        var data = Json.JsonManage.ReadJsonFromFile<List<ProcessData>>(_tempFile);
        Assert.Single(data);
    }
}
