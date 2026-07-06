namespace HorosPulse.Services.RegistryTuner;

using HorosPulse.Core.Interfaces;

public sealed class RegistryTunerOptimizationModule : IOptimizationModule
{
    private readonly IRegistryTunerService _registryTunerService;

    public RegistryTunerOptimizationModule(IRegistryTunerService registryTunerService) =>
        _registryTunerService = registryTunerService;

    public string ModuleName => "RegistryTuner";

    public bool CanApply => true;

    public Task ApplyAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Registry-Tweaks erfordern Einzelanwendung mit User-Zustimmung.");

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        var result = await _registryTunerService.RollbackAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }
}
