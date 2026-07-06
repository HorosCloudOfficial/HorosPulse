namespace HorosPulse.Services.WindowsServices;

using HorosPulse.Core.Interfaces;

public sealed class ServicesOptimizationModule : IOptimizationModule
{
    private readonly IWindowsServiceManager _serviceManager;
    private readonly IServicesOptInGate _optInGate;

    public ServicesOptimizationModule(IWindowsServiceManager serviceManager, IServicesOptInGate optInGate)
    {
        _serviceManager = serviceManager;
        _optInGate = optInGate;
    }

    public string ModuleName => "Services";

    public bool CanApply => _optInGate.IsConfirmed;

    public void SetUserConfirmed(bool confirmed) => _optInGate.IsConfirmed = confirmed;

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        if (!_optInGate.IsConfirmed)
            throw new InvalidOperationException("Dienste-Optimierung erfordert ausdrückliche Zustimmung.");

        var result = await _serviceManager.ApplyDevPresetAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        var result = await _serviceManager.RollbackStartupTypesAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }
}
