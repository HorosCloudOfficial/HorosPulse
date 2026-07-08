namespace HorosPulse.Services.DevTempCleanup;

using HorosPulse.Core.Interfaces;

/// <summary>
/// Preset-Modul: bereinigt sichere Dev-Caches (npm, TEMP, NuGet HTTP-Cache).
/// Global-packages werden nicht ohne UI-Extra-Bestätigung gelöscht.
/// </summary>
public sealed class DevTempCleanupOptimizationModule : IOptimizationModule
{
    private static readonly string[] SafePresetEntryIds =
    [
        "windows-temp",
        "localappdata-temp",
        "npm-cache",
        "dotnet-http-cache",
        "cargo-registry-cache",
    ];

    private readonly IDevTempCleanupService _cleanupService;

    public DevTempCleanupOptimizationModule(IDevTempCleanupService cleanupService) =>
        _cleanupService = cleanupService;

    public string ModuleName => "DevTempCleanup";

    public bool CanApply => true;

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        var result = await _cleanupService.CleanupAsync(SafePresetEntryIds, allowGlobalPackages: false, cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
