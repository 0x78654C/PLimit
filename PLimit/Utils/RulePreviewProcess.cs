using System.Diagnostics;

namespace PLimit.Utils;

internal sealed record RulePreviewProcess(string Name, int Pid)
{
    public static IReadOnlyList<RulePreviewProcess> Capture()
    {
        var result = new List<RulePreviewProcess>();
        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                try { result.Add(new(process.ProcessName, process.Id)); }
                catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or NotSupportedException) { }
            }
        }
        return result.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ThenBy(p => p.Pid).ToArray();
    }
}
