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
            var themeName = settings.UseLightMode && !settings.Theme.Equals(nameof(AppTheme.Light), StringComparison.OrdinalIgnoreCase)
                ? nameof(AppTheme.Light)
                : settings.Theme;

            var theme = Enum.TryParse<AppTheme>(themeName, ignoreCase: true, out var parsed)
                ? parsed
                : AppTheme.TokyoNight;
            themeService.ApplyTheme(theme);
        }
    }
}
