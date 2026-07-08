namespace HorosPulse.Services.DevTempCleanup;

using HorosPulse.Core.Interfaces;

public sealed class DirectorySizeCalculator : IDirectorySizeCalculator
{
    public bool PathExists(string path) =>
        !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);

    public long GetDirectorySizeBytes(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!PathExists(path))
            return 0;

        try
        {
            return SumDirectory(new DirectoryInfo(path), cancellationToken);
        }
        catch
        {
            return 0;
        }
    }

    private static long SumDirectory(DirectoryInfo directory, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        long size = 0;

        try
        {
            foreach (var file in directory.EnumerateFiles())
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    size += file.Length;
                }
                catch
                {
                    // Skip locked/inaccessible files
                }
            }

            foreach (var subDir in directory.EnumerateDirectories())
            {
                cancellationToken.ThrowIfCancellationRequested();
                size += SumDirectory(subDir, cancellationToken);
            }
        }
        catch
        {
            // Partial size is acceptable for estimates
        }

        return size;
    }
}
