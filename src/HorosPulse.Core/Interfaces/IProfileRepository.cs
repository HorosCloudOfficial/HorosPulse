namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Lädt und speichert Preset-Profile (Built-in + User).
/// </summary>
public interface IProfileRepository
{
    /// <summary>Alle Profile abrufen.</summary>
    Task<IReadOnlyList<ProfileDefinition>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Profil per ID laden.</summary>
    Task<ProfileDefinition?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Profil speichern oder aktualisieren.</summary>
    Task SaveAsync(ProfileDefinition profile, CancellationToken cancellationToken = default);

    /// <summary>Profil löschen.</summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Built-in Presets anlegen falls nicht vorhanden.</summary>
    Task EnsureDefaultPresetsAsync(CancellationToken cancellationToken = default);
}
