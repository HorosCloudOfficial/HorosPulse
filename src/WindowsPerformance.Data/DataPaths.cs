namespace WindowsPerformance.Data;

public static class DataPaths
{
    public static string AppDataRoot =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WindowsPerformance");

    public static string DatabasePath => Path.Combine(AppDataRoot, "data.db");

    public static string ProfilesDirectory => Path.Combine(AppDataRoot, "profiles");

    public static string SettingsPath => Path.Combine(AppDataRoot, "settings.json");

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(AppDataRoot);
        Directory.CreateDirectory(ProfilesDirectory);
    }
}
