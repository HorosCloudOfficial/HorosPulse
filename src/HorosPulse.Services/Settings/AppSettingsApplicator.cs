namespace HorosPulse.Services.Settings;

using HorosPulse.Core.Enums;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Services.PowerShell;

/// <summary>
/// Wendet geladene <see cref="AppSettings"/> auf abhängige Runtime-Optionen an.
/// </summary>
public static class AppSettingsApplicator
{
    public static void Apply(
        AppSettings settings,
        PowerShellOptions powerShellOptions,
        ILoggingLevelService loggingLevelService,
        IThemeService? themeService = null)
    {
        powerShellOptions.DefaultTimeout = TimeSpan.FromSeconds(
            Math.Clamp(settings.PowerShellTimeoutSeconds, 5, 600));

        loggingLevelService.SetMinimumLevel(settings.MinimumLogLevel);

        if (themeService is not null)
        {
            var theme = Enum.TryParse<AppTheme>(settings.Theme, ignoreCase: true, out var parsed)
                ? parsed
                : AppTheme.TokyoNight;
            themeService.ApplyTheme(theme);
        }
    }
}
