namespace HorosPulse.Services.ScheduledTasks;

using HorosPulse.Core.Interfaces;

public sealed class TaskSchedulerOptimizationModule : IOptimizationModule
{
    private readonly ITaskSchedulerService _taskSchedulerService;

    public TaskSchedulerOptimizationModule(ITaskSchedulerService taskSchedulerService) =>
        _taskSchedulerService = taskSchedulerService;

    public string ModuleName => "TaskScheduler";

    public bool CanApply => true;

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        var result = await _taskSchedulerService.ApplyDevProtectionPresetAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        var result = await _taskSchedulerService.RollbackAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }
}
