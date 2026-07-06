namespace HorosPulse.Services.ScheduledTasks;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.TaskScheduler;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed class TaskSchedulerService : ITaskSchedulerService
{
    private static readonly string[] DevProtectionTaskPaths =
    [
        @"\Microsoft\Windows\WindowsUpdate\Scheduled Start",
        @"\Microsoft\Windows\UpdateOrchestrator\Schedule Scan",
        @"\Microsoft\Windows\UpdateOrchestrator\Schedule Maintenance",
    ];

    private readonly ILogger<TaskSchedulerService> _logger;
    private readonly string _statePath;
    private IReadOnlyList<string> _disabledTasks = Array.Empty<string>();

    public TaskSchedulerService(ILogger<TaskSchedulerService> logger)
    {
        _logger = logger;
        _statePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HorosPulse", "task-scheduler-disabled.json");
    }

    public Task<IReadOnlyList<ScheduledTaskInfo>> GetTasksAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var service = new TaskService();
        var tasks = new List<ScheduledTaskInfo>();

        foreach (var task in service.AllTasks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            tasks.Add(new ScheduledTaskInfo
            {
                Name = task.Name,
                Path = task.Path,
                State = task.State.ToString(),
                IsEnabled = task.Enabled,
                Description = task.Definition.RegistrationInfo.Description,
            });
        }

        return System.Threading.Tasks.Task.FromResult<IReadOnlyList<ScheduledTaskInfo>>(tasks.OrderBy(t => t.Path).ToList());
    }

    public Task<OptimizationResult> ApplyDevProtectionPresetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var service = new TaskService();
        var disabled = new List<string>();

        foreach (var taskPath in DevProtectionTaskPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var task = service.GetTask(taskPath);
            if (task is null || !task.Enabled)
                continue;

            task.Enabled = false;
            disabled.Add(task.Path);
            _logger.LogInformation("Task deaktiviert: {Path}", task.Path);
        }

        if (disabled.Count == 0)
            return System.Threading.Tasks.Task.FromResult(OptimizationResult.Fail("Keine störenden Tasks gefunden oder bereits deaktiviert."));

        _disabledTasks = disabled;
        SaveDisabledTasks(disabled);
        return System.Threading.Tasks.Task.FromResult(OptimizationResult.Ok(
            disabled.Select(p => $"Deaktiviert: {p}").ToArray()));
    }

    public Task<OptimizationResult> RollbackAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var toEnable = _disabledTasks.Count > 0 ? _disabledTasks : LoadDisabledTasks();
        if (toEnable.Count == 0)
            return System.Threading.Tasks.Task.FromResult(OptimizationResult.Fail("Keine deaktivierten Tasks zum Wiederherstellen."));

        using var service = new TaskService();
        var enabled = new List<string>();

        foreach (var taskPath in toEnable)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var task = service.GetTask(taskPath);
            if (task is null)
                continue;

            task.Enabled = true;
            enabled.Add(task.Path);
        }

        _disabledTasks = Array.Empty<string>();
        SaveDisabledTasks([]);
        return System.Threading.Tasks.Task.FromResult(OptimizationResult.Ok(
            enabled.Select(p => $"Aktiviert: {p}").ToArray()));
    }

    private IReadOnlyList<string> LoadDisabledTasks()
    {
        if (!File.Exists(_statePath))
            return Array.Empty<string>();

        return JsonSerializer.Deserialize<List<string>>(File.ReadAllText(_statePath)) ?? [];
    }

    private void SaveDisabledTasks(IReadOnlyList<string> paths)
    {
        var dir = Path.GetDirectoryName(_statePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(_statePath, JsonSerializer.Serialize(paths, new JsonSerializerOptions { WriteIndented = true }));
    }
}
