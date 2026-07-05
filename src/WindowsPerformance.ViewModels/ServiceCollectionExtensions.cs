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
        services.AddSingleton<PresetsViewModel>();
        services.AddSingleton<EinstellungenViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        return services;
    }
}
