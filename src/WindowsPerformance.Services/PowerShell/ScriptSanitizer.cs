namespace WindowsPerformance.Services.PowerShell;

using System.Text.RegularExpressions;

public static partial class ScriptSanitizer
{
    private static readonly string[] BlockedPatterns =
    [
        @"rm\s+-rf",
        @"Remove-Item\s+[Cc]:\\Windows",
        @"Format-Volume",
        @"Format-\w+",
        @"Stop-Computer",
        @"Restart-Computer",
        @"Clear-EventLog",
        @"Remove-Item\s+-Recurse\s+-Force\s+[Cc]:\\",
    ];

    public static bool IsScriptSafe(string script)
    {
        if (string.IsNullOrWhiteSpace(script))
            return false;

        foreach (var pattern in BlockedPatterns)
        {
            if (Regex.IsMatch(script, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                return false;
        }

        return true;
    }

    public static string RequireSafeScript(string script)
    {
        if (!IsScriptSafe(script))
            throw new InvalidOperationException("Das PowerShell-Skript enthält verbotene Muster.");

        return script;
    }
}
