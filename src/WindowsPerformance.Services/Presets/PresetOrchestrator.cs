namespace WindowsPerformance.Services.Presets;

using Microsoft.Extensions.Logging;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;
using WindowsPerformance.Core;

public sealed class PresetOrchestrator : IPresetOrchestrator
{
    private readonly IProfileRepository _profileRepository;
    private readonly ISnapshotManager _snapshotManager;
    private readonly IPowerPlanService _powerPlanService;
    private readonly IProcessPriorityService _processPriorityService;
    private readonly IIndexerExclusionService _indexerExclusionService;
    private readonly IDefenderExclusionService _defenderExclusionService;
    private readonly ICursorOptimizer _cursorOptimizer;
    private readonly IAuditLogger _auditLogger;
    private readonly IAppSettingsService _appSettingsService;
    private readonly ILogger<PresetOrchestrator> _logger;

    public PresetOrchestrator(
        IProfileRepository profileRepository,
        ISnapshotManager snapshotManager,
        IPowerPlanService powerPlanService,
        IProcessPriorityService processPriorityService,
        IIndexerExclusionService indexerExclusionService,
        IDefenderExclusionService defenderExclusionService,
        ICursorOptimizer cursorOptimizer,
        IAuditLogger auditLogger,
        IAppSettingsService appSettingsService,
        ILogger<PresetOrchestrator> logger)
    {
        _profileRepository = profileRepository;
        _snapshotManager = snapshotManager;
        _powerPlanService = powerPlanService;
        _processPriorityService = processPriorityService;
        _indexerExclusionService = indexerExclusionService;
        _defenderExclusionService = defenderExclusionService;
        _cursorOptimizer = cursorOptimizer;
        _auditLogger = auditLogger;
        _appSettingsService = appSettingsService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProfileDefinition>> GetPresetsAsync(CancellationToken cancellationToken = default)
    {
        await _profileRepository.EnsureDefaultPresetsAsync(cancellationToken);
        return await _profileRepository.GetAllAsync(cancellationToken);
    }

    public async Task<PresetApplyResult> ApplyPresetAsync(string presetId, CancellationToken cancellationToken = default)
    {
        await _profileRepository.EnsureDefaultPresetsAsync(cancellationToken);
        var preset = await _profileRepository.GetByIdAsync(presetId, cancellationToken);
        if (preset is null)
            return PresetApplyResult.Fail(presetId, presetId, null, [], $"Preset '{presetId}' nicht gefunden.");

        return presetId switch
        {
            PresetIds.CursorDevMode => await ApplyCursorDevModeAsync(preset, cancellationToken),
            PresetIds.Balanced => await ApplyBalancedAsync(preset, cancellationToken),
            PresetIds.Gaming => await ApplyGamingAsync(preset, cancellationToken),
            _ => PresetApplyResult.Fail(preset.Id, preset.Name, null, [], "Benutzerdefinierte Presets werden in Sprint 4 unterstützt."),
        };
    }

    private async Task<PresetApplyResult> ApplyCursorDevModeAsync(
        ProfileDefinition preset,
        CancellationToken cancellationToken)
    {
        var steps = new List<PresetStepResult>();
        Guid? snapshotId = null;

        try
        {
            var snapshot = await _snapshotManager.CreateBaselineAsync("before_cursor_dev_mode", cancellationToken);
            snapshotId = snapshot.Id;
            steps.Add(new PresetStepResult("Snapshot erstellen", true, snapshot.Label));

            var powerResult = await _powerPlanService.EnsureHighPerformancePlanAsync(cancellationToken);
            steps.Add(StepFromResult("Energieplan: Höchstleistung", powerResult));
            await _auditLogger.LogApplyAsync("PowerPlan", powerResult.Success, null, powerResult.Changes?.FirstOrDefault(), cancellationToken);

            steps.Add(new PresetStepResult(
                "Visuelle Effekte minimieren",
                true,
                "Übersprungen — VisualEffectsService nicht implementiert"));

            if (_appSettingsService.Current.DefenderOptIn)
            {
                var defenderResult = await _defenderExclusionService.ApplyExclusionsAsync(true, cancellationToken);
                steps.Add(StepFromResult("Windows Defender Ausnahmen", defenderResult));
                await _auditLogger.LogApplyAsync("DefenderExclusion", defenderResult.Success, null, null, cancellationToken);
            }
            else
            {
                steps.Add(new PresetStepResult("Windows Defender Ausnahmen", true, "Übersprungen — Opt-in nicht aktiviert"));
            }

            var indexerPaths = await BuildIndexerPathsAsync(cancellationToken);
            var indexerResult = await _indexerExclusionService.ApplyExclusionsAsync(indexerPaths, cancellationToken);
            steps.Add(StepFromResult("Suchindexer-Ausschlüsse", indexerResult));
            await _auditLogger.LogApplyAsync("IndexerExclusion", indexerResult.Success, null, null, cancellationToken);

            var priorityResult = await _processPriorityService.ApplyCursorPrioritiesAsync(cancellationToken);
            steps.Add(StepFromResult("Prozessprioritäten", priorityResult));
            await _auditLogger.LogApplyAsync("ProcessPriority", priorityResult.Success, null, null, cancellationToken);

            var cursorResult = await _cursorOptimizer.ApplyOptimizationsAsync(cancellationToken);
            steps.Add(StepFromResult("Cursor-Einstellungen optimieren", cursorResult));
            await _auditLogger.LogApplyAsync("Cursor", cursorResult.Success, null, null, cancellationToken);

            await _auditLogger.LogAsync("PresetApply", preset.Name, "Cursor Dev Mode angewendet", cancellationToken);
            steps.Add(new PresetStepResult("Audit-Protokoll", true));

            var failed = steps.Any(s => !s.Success);
            if (failed)
            {
                return PresetApplyResult.Fail(
                    preset.Id,
                    preset.Name,
                    snapshotId,
                    steps,
                    "Einige Schritte sind fehlgeschlagen.");
            }

            return PresetApplyResult.Ok(preset.Id, preset.Name, snapshotId, steps);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cursor Dev Mode fehlgeschlagen");
            steps.Add(new PresetStepResult("Fehler", false, ex.Message));
            return PresetApplyResult.Fail(preset.Id, preset.Name, snapshotId, steps, ex.Message);
        }
    }

    private async Task<PresetApplyResult> ApplyBalancedAsync(
        ProfileDefinition preset,
        CancellationToken cancellationToken)
    {
        var steps = new List<PresetStepResult>();
        Guid? snapshotId = null;

        try
        {
            var snapshot = await _snapshotManager.CreateBaselineAsync("before_balanced", cancellationToken);
            snapshotId = snapshot.Id;
            steps.Add(new PresetStepResult("Snapshot erstellen", true));

            var cursorRevert = await _cursorOptimizer.RevertOptimizationsAsync(cancellationToken);
            steps.Add(StepFromResult("Cursor zurücksetzen", cursorRevert));

            var plans = await _powerPlanService.GetAvailablePlansAsync(cancellationToken);
            var balanced = plans.FirstOrDefault(p =>
                p.Name.Contains("Balanced", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains("Ausbalanciert", StringComparison.OrdinalIgnoreCase));

            if (balanced is not null)
            {
                var planResult = await _powerPlanService.SetActivePlanAsync(balanced.Guid, cancellationToken);
                steps.Add(StepFromResult("Energieplan: Ausgewogen", planResult));
            }
            else
            {
                steps.Add(new PresetStepResult("Energieplan: Ausgewogen", false, "Ausgewogener Plan nicht gefunden"));
            }

            await _auditLogger.LogAsync("PresetApply", preset.Name, "Balanced angewendet", cancellationToken);

            var failed = steps.Any(s => !s.Success);
            return failed
                ? PresetApplyResult.Fail(preset.Id, preset.Name, snapshotId, steps, "Einige Schritte fehlgeschlagen")
                : PresetApplyResult.Ok(preset.Id, preset.Name, snapshotId, steps);
        }
        catch (Exception ex)
        {
            return PresetApplyResult.Fail(preset.Id, preset.Name, snapshotId, steps, ex.Message);
        }
    }

    private async Task<PresetApplyResult> ApplyGamingAsync(
        ProfileDefinition preset,
        CancellationToken cancellationToken)
    {
        var steps = new List<PresetStepResult>();
        Guid? snapshotId = null;

        try
        {
            var snapshot = await _snapshotManager.CreateBaselineAsync("before_gaming", cancellationToken);
            snapshotId = snapshot.Id;
            steps.Add(new PresetStepResult("Snapshot erstellen", true));

            var powerResult = await _powerPlanService.EnsureHighPerformancePlanAsync(cancellationToken);
            steps.Add(StepFromResult("Energieplan: Höchstleistung", powerResult));

            await _auditLogger.LogAsync("PresetApply", preset.Name, "Gaming angewendet", cancellationToken);
            return PresetApplyResult.Ok(preset.Id, preset.Name, snapshotId, steps);
        }
        catch (Exception ex)
        {
            return PresetApplyResult.Fail(preset.Id, preset.Name, snapshotId, steps, ex.Message);
        }
    }

    private async Task<IReadOnlyList<string>> BuildIndexerPathsAsync(CancellationToken cancellationToken)
    {
        var available = await _indexerExclusionService.GetAvailableEntriesAsync(cancellationToken);
        var devFolders = _appSettingsService.Current.DefaultDevFolders;

        return available
            .Where(entry =>
            {
                var folderName = Path.GetFileName(
                    entry.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                return entry.IsSelected ||
                    devFolders.Any(f =>
                        folderName.Contains(f, StringComparison.OrdinalIgnoreCase) ||
                        entry.Path.Contains(f, StringComparison.OrdinalIgnoreCase));
            })
            .Select(entry => entry.Path)
            .ToList();
    }

    private static PresetStepResult StepFromResult(string name, OptimizationResult result) =>
        new(name, result.Success, result.Success ? result.Changes?.FirstOrDefault() : result.ErrorMessage);
}
