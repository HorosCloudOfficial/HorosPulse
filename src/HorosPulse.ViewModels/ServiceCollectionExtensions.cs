namespace HorosPulse.ViewModels;

using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHorosPulseViewModels(this IServiceCollection services)
    {
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<EnergieViewModel>();
        services.AddSingleton<CursorViewModel>();
        services.AddSingleton<MonitorViewModel>();
        services.AddSingleton<SnapshotViewModel>();
        services.AddSingleton<PresetsViewModel>();
        services.AddSingleton<EinstellungenViewModel>();
        services.AddSingleton<ServicesViewModel>();
        services.AddSingleton<StartupViewModel>();
        services.AddSingleton<VisualEffectsViewModel>();
        services.AddSingleton<MemoryViewModel>();
        services.AddSingleton<NetworkViewModel>();
        services.AddSingleton<TrendViewModel>();
        services.AddSingleton<ProcessInspectorViewModel>();
        services.AddSingleton<DiskOptimizerViewModel>();
        services.AddSingleton<TaskSchedulerViewModel>();
        services.AddSingleton<RegistryTunerViewModel>();
        services.AddSingleton<BuildToolDefenderViewModel>();
        services.AddSingleton<DevDriveAdvisorViewModel>();
        services.AddSingleton<CodingBoostViewModel>();
        services.AddSingleton<WslDockerTuningViewModel>();
        services.AddSingleton<DevTempCleanupViewModel>();
        services.AddSingleton<CompactWindowViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        return services;
    }
}
