namespace WindowsPerformance.Services.Stubs;

using WindowsPerformance.Core.Interfaces;

public sealed class StubOptimizationModule : IOptimizationModule
{
    public StubOptimizationModule(string moduleName) => ModuleName = moduleName;

    public string ModuleName { get; }

    public bool CanApply => false;

    public Task ApplyAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task RollbackAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
