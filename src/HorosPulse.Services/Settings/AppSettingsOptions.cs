namespace HorosPulse.Services.Settings;

using Microsoft.Extensions.Options;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed class AppSettingsOptions : IOptions<AppSettings>
{
    private readonly IAppSettingsService _appSettingsService;

    public AppSettingsOptions(IAppSettingsService appSettingsService) =>
        _appSettingsService = appSettingsService;

    public AppSettings Value => _appSettingsService.Current;
}
