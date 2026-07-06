namespace WindowsPerformance.Core.Interfaces;

/// <summary>
/// Plattform-File-Dialoge für Import/Export (WPF-Implementierung in App).
/// </summary>
public interface IFilePickerService
{
    /// <summary>Speicherpfad wählen; null wenn abgebrochen.</summary>
    Task<string?> PickSaveFileAsync(string filter, string defaultFileName);

    /// <summary>Öffnen-Pfad wählen; null wenn abgebrochen.</summary>
    Task<string?> PickOpenFileAsync(string filter);
}
