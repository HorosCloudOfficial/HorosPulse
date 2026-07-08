namespace HorosPulse.Core;

public static class PowerPlanNames
{
    public static bool IsHighPerformance(string? name) =>
        name is not null && (
            name.Contains("High", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Höchst", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Ultimat", StringComparison.OrdinalIgnoreCase));
}
