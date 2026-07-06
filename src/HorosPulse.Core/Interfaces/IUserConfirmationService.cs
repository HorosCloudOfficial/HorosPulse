namespace HorosPulse.Core.Interfaces;

/// <summary>
/// Zeigt Bestätigungsdialoge in der WPF-UI an.
/// </summary>
public interface IUserConfirmationService
{
    /// <summary>Ja/Nein-Dialog anzeigen; true bei Bestätigung.</summary>
    bool Confirm(string title, string message, bool isWarning = false);
}
