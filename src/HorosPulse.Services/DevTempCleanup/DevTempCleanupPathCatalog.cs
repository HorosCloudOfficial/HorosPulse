namespace HorosPulse.Services.DevTempCleanup;

using HorosPulse.Core.Models;

/// <summary>Statischer Katalog bekannter Dev-Cache-Pfade mit Sicherheitsklassifikation.</summary>
internal static class DevTempCleanupPathCatalog
{
    internal sealed record PathDefinition(
        string Id,
        string DisplayName,
        string PathTemplate,
        DevTempCacheSafety Safety,
        DevTempCacheCleanupMethod CleanupMethod,
        string SafetyReason);

    internal static readonly IReadOnlyList<PathDefinition> Definitions =
    [
        new(
            "windows-temp",
            "Windows TEMP",
            @"%TEMP%",
            DevTempCacheSafety.SafeDeletable,
            DevTempCacheCleanupMethod.DirectoryContents,
            "Benutzer-Temp — Build-Zwischendateien; gesperrte Dateien werden übersprungen."),
        new(
            "localappdata-temp",
            "LocalAppData Temp",
            @"%LOCALAPPDATA%\Temp",
            DevTempCacheSafety.SafeDeletable,
            DevTempCacheCleanupMethod.DirectoryContents,
            "MSBuild/IDE-Zwischendateien; gesperrte Dateien werden übersprungen."),
        new(
            "npm-cache",
            "npm-Cache",
            @"%LOCALAPPDATA%\npm-cache",
            DevTempCacheSafety.SafeDeletable,
            DevTempCacheCleanupMethod.DirectoryContents,
            "npm-Cache kann jederzeit neu aufgebaut werden."),
        new(
            "dotnet-http-cache",
            "NuGet HTTP-Cache",
            "",
            DevTempCacheSafety.SafeDeletable,
            DevTempCacheCleanupMethod.DotNetNugetHttpCache,
            "NuGet-Download-Cache — wird via dotnet nuget locals http-cache --clear geleert."),
        new(
            "pnpm-store",
            "pnpm Store",
            @"%LOCALAPPDATA%\pnpm\store",
            DevTempCacheSafety.SafeDeletable,
            DevTempCacheCleanupMethod.PnpmStorePrune,
            "Unreferenzierte Pakete via pnpm store prune (falls pnpm installiert)."),
        new(
            "nuget-packages-user",
            "NuGet global-packages (Benutzer)",
            @"%USERPROFILE%\.nuget\packages",
            DevTempCacheSafety.InfoOnly,
            DevTempCacheCleanupMethod.None,
            "Enthält installierte NuGet-Pakete — nicht blind löschen. Nur zur Größeninfo."),
        new(
            "dotnet-global-packages",
            "NuGet global-packages (dotnet)",
            "",
            DevTempCacheSafety.RequiresExtraConfirmation,
            DevTempCacheCleanupMethod.DotNetNugetGlobalPackages,
            "Löscht alle globalen NuGet-Pakete — nur mit Extra-Bestätigung und nach Builds prüfen."),
        new(
            "cargo-registry-cache",
            "Cargo Registry-Cache",
            @"%USERPROFILE%\.cargo\registry\cache",
            DevTempCacheSafety.SafeDeletable,
            DevTempCacheCleanupMethod.DirectoryContents,
            "Cargo-Download-Cache — wird bei nächstem Build neu geladen."),
    ];

    internal static string ExpandPath(string pathTemplate)
    {
        if (string.IsNullOrWhiteSpace(pathTemplate))
            return string.Empty;

        var expanded = Environment.ExpandEnvironmentVariables(pathTemplate);
        try
        {
            return Path.GetFullPath(expanded);
        }
        catch
        {
            return expanded;
        }
    }

    internal static bool IsPathSafeForDeletion(string path, string pathTemplate)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var normalized = ExpandPath(path);
        if (string.IsNullOrWhiteSpace(normalized))
            return false;

        // Never delete user profile root
        var userProfile = ExpandPath(@"%USERPROFILE%");
        if (PathsEqual(normalized, userProfile))
            return false;

        // Never delete node_modules anywhere
        if (normalized.Contains($"{Path.DirectorySeparatorChar}node_modules", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains($"{Path.AltDirectorySeparatorChar}node_modules", StringComparison.OrdinalIgnoreCase))
            return false;

        // Must be under an allowed root for directory cleanup
        var allowedRoots = GetAllowedDeletionRoots();
        if (!allowedRoots.Any(root => IsUnderRoot(normalized, root)))
            return false;

        // Block .nuget\packages unless explicitly global-packages cleanup method
        if (normalized.Contains($"{Path.DirectorySeparatorChar}.nuget{Path.DirectorySeparatorChar}packages", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    internal static IReadOnlyList<string> GetAllowedDeletionRoots()
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ExpandPath(@"%TEMP%"),
            ExpandPath(@"%LOCALAPPDATA%\Temp"),
            ExpandPath(@"%LOCALAPPDATA%\npm-cache"),
            ExpandPath(@"%LOCALAPPDATA%\pnpm\store"),
            ExpandPath(@"%USERPROFILE%\.cargo\registry\cache"),
        };

        var tempRoot = Path.GetPathRoot(ExpandPath(@"%TEMP%"));
        if (!string.IsNullOrWhiteSpace(tempRoot))
            roots.Add(tempRoot.TrimEnd('\\'));

        return roots.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
    }

    internal static bool IsUnderRoot(string path, string root)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(root))
            return false;

        var normalizedPath = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedRoot = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase) &&
               (normalizedPath.Length == normalizedRoot.Length ||
                normalizedPath[normalizedRoot.Length] == Path.DirectorySeparatorChar ||
                normalizedPath[normalizedRoot.Length] == Path.AltDirectorySeparatorChar);
    }

    internal static bool PathsEqual(string a, string b) =>
        string.Equals(
            a.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            b.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            StringComparison.OrdinalIgnoreCase);
}
