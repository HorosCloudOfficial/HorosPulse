namespace WindowsPerformance.Services.Settings;

using Microsoft.Extensions.Options;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed class AppSettingsOptions : IOptions<AppSettings>
{
    private readonly IAppSettingsService _appSettingsService;

    public AppSettingsOptions(IAppSettingsService appSettingsService) =>
        _appSettingsService = appSettingsService;

    public AppSettings Value => _appSettingsService.Current;
}
