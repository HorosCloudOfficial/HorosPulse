namespace HorosPulse.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Services.Audit;
using HorosPulse.Services.Cursor;
using HorosPulse.Services.Defender;
using HorosPulse.Services.Elevation;
using HorosPulse.Services.Indexer;
using HorosPulse.Services.Monitoring;
using HorosPulse.Services.PowerPlan;
using HorosPulse.Services.PowerShell;
using HorosPulse.Services.Presets;
using HorosPulse.Services.ProcessInspector;
using HorosPulse.Services.ProcessPriority;
using HorosPulse.Services.Rollback;
using HorosPulse.Services.Settings;
using HorosPulse.Services.Snapshots;
using HorosPulse.Services.Startup;
using HorosPulse.Services.Trends;
using HorosPulse.Services.VisualEffects;
using HorosPulse.Services.WindowsServices;
using HorosPulse.Services.Memory;
using HorosPulse.Services.Network;
using HorosPulse.Services.Health;
using HorosPulse.Services.Disk;
using HorosPulse.Services.ScheduledTasks;
using HorosPulse.Services.Ml;
using HorosPulse.Services.RegistryTuner;
using HorosPulse.Services.BuildToolDefender;
using HorosPulse.Services.DevDrive;
using HorosPulse.Services.CodingBoost;
using HorosPulse.Services.WslDocker;
using HorosPulse.Services.DevTempCleanup;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHorosPulseServices(this IServiceCollection services)
    {
        services.AddSingleton<PowerShellOptions>();
        services.AddSingleton<IElevationUiInvoker, SyncElevationUiInvoker>();
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
        services.AddSingleton<IBuildToolDefenderService, BuildToolDefenderService>();
        services.AddSingleton<IDevDriveVolumeProbe, DevDriveVolumeProbe>();
        services.AddSingleton<IDevDriveAdvisorService, DevDriveAdvisorService>();
        services.AddSingleton<ICodingBoostService, CodingBoostService>();
        services.AddSingleton<IWslDockerTuningService, WslDockerTuningService>();
        services.AddSingleton<IDirectorySizeCalculator, DirectorySizeCalculator>();
        services.AddSingleton<IDevTempCleanupService, DevTempCleanupService>();

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
        services.AddSingleton<BuildToolDefenderOptimizationModule>();
        services.AddSingleton<DevDriveAdvisorOptimizationModule>();
        services.AddSingleton<CodingBoostOptimizationModule>();
        services.AddSingleton<WslDockerTuningOptimizationModule>();
        services.AddSingleton<DevTempCleanupOptimizationModule>();

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
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<BuildToolDefenderOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<DevDriveAdvisorOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<CodingBoostOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<WslDockerTuningOptimizationModule>());
        services.AddSingleton<IOptimizationModule>(sp => sp.GetRequiredService<DevTempCleanupOptimizationModule>());

        services.AddSingleton<ISnapshotManager, SnapshotManager>();
        services.AddSingleton<IRollbackEngine, RollbackEngine>();
        services.AddSingleton<IAuditLogger, AuditLogger>();
        services.AddSingleton<IMetricsCollector, PerformanceCounterService>();
        services.AddSingleton<IProcessMonitorService, ProcessMonitorService>();
        services.AddSingleton<IPresetOrchestrator, PresetOrchestrator>();

        return services;
    }
}
