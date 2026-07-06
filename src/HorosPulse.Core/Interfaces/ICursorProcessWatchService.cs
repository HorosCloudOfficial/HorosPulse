namespace HorosPulse.Core.Interfaces;

/// <summary>
/// Überwacht cursor.exe-Neustarts und wendet Prioritäten optional erneut an.
/// </summary>
public interface ICursorProcessWatchService : IDisposable
{
    /// <summary>Prozess-Überwachung starten.</summary>
    void Start();

    /// <summary>Prozess-Überwachung stoppen.</summary>
    void Stop();
}
