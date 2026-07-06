namespace HorosPulse.Data;

using Microsoft.Extensions.DependencyInjection;
using HorosPulse.Core.Interfaces;
using HorosPulse.Data.Repositories;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHorosPulseData(this IServiceCollection services)
    {
        services.AddSingleton<DatabaseBootstrap>();
        services.AddSingleton<ISnapshotRepository, SnapshotRepository>();
        services.AddSingleton<IAuditRepository, AuditRepository>();
        services.AddSingleton<IPerformanceMetricRepository, PerformanceMetricRepository>();
        services.AddSingleton<IProfileRepository, ProfileRepository>();
        return services;
    }
}
