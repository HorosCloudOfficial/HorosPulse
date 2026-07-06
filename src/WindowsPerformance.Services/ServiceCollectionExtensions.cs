namespace WindowsPerformance.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;
using WindowsPerformance.Services.Audit;
using WindowsPerformance.Services.Cursor;
using WindowsPerformance.Services.Defender;
using WindowsPerformance.Services.Elevation;
using WindowsPerformance.Services.Indexer;
using WindowsPerformance.Services.Monitoring;
using WindowsPerformance.Services.PowerPlan;
using WindowsPerformance.Services.PowerShell;
using WindowsPerformance.Services.Presets;
using WindowsPerformance.Services.ProcessInspector;
using WindowsPerformance.Services.ProcessPriority;
using WindowsPerformance.Services.Rollback;
using WindowsPerformance.Services.Settings;
using WindowsPerformance.Services.Snapshots;
using WindowsPerformance.Services.Startup;
using WindowsPerformance.Services.Trends;
using WindowsPerformance.Services.VisualEffects;
using WindowsPerformance.Services.WindowsServices;
using WindowsPerformance.Services.Memory;
using WindowsPerformance.Services.Network;
using WindowsPerformance.Services.Health;
using WindowsPerformance.Services.Disk;
using WindowsPerformance.Services.ScheduledTasks;
using WindowsPerformance.Services.Ml;
using WindowsPerformance.Services.RegistryTuner;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsPerformanceServices(this IServiceCollection services)
    {
        services.AddSingleton<PowerShellOptions>();
        services.AddSingleton<IElevationService, ElevationService>();
        services.AddSingleton<IPowerShellBridge, PowerShellBridge>();

        services.AddSingleton<IAppSettingsService, AppSettingsService>();
        services.AddSingleton<IOptions<AppSettings>, AppSettingsOptions>();
        services.AddSingleton<IStartupRegistrationService, StartupRegistrationService>();
        services.AddSingleton<IPowerPlanService, PowerPlanService>();
        services.AddSingleton<ICursorOptimizer, CursorOptimizerService>();
        services.AddSingleton<IProcessPriorityService, ProcessPriorityService>();
        services.AddSingleton<IDefenderExclusionService, DefenderExclusionService>();
        services.AddSingleton<IIndexerExclusionService, IndexerExclusionService>();

        services.AddSingleton<IWindowsServiceManager, WindowsServiceManager>();
        services.AddSingleton<IServicesOptInGate, ServicesOptInGate>();
        services.AddSingleton<IStartupManagerService, StartupManagerService>();
        services.AddSingleton<IVisualEffectsService, VisualEffectsService>();
        services.AddSingleton<IMemoryOptimizerService, MemoryOptimizerService>();
        services.AddSingleton<INetworkOptimizerService, NetworkOptimizerService>();
        services.AddSingleton<IHealthScorerService, HealthScorerService>();
        services.AddSingleton<ITrendAnalysisService, TrendAnalysisService>();
        services.AddSingleton<IProcessInspectorService, ProcessInspectorService>();
        services.AddSingleton<IDiskOptimizerService, DiskOptimizerService>();
        services.AddSingleton<ITaskSchedulerService, TaskSchedulerService>();
        services.AddSingleton<IMetricsAnomalyService, MetricsAnomalyService>();
        services.AddSingleton<IRecommendationEngine, RecommendationEngine>();
        services.AddSingleton<IRegistryTunerService, RegistryTunerService>();
        services.AddSingleton<ICursorProcessWatchService, CursorProcessWatchService>();

        services.AddSingleton<PowerPlanOptimizationModule>();
        services.AddSingleton<CursorOptimizationModule>();
        services.AddSingleton<ProcessPriorityOptimizationModule>();
        services.AddSingleton<DefenderExclusionOptimizationModule>();
        services.AddSingleton<IndexerExclusionOptimizationModule>();
        services.AddSingleton<ServicesOptimizationModule>();
        services.AddSingleton<StartupOptimizationModule>();
        services.AddSingleton<VisualEffectsOptimizationModule>();
        services.AddSingleton<NetworkOptimizationModule>();
        services.AddSingleton<DiskOptimizerOptimizationModule>();
        services.AddSingleton<TaskSchedulerOptimizationModule>();
        services.AddSingleton<RegistryTunerOptimizationModule>();

        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<PowerPlanOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<CursorOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<ProcessPriorityOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<DefenderExclusionOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<IndexerExclusionOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<ServicesOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<StartupOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<VisualEffectsOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<NetworkOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<DiskOptimizerOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<TaskSchedulerOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<RegistryTunerOptimizationModule>());

        services.AddSingleton<ISnapshotManager, SnapshotManager>();
        services.AddSingleton<IRollbackEngine, RollbackEngine>();
        services.AddSingleton<IAuditLogger, AuditLogger>();
        services.AddSingleton<IMetricsCollector, PerformanceCounterService>();
        services.AddSingleton<IProcessMonitorService, ProcessMonitorService>();
        services.AddSingleton<IPresetOrchestrator, PresetOrchestrator>();

        return services;
    }
}
