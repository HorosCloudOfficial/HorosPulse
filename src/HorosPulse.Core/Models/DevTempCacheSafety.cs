namespace HorosPulse.Core.Models;

/// <summary>Sicherheitsklassifikation für Dev-Cache-Einträge.</summary>
public enum DevTempCacheSafety
{
    /// <summary>Sicher löschbar ohne zusätzliche Warnung.</summary>
    SafeDeletable,

    /// <summary>Nur zur Information — kein automatisches Löschen.</summary>
    InfoOnly,

    /// <summary>Löschbar nur nach expliziter Extra-Bestätigung.</summary>
    RequiresExtraConfirmation,
}
