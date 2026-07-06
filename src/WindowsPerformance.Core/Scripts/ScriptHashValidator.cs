using System.Security.Cryptography;
using System.Text;

namespace WindowsPerformance.Core.Scripts;

public static class ScriptHashValidator
{
    private static readonly HashSet<string> StaticAllowedHashes = new(StringComparer.OrdinalIgnoreCase)
    {
        ComputeHash(PowerShellScriptLibrary.GetDefenderExclusions),
        ComputeHash(PowerShellScriptLibrary.IndexerExclusionRollback),
        ComputeHash(PowerShellScriptLibrary.ElevationTestScript),
        ComputeHash(PowerShellScriptLibrary.RestartSearchService),
    };

    public static string ComputeHash(string script)
    {
        var normalized = script.Replace("\r\n", "\n", StringComparison.Ordinal).Trim();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes);
    }

    public static bool IsAllowed(string script, string providedHash)
    {
        if (string.IsNullOrWhiteSpace(script) || string.IsNullOrWhiteSpace(providedHash))
            return false;

        var actualHash = ComputeHash(script);
        if (!actualHash.Equals(providedHash, StringComparison.OrdinalIgnoreCase))
            return false;

        if (StaticAllowedHashes.Contains(actualHash))
            return true;

        if (PowerShellScriptLibrary.TryMatchAddDefenderExclusion(script, out var addPath) &&
            addPath.Length > 0)
            return true;

        if (PowerShellScriptLibrary.TryMatchRemoveDefenderExclusion(script, out var removePath) &&
            removePath.Length > 0)
            return true;

        if (PowerShellScriptLibrary.TryMatchIndexerExclusion(script, out var indexerPath) &&
            indexerPath.Length > 0)
            return true;

        return false;
    }
}
