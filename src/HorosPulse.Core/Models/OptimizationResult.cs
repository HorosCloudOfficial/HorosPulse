namespace HorosPulse.Core.Models;

public sealed record OptimizationResult(
    bool Success,
    string? ErrorMessage = null,
    IReadOnlyList<string>? Changes = null)
{
    public static OptimizationResult Ok(params string[] changes) =>
        new(true, Changes: changes);

    public static OptimizationResult Fail(string errorMessage) =>
        new(false, errorMessage);
}
