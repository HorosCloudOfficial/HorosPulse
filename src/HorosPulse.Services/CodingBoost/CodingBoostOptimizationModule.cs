namespace HorosPulse.Services.CodingBoost;

using HorosPulse.Core.Interfaces;

public sealed class CodingBoostOptimizationModule : IOptimizationModule
{
    private readonly ICodingBoostService _codingBoostService;

    public CodingBoostOptimizationModule(ICodingBoostService codingBoostService) =>
        _codingBoostService = codingBoostService;

    public string ModuleName => "CodingBoost";

    public bool CanApply => true;

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        var result = await _codingBoostService.ApplyDevSetupAsync(userConfirmed: true, cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        var result = await _codingBoostService.RollbackAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }
}
