namespace HorosPulse.Services.Defender;

using HorosPulse.Core.Interfaces;

public sealed class DefenderExclusionOptimizationModule : IOptimizationModule
{
    private readonly IDefenderExclusionService _defenderExclusionService;
    private bool _userConfirmed;

    public DefenderExclusionOptimizationModule(IDefenderExclusionService defenderExclusionService) =>
        _defenderExclusionService = defenderExclusionService;

    public string ModuleName => "DefenderExclusions";

    public bool CanApply => _userConfirmed;

    public void SetUserConfirmed(bool confirmed) => _userConfirmed = confirmed;

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        var result = await _defenderExclusionService.ApplyExclusionsAsync(_userConfirmed, cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        var result = await _defenderExclusionService.RollbackExclusionsAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }
}
