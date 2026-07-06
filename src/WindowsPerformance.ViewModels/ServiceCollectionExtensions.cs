namespace WindowsPerformance.ViewModels;

using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsPerformanceViewModels(this IServiceCollection services)
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
        services.AddSingleton<MainWindowViewModel>();
        return services;
    }
}
