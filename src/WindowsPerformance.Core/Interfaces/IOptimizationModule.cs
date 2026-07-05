namespace WindowsPerformance.Core.Interfaces;

public interface IOptimizationModule
{
    string ModuleName { get; }
    bool CanApply { get; }
    Task ApplyAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
