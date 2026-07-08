namespace HorosPulse.Services.Health;

using HorosPulse.Core;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed class HealthScorerService : IHealthScorerService
{
    private readonly IPowerPlanService _powerPlanService;
    private readonly IProcessPriorityService _processPriorityService;
    private readonly IIndexerExclusionService _indexerExclusionService;
    private readonly IDefenderExclusionService _defenderExclusionService;
    private readonly IWindowsServiceManager _windowsServiceManager;
    private readonly IAppSettingsService _appSettingsService;

    public HealthScorerService(
        IPowerPlanService powerPlanService,
        IProcessPriorityService processPriorityService,
        IIndexerExclusionService indexerExclusionService,
        IDefenderExclusionService defenderExclusionService,
        IWindowsServiceManager windowsServiceManager,
        IAppSettingsService appSettingsService)
    {
        _powerPlanService = powerPlanService;
        _processPriorityService = processPriorityService;
        _indexerExclusionService = indexerExclusionService;
        _defenderExclusionService = defenderExclusionService;
        _windowsServiceManager = windowsServiceManager;
        _appSettingsService = appSettingsService;
    }

    public async Task<HealthScoreResult> CalculateScoreAsync(CancellationToken cancellationToken = default)
    {
        var factors = new List<HealthScoreFactor>();
        var total = 0;
        var max = 0;

        var powerFactor = await EvaluatePowerPlanAsync(cancellationToken);
        factors.Add(powerFactor);
        total += powerFactor.EarnedPoints;
        max += powerFactor.MaxPoints;

        var priorityFactor = await EvaluateCursorPriorityAsync(cancellationToken);
        factors.Add(priorityFactor);
        total += priorityFactor.EarnedPoints;
        max += priorityFactor.MaxPoints;

        var indexerFactor = await EvaluateIndexerAsync(cancellationToken);
        factors.Add(indexerFactor);
        total += indexerFactor.EarnedPoints;
        max += indexerFactor.MaxPoints;

        var defenderFactor = await EvaluateDefenderAsync(cancellationToken);
        factors.Add(defenderFactor);
        total += defenderFactor.EarnedPoints;
        max += defenderFactor.MaxPoints;

        var servicesFactor = await EvaluateServicesAsync(cancellationToken);
        factors.Add(servicesFactor);
        total += servicesFactor.EarnedPoints;
        max += servicesFactor.MaxPoints;

        var score = max > 0 ? (int)Math.Round(total * 100.0 / max) : 0;
        return new HealthScoreResult
        {
            Score = score,
            ColorHex = ScoreColor(score),
            Factors = factors,
        };
    }

    private async Task<HealthScoreFactor> EvaluatePowerPlanAsync(CancellationToken cancellationToken)
    {
        const int max = 25;
        var active = await _powerPlanService.GetActivePlanAsync(cancellationToken);
        var isHigh = PowerPlanNames.IsHighPerformance(active?.Name);

        return new HealthScoreFactor
        {
            Name = "Energieplan",
            MaxPoints = max,
            EarnedPoints = isHigh ? max : active is not null ? max / 2 : 0,
            Detail = active?.Name ?? "Kein Plan",
        };
    }

    private async Task<HealthScoreFactor> EvaluateCursorPriorityAsync(CancellationToken cancellationToken)
    {
        const int max = 20;
        cancellationToken.ThrowIfCancellationRequested();
        var statePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HorosPulse", "process-priority-state.json");

        var hasState = File.Exists(statePath);
        var cursorActive = _processPriorityService.GetCursorProcessStatus() is not null;
        var applied = hasState || cursorActive;
        return new HealthScoreFactor
        {
            Name = "Cursor-Priorität",
            MaxPoints = max,
            EarnedPoints = applied ? max : 0,
            Detail = hasState ? "Prioritäten angewendet" : cursorActive ? "Cursor aktiv" : "Nicht angewendet",
        };
    }

    private async Task<HealthScoreFactor> EvaluateIndexerAsync(CancellationToken cancellationToken)
    {
        const int max = 20;
        var entries = await _indexerExclusionService.GetAvailableEntriesAsync(cancellationToken);
        var applied = entries.Count(e => e.IsApplied);
        var earned = applied >= 2 ? max : applied == 1 ? max / 2 : 0;

        return new HealthScoreFactor
        {
            Name = "Suchindexer-Ausschlüsse",
            MaxPoints = max,
            EarnedPoints = earned,
            Detail = $"{applied} Ausschlüsse angewendet",
        };
    }

    private async Task<HealthScoreFactor> EvaluateDefenderAsync(CancellationToken cancellationToken)
    {
        const int max = 15;
        if (!_appSettingsService.Current.DefenderOptIn)
        {
            return new HealthScoreFactor
            {
                Name = "Defender-Ausschlüsse",
                MaxPoints = max,
                EarnedPoints = max,
                Detail = "Opt-in nicht aktiv (neutral)",
            };
        }

        var set = await _defenderExclusionService.GetExclusionSetAsync(cancellationToken);
        var added = set.AddedByApp.Count;
        var earned = added >= 2 ? max : added == 1 ? max / 2 : 0;

        return new HealthScoreFactor
        {
            Name = "Defender-Ausschlüsse",
            MaxPoints = max,
            EarnedPoints = earned,
            Detail = $"{added} vom Tool hinzugefügt",
        };
    }

    private async Task<HealthScoreFactor> EvaluateServicesAsync(CancellationToken cancellationToken)
    {
        const int max = 20;
        var services = await _windowsServiceManager.GetServicesAsync(cancellationToken);
        var sysMain = services.FirstOrDefault(s =>
            s.Name.Equals("SysMain", StringComparison.OrdinalIgnoreCase));
        var wSearch = services.FirstOrDefault(s =>
            s.Name.Equals("WSearch", StringComparison.OrdinalIgnoreCase));

        var optimized = 0;
        if (sysMain?.StartupType is "Manual" or "Disabled")
            optimized++;
        if (wSearch?.StartupType is "Manual" or "Disabled")
            optimized++;

        var earned = optimized switch
        {
            2 => max,
            1 => max / 2,
            _ => 0,
        };

        return new HealthScoreFactor
        {
            Name = "Dienste (SysMain/WSearch)",
            MaxPoints = max,
            EarnedPoints = earned,
            Detail = $"SysMain: {sysMain?.StartupType ?? "—"}, WSearch: {wSearch?.StartupType ?? "—"} (nicht Teil von Cursor Dev Mode)",
        };
    }

    private static string ScoreColor(int score) => score switch
    {
        >= 80 => "#9ECE6A",
        >= 50 => "#E0AF68",
        _ => "#F7768E",
    };
}
