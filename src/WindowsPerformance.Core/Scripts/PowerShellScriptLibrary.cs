using System.Text.RegularExpressions;

namespace WindowsPerformance.Core.Scripts;

public static partial class PowerShellScriptLibrary
{
    public const string GetDefenderExclusions = "(Get-MpPreference).ExclusionPath | ConvertTo-Json -Compress";

    public const string IndexerExclusionRollback = """
        $regBase = 'HKLM:\SOFTWARE\Microsoft\Windows Search\Gather\Windows\SystemIndex\ExcludePaths'
        if (Test-Path $regBase) {
          Get-ChildItem $regBase | ForEach-Object {
            Remove-Item $_.PSPath -Recurse -Force -ErrorAction SilentlyContinue
          }
        }
        """;

    public const string ElevationTestScript = "Write-Output '{\"ok\":true}'";

    public const string RestartSearchService = """
        Restart-Service -Name 'WSearch' -Force -ErrorAction Stop
        """;

    public static string AddDefenderExclusion(string path)
    {
        ValidatePathArgument(path);
        var escaped = path.Replace("'", "''", StringComparison.Ordinal);
        return $"Add-MpPreference -ExclusionPath '{escaped}'";
    }

    public static string RemoveDefenderExclusion(string path)
    {
        ValidatePathArgument(path);
        var escaped = path.Replace("'", "''", StringComparison.Ordinal);
        return $"Remove-MpPreference -ExclusionPath '{escaped}'";
    }

    public static string BuildIndexerExclusionScript(string path)
    {
        ValidatePathArgument(path);
        var normalized = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var escaped = normalized.Replace("'", "''", StringComparison.Ordinal);
        return $$"""
            $regBase = 'HKLM:\SOFTWARE\Microsoft\Windows Search\Gather\Windows\SystemIndex\ExcludePaths'
            if (-not (Test-Path $regBase)) { New-Item -Path $regBase -Force | Out-Null }
            $id = [guid]::NewGuid().ToString('B')
            $key = Join-Path $regBase $id
            New-Item -Path $key -Force | Out-Null
            Set-ItemProperty -Path $key -Name 'Path' -Value '{{escaped}}'
            Set-ItemProperty -Path $key -Name 'Type' -Value 0 -Type DWord
            """;
    }

    public static bool TryMatchAddDefenderExclusion(string script, out string path) =>
        TryMatchPathScript(script, AddDefenderExclusionPattern(), out path);

    public static bool TryMatchRemoveDefenderExclusion(string script, out string path) =>
        TryMatchPathScript(script, RemoveDefenderExclusionPattern(), out path);

    public static bool TryMatchIndexerExclusion(string script, out string path) =>
        TryMatchPathScript(script, IndexerExclusionPattern(), out path);

    private static bool TryMatchPathScript(string script, Regex pattern, out string path)
    {
        var match = pattern.Match(script.Trim());
        if (!match.Success)
        {
            path = string.Empty;
            return false;
        }

        path = match.Groups["path"].Value.Replace("''", "'", StringComparison.Ordinal);
        return true;
    }

    private static void ValidatePathArgument(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Pfad darf nicht leer sein.", nameof(path));

        if (path.Contains('\n', StringComparison.Ordinal) || path.Contains('\r', StringComparison.Ordinal))
            throw new ArgumentException("Pfad enthält ungültige Zeichen.", nameof(path));
    }

    [GeneratedRegex(@"^Add-MpPreference -ExclusionPath '(?<path>(?:''|[^'])*)'$", RegexOptions.Singleline)]
    private static partial Regex AddDefenderExclusionPattern();

    [GeneratedRegex(@"^Remove-MpPreference -ExclusionPath '(?<path>(?:''|[^'])*)'$", RegexOptions.Singleline)]
    private static partial Regex RemoveDefenderExclusionPattern();

    [GeneratedRegex(
        @"^\$regBase = 'HKLM:\\SOFTWARE\\Microsoft\\Windows Search\\Gather\\Windows\\SystemIndex\\ExcludePaths'[\s\S]*Set-ItemProperty -Path \$key -Name 'Path' -Value '(?<path>(?:''|[^'])*)'[\s\S]*Set-ItemProperty -Path \$key -Name 'Type' -Value 0 -Type DWord\s*$",
        RegexOptions.Singleline)]
    private static partial Regex IndexerExclusionPattern();
}
