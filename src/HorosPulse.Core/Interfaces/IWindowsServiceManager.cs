namespace HorosPulse.Core.Interfaces;

using System.ServiceProcess;
using HorosPulse.Core.Models;

public interface IWindowsServiceManager
{
    Task<IReadOnlyList<WindowsServiceInfo>> GetServicesAsync(CancellationToken cancellationToken = default);

    Task<OptimizationResult> SetStartupTypeAsync(
        string name,
        ServiceStartMode mode,
        CancellationToken cancellationToken = default);

    Task<OptimizationResult> ApplyDevPresetAsync(CancellationToken cancellationToken = default);

    Task<OptimizationResult> RollbackStartupTypesAsync(CancellationToken cancellationToken = default);
}
