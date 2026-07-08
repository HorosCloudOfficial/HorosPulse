namespace HorosPulse.Core.Interfaces;

/// <summary>Schätzt die Größe von Verzeichnissen (mockbar für Tests).</summary>
public interface IDirectorySizeCalculator
{
    bool PathExists(string path);

    long GetDirectorySizeBytes(string path, CancellationToken cancellationToken = default);
}
