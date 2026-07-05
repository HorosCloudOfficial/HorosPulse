namespace WindowsPerformance.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Services.Audit;
using WindowsPerformance.Services.Cursor;
using WindowsPerformance.Services.Defender;
using WindowsPerformance.Services.Elevation;
using WindowsPerformance.Services.Indexer;
using WindowsPerformance.Services.Monitoring;
using WindowsPerformance.Services.PowerPlan;
using WindowsPerformance.Services.PowerShell;
using WindowsPerformance.Services.Presets;
using WindowsPerformance.Services.ProcessPriority;
using WindowsPerformance.Services.Rollback;
using WindowsPerformance.Services.Settings;
using WindowsPerformance.Services.Snapshots;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsPerformanceServices(this IServiceCollection services)
    {
        services.AddSingleton<PowerShellOptions>();
        services.AddSingleton<IElevationService, ElevationService>();
        services.AddSingleton<IPowerShellBridge, PowerShellBridge>();

        services.AddSingleton<IAppSettingsService, AppSettingsService>();
        services.AddSingleton<IPowerPlanService, PowerPlanService>();
        services.AddSingleton<ICursorOptimizer, CursorOptimizerService>();
        services.AddSingleton<IProcessPriorityService, ProcessPriorityService>();
        services.AddSingleton<IDefenderExclusionService, DefenderExclusionService>();
        services.AddSingleton<IIndexerExclusionService, IndexerExclusionService>();

        services.AddSingleton<PowerPlanOptimizationModule>();
        services.AddSingleton<CursorOptimizationModule>();
        services.AddSingleton<ProcessPriorityOptimizationModule>();
        services.AddSingleton<DefenderExclusionOptimizationModule>();
        services.AddSingleton<IndexerExclusionOptimizationModule>();

        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<PowerPlanOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<CursorOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<ProcessPriorityOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<DefenderExclusionOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<IndexerExclusionOptimizationModule>());

        services.AddSingleton<ISnapshotManager, SnapshotManager>();
        services.AddSingleton<IRollbackEngine, RollbackEngine>();
        services.AddSingleton<IAuditLogger, AuditLogger>();
        services.AddSingleton<IMetricsCollector, PerformanceCounterService>();
        services.AddSingleton<IPresetOrchestrator, PresetOrchestrator>();

        return services;
    }
}
