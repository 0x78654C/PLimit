using PLimit.Json;
using Xunit;

namespace PLimit.Tests;

public class JsonManageTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();

    public void Dispose() => File.Delete(_tempFile);

    // ── CreateJsonFile ────────────────────────────────────────────────────

    [Fact]
    public void CreateJsonFile_WritesValidJson()
    {
        var data = new[] { new ProcessData { ProcessName = "proc1", Boosted = "True" } };

        JsonManage.CreateJsonFile(_tempFile, data);

        var json = File.ReadAllText(_tempFile);
        Assert.Contains("proc1", json);
        Assert.Contains("True", json);
    }

    [Fact]
    public void CreateJsonFile_OverwritesExistingContent()
    {
        JsonManage.CreateJsonFile(_tempFile, new[] { new ProcessData { ProcessName = "old" } });
        JsonManage.CreateJsonFile(_tempFile, new[] { new ProcessData { ProcessName = "new" } });

        var result = JsonManage.ReadJsonFromFile<ProcessData[]>(_tempFile);
        Assert.Single(result);
        Assert.Equal("new", result[0].ProcessName);
    }

    [Fact]
    public void CreateJsonFile_CreatesMissingParentDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string path = Path.Combine(directory, "nested", "settings.json");
        try
        {
            JsonManage.CreateJsonFile(path, new[] { new ProcessData { ProcessName = "new" } });

            Assert.True(File.Exists(path));
            Assert.Equal("new", JsonManage.ReadJsonFromFile<ProcessData[]>(path)[0].ProcessName);
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    // ── ReadJsonFromFile ──────────────────────────────────────────────────

    [Fact]
    public void CreateJsonFile_LockedDestination_PreservesContentAndCleansTemporaryFile()
    {
        const string original = "[{\"ProcessName\":\"keep\"}]";
        File.WriteAllText(_tempFile, original);
        using var lockedFile = new FileStream(_tempFile, FileMode.Open, FileAccess.Read, FileShare.Read);

        Assert.Throws<IOException>(() =>
            JsonManage.CreateJsonFile(_tempFile, new[] { new ProcessData { ProcessName = "replacement" } }));

        Assert.Equal(original, File.ReadAllText(_tempFile));
        Assert.Empty(Directory.GetFiles(Path.GetDirectoryName(_tempFile)!, $".{Path.GetFileName(_tempFile)}.*.tmp"));
    }

    [Fact]
    public void CreateJsonFile_ReplacesDocumentWithoutChangingAnOpenReader()
    {
        const string original = "[{\"ProcessName\":\"keep\"}]";
        File.WriteAllText(_tempFile, original);
        using var stream = new FileStream(_tempFile, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
        using var reader = new StreamReader(stream);

        JsonManage.CreateJsonFile(_tempFile, new[] { new ProcessData { ProcessName = "replacement" } });

        Assert.Equal(original, reader.ReadToEnd());
        Assert.Equal("replacement", Assert.Single(JsonManage.ReadJsonFromFile<ProcessData[]>(_tempFile)).ProcessName);
    }

    [Fact]
    public void UpdateJsonFileParameter_FailedUpdate_PreservesOriginalContent()
    {
        const string original = "[{\"ProcessName\":\"keep\"}]";
        File.WriteAllText(_tempFile, original);

        Assert.Throws<InvalidOperationException>(() =>
            JsonManage.UpdateJsonFileParameter<List<ProcessData>>(_tempFile, data =>
            {
                data.Clear();
                throw new InvalidOperationException("Update failed");
            }));

        Assert.Equal(original, File.ReadAllText(_tempFile));
    }

    [Fact]
    public void ReadJsonFromFile_ReturnsDeserializedObject()
    {
        var original = new[] { new ProcessData { ProcessName = "notepad", Property = "Normal" } };
        JsonManage.CreateJsonFile(_tempFile, original);

        var result = JsonManage.ReadJsonFromFile<ProcessData[]>(_tempFile);

        Assert.Single(result);
        Assert.Equal("notepad", result[0].ProcessName);
        Assert.Equal("Normal", result[0].Property);
    }

    [Fact]
    public void ReadJsonFromFile_ReturnsEmptyArray_WhenFileContainsEmptyArray()
    {
        File.WriteAllText(_tempFile, "[]");

        var result = JsonManage.ReadJsonFromFile<ProcessData[]>(_tempFile);

        Assert.Empty(result);
    }

    // ── UpdateJsonFile ────────────────────────────────────────────────────

    [Fact]
    public void UpdateJsonFile_AppendsItemToExistingFile()
    {
        JsonManage.CreateJsonFile(_tempFile, new[] { new ProcessData { ProcessName = "proc1" } });

        JsonManage.UpdateJsonFile(_tempFile, new ProcessData { ProcessName = "proc2" });

        var result = JsonManage.ReadJsonFromFile<ProcessData[]>(_tempFile);
        Assert.Equal(2, result.Length);
        Assert.Contains(result, p => p.ProcessName == "proc2");
    }

    [Fact]
    public void UpdateJsonFile_CreatesFileWithSingleItem_WhenFileDoesNotExist()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            JsonManage.UpdateJsonFile(path, new ProcessData { ProcessName = "first" });

            var result = JsonManage.ReadJsonFromFile<ProcessData[]>(path);
            Assert.Single(result);
            Assert.Equal("first", result[0].ProcessName);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void UpdateJsonFile_PreservesExistingItems()
    {
        var existing = new[]
        {
            new ProcessData { ProcessName = "a" },
            new ProcessData { ProcessName = "b" },
        };
        JsonManage.CreateJsonFile(_tempFile, existing);

        JsonManage.UpdateJsonFile(_tempFile, new ProcessData { ProcessName = "c" });

        var result = JsonManage.ReadJsonFromFile<ProcessData[]>(_tempFile);
        Assert.Equal(3, result.Length);
    }

    // ── DeleteJsonData ────────────────────────────────────────────────────

    [Fact]
    public void DeleteJsonData_RemovesMatchingEntries()
    {
        var data = new[]
        {
            new ProcessData { ProcessName = "keep" },
            new ProcessData { ProcessName = "remove" },
        };
        JsonManage.CreateJsonFile(_tempFile, data);

        JsonManage.DeleteJsonData<ProcessData>(_tempFile,
            items => items.Where(p => p.ProcessName == "remove"));

        var result = JsonManage.ReadJsonFromFile<ProcessData[]>(_tempFile);
        Assert.Single(result);
        Assert.Equal("keep", result[0].ProcessName);
    }

    [Fact]
    public void DeleteJsonData_DoesNothing_WhenFileDoesNotExist()
    {
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        // Should not throw
        JsonManage.DeleteJsonData<ProcessData>(missing, items => items);
    }

    [Fact]
    public void DeleteJsonData_LeavesEmptyArray_WhenAllItemsRemoved()
    {
        JsonManage.CreateJsonFile(_tempFile, new[] { new ProcessData { ProcessName = "only" } });

        JsonManage.DeleteJsonData<ProcessData>(_tempFile, items => items);

        var result = JsonManage.ReadJsonFromFile<ProcessData[]>(_tempFile);
        Assert.Empty(result);
    }

    // ── UpdateJsonFileParameter ───────────────────────────────────────────

    [Fact]
    public void UpdateJsonFileParameter_CreatesFileAndAppliesAction_WhenFileDoesNotExist()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            JsonManage.UpdateJsonFileParameter<List<ProcessData>>(path, data =>
            {
                data ??= new List<ProcessData>();
                data.Add(new ProcessData { ProcessName = "new" });
            });

            var result = JsonManage.ReadJsonFromFile<List<ProcessData>>(path);
            Assert.Single(result);
            Assert.Equal("new", result[0].ProcessName);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void UpdateJsonFileParameter_MutatesExistingData()
    {
        var initial = new List<ProcessData> { new() { ProcessName = "proc1", Boosted = "False" } };
        JsonManage.CreateJsonFile(_tempFile, initial);

        JsonManage.UpdateJsonFileParameter<List<ProcessData>>(_tempFile, data =>
        {
            var item = data.First(p => p.ProcessName == "proc1");
            item.Boosted = "True";
        });

        var result = JsonManage.ReadJsonFromFile<List<ProcessData>>(_tempFile);
        Assert.Equal("True", result[0].Boosted);
    }

    [Fact]
    public void UpdateJsonFileParameter_ConcurrentUpdates_DoNotLoseData()
    {
        File.WriteAllText(_tempFile, "[]");

        Parallel.For(0, 20, index =>
            JsonManage.UpdateJsonFileParameter<List<ProcessData>>(_tempFile, data =>
                data.Add(new ProcessData { ProcessName = $"proc{index}" })));

        var result = JsonManage.ReadJsonFromFile<List<ProcessData>>(_tempFile);
        Assert.Equal(20, result.Count);
        Assert.Equal(20, result.Select(process => process.ProcessName).Distinct().Count());
    }
}
