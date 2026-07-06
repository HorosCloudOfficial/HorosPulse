namespace HorosPulse.Services.ProcessPriority;

using HorosPulse.Core.Interfaces;

public sealed class ProcessPriorityOptimizationModule : IOptimizationModule
{
    private readonly IProcessPriorityService _processPriorityService;

    public ProcessPriorityOptimizationModule(IProcessPriorityService processPriorityService) =>
        _processPriorityService = processPriorityService;

    public string ModuleName => "ProcessPriority";

    public bool CanApply => true;

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        var result = await _processPriorityService.ApplyCursorPrioritiesAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        var result = await _processPriorityService.RollbackCursorPrioritiesAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }
}
