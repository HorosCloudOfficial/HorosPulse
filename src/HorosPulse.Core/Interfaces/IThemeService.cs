namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Enums;

/// <summary>
/// Wendet das UI-Theme der WPF-Anwendung an.
/// </summary>
public interface IThemeService
{
    /// <summary>Aktuell aktives Theme.</summary>
    AppTheme CurrentTheme { get; }

    /// <summary>Theme wechseln und ResourceDictionary aktualisieren.</summary>
    void ApplyTheme(AppTheme theme);
}
