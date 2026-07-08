namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Berät zu Microsoft Dev Drive (ReFS): Erkennung, Dev-Pfad-Platzierung und Migrations-Hinweise.
/// Keine automatischen Dateiverschiebungen — nur Analyse und Empfehlungen.
/// </summary>
public interface IDevDriveAdvisorService
{
    /// <summary>Vollständige Bewertung inkl. Volumes, Pfade und Empfehlungen.</summary>
    Task<DevDriveAdvisorState> GetAssessmentAsync(CancellationToken cancellationToken = default);
}
