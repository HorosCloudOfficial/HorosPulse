namespace HorosPulse.Core.Models;

/// <summary>Wie ein Cache-Eintrag bereinigt wird.</summary>
public enum DevTempCacheCleanupMethod
{
    /// <summary>Keine Bereinigung möglich.</summary>
    None,

    /// <summary>Dateien im Verzeichnis löschen (mit Pfad-Validierung).</summary>
    DirectoryContents,

    /// <summary><c>dotnet nuget locals http-cache --clear</c></summary>
    DotNetNugetHttpCache,

    /// <summary><c>dotnet nuget locals global-packages --clear</c> (Extra-Bestätigung).</summary>
    DotNetNugetGlobalPackages,

    /// <summary><c>pnpm store prune</c> falls pnpm verfügbar.</summary>
    PnpmStorePrune,
}
