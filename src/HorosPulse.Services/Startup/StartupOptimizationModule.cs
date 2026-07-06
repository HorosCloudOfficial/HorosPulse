namespace HorosPulse.Services.Startup;

using HorosPulse.Core.Interfaces;

public sealed class StartupOptimizationModule : IOptimizationModule
{
    private readonly IStartupManagerService _startupManager;

    public StartupOptimizationModule(IStartupManagerService startupManager) =>
        _startupManager = startupManager;

    public string ModuleName => "Startup";

    public bool CanApply => true;

    public Task ApplyAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        var result = await _startupManager.RollbackAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }
}
