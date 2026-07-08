namespace HorosPulse.Services.DevDrive;

using HorosPulse.Core.Interfaces;

/// <summary>
/// Preset-Modul für Dev-Drive-Beratung — rein informativ, keine Systemänderungen.
/// </summary>
public sealed class DevDriveAdvisorOptimizationModule : IOptimizationModule
{
    public string ModuleName => "DevDriveAdvisor";

    public bool CanApply => false;

    public Task ApplyAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task RollbackAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
