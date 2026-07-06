namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

public interface IProcessInspectorService
{
    Task<IReadOnlyList<ProcessInspectorEntry>> GetProcessesAsync(
        string? filter = null,
        CancellationToken cancellationToken = default);

    Task<OptimizationResult> KillProcessAsync(int processId, CancellationToken cancellationToken = default);
}
