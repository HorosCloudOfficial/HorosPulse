namespace WindowsPerformance.Services.Network;

using WindowsPerformance.Core.Interfaces;

public sealed class NetworkOptimizationModule : IOptimizationModule
{
    private readonly INetworkOptimizerService _networkOptimizerService;

    public NetworkOptimizationModule(INetworkOptimizerService networkOptimizerService) =>
        _networkOptimizerService = networkOptimizerService;

    public string ModuleName => "Network";

    public bool CanApply => true;

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        var result = await _networkOptimizerService.ApplyOptimizationsAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        var result = await _networkOptimizerService.RollbackAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }
}
