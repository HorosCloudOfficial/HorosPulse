namespace HorosPulse.App.Services;

using System.Windows;
using HorosPulse.Core.Enums;
using HorosPulse.Core.Interfaces;

public sealed class ThemeService : IThemeService
{
    public AppTheme CurrentTheme { get; private set; } = AppTheme.TokyoNight;

    public void ApplyTheme(AppTheme theme)
    {
        CurrentTheme = theme;
        var app = Application.Current;
        if (app is null)
            return;

        var merged = app.Resources.MergedDictionaries;
        for (var i = merged.Count - 1; i >= 0; i--)
        {
            var source = merged[i].Source?.OriginalString ?? string.Empty;
            if (source.Contains("Themes/", StringComparison.OrdinalIgnoreCase))
                merged.RemoveAt(i);
        }

        var themePath = theme switch
        {
            AppTheme.Dark => "Resources/Themes/Dark.xaml",
            AppTheme.Light => "Resources/Themes/TokyoNight.xaml",
            _ => "Resources/Themes/TokyoNight.xaml",
        };

        merged.Insert(0, new ResourceDictionary
        {
            Source = new Uri(themePath, UriKind.Relative),
        });
    }
}
