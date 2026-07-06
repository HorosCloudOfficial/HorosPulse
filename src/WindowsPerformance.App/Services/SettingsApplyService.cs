namespace WindowsPerformance.App.Services;

using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Services.PowerShell;
using WindowsPerformance.Services.Settings;

public sealed class SettingsApplyService : ISettingsApplyService
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly PowerShellOptions _powerShellOptions;
    private readonly ILoggingLevelService _loggingLevelService;
    private readonly IThemeService _themeService;

    public SettingsApplyService(
        IAppSettingsService appSettingsService,
        PowerShellOptions powerShellOptions,
        ILoggingLevelService loggingLevelService,
        IThemeService themeService)
    {
        _appSettingsService = appSettingsService;
        _powerShellOptions = powerShellOptions;
        _loggingLevelService = loggingLevelService;
        _themeService = themeService;
    }

    public void ApplyCurrent() =>
        AppSettingsApplicator.Apply(
            _appSettingsService.Current,
            _powerShellOptions,
            _loggingLevelService,
            _themeService);
}
