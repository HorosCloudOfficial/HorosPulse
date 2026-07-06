namespace HorosPulse.Services.Stubs;

using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed class StubCursorOptimizer : ICursorOptimizer
{
    public string SettingsPath => string.Empty;
    public bool HasBackup => false;

    public Task<CursorSettingsProfile> PreviewOptimizationsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(new CursorSettingsProfile());

    public Task<OptimizationResult> ApplyOptimizationsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(OptimizationResult.Ok());

    public Task<OptimizationResult> RevertOptimizationsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(OptimizationResult.Ok());
}
