namespace WindowsPerformance.Data;

using Microsoft.Extensions.DependencyInjection;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Data.Repositories;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsPerformanceData(this IServiceCollection services)
    {
        services.AddSingleton<DatabaseBootstrap>();
        services.AddSingleton<ISnapshotRepository, SnapshotRepository>();
        services.AddSingleton<IAuditRepository, AuditRepository>();
        services.AddSingleton<IPerformanceMetricRepository, PerformanceMetricRepository>();
        services.AddSingleton<IProfileRepository, ProfileRepository>();
        return services;
    }
}
