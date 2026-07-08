namespace HorosPulse.Core.Interfaces;

/// <summary>Steuert Navigation und Sichtbarkeit zwischen Haupt- und Kompakt-Fenster.</summary>
public interface ICompactWindowCoordinator
{
    bool IsCompactWindowVisible { get; }

    void ShowCompactWindow();

    void HideCompactWindow();

    void ShowMainWindow();

    void ReloadCompactSettings();
}
