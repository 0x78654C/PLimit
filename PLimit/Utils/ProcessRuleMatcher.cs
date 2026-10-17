using System.Text;

namespace PLimit.Utils;

internal static class ProcessRuleMatcher
{
    public static string NormalizePattern(string pattern)
    {
        string normalized = AutomationSettings.NormalizeName(pattern);
        var result = new StringBuilder(normalized.Length);
        foreach (char c in normalized)
            if (c != '*' || result.Length == 0 || result[^1] != '*') result.Append(c);
        return result.ToString();
    }

    public static void Validate(string? pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern) || pattern.Length > 260
            || pattern.Any(c => char.IsControl(c) || "\\/:\"<>|".Contains(c))
            || string.IsNullOrWhiteSpace(NormalizePattern(pattern)))
            throw new InvalidDataException("Enter a process name or wildcard pattern (up to 260 characters), such as chrome, game* or worker?.exe. Paths are not supported.");
    }

    /// <summary>Anchored glob matching: * is zero or more characters; ? is exactly one.</summary>
    public static bool Matches(string pattern, string processName)
    {
        string glob = NormalizePattern(pattern);
        string name = AutomationSettings.NormalizeName(processName);
        if (glob.Length == 0 || name.Length == 0) return false;
        int patternIndex = 0, nameIndex = 0, starIndex = -1, retryIndex = 0;
        while (nameIndex < name.Length)
        {
            if (patternIndex < glob.Length && (glob[patternIndex] == '?'
                || char.ToUpperInvariant(glob[patternIndex]) == char.ToUpperInvariant(name[nameIndex])))
            { patternIndex++; nameIndex++; }
            else if (patternIndex < glob.Length && glob[patternIndex] == '*')
            { starIndex = patternIndex++; retryIndex = nameIndex; }
            else if (starIndex >= 0)
            { patternIndex = starIndex + 1; nameIndex = ++retryIndex; }
            else return false;
        }
        while (patternIndex < glob.Length && glob[patternIndex] == '*') patternIndex++;
        return patternIndex == glob.Length;
    }
}
