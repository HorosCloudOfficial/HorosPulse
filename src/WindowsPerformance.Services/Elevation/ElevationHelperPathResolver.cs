namespace WindowsPerformance.Services.Elevation;

public static class ElevationHelperPathResolver
{
    public const string HelperFileName = "WindowsPerformance.Elevation.exe";

    public static string GetExpectedPath() =>
        Path.Combine(AppContext.BaseDirectory, HelperFileName);

    public static bool IsHelperPresent() =>
        File.Exists(GetExpectedPath());
}
