namespace WindowsPerformance.Core.Models;

public sealed class PowerPlanInfo
{
    public Guid Guid { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
