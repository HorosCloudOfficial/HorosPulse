namespace WindowsPerformance.Core.Interfaces;

/// <summary>
/// Verwaltet Windows-Autostart-Eintrag für die Anwendung.
/// </summary>
public interface IStartupRegistrationService
{
    /// <summary>Ob die App für Autostart registriert ist.</summary>
    bool IsRegistered { get; }

    /// <summary>App für Autostart beim Windows-Login registrieren.</summary>
    bool Register();

    /// <summary>Autostart-Eintrag entfernen.</summary>
    bool Unregister();
}
