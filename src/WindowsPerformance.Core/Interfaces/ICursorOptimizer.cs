namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface ICursorOptimizer
{
    string SettingsPath { get; }
    Task<CursorSettingsProfile> PreviewOptimizationsAsync(CancellationToken cancellationToken = default);
    Task<OptimizationResult> ApplyOptimizationsAsync(CancellationToken cancellationToken = default);
    Task<OptimizationResult> RevertOptimizationsAsync(CancellationToken cancellationToken = default);
    bool HasBackup { get; }
}
