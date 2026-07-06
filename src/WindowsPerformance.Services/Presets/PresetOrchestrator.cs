namespace WindowsPerformance.Services.Presets;

using Microsoft.Extensions.Logging;
using WindowsPerformance.Core;
using WindowsPerformance.Core.Enums;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;
using WindowsPerformance.Services.VisualEffects;

public sealed class PresetOrchestrator : IPresetOrchestrator
{
    private readonly IProfileRepository _profileRepository;
    private readonly ISnapshotManager _snapshotManager;
    private readonly IPowerPlanService _powerPlanService;
    private readonly IProcessPriorityService _processPriorityService;
    private readonly IIndexerExclusionService _indexerExclusionService;
    private readonly IDefenderExclusionService _defenderExclusionService;
    private readonly ICursorOptimizer _cursorOptimizer;
    private readonly IVisualEffectsService _visualEffectsService;
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
        IVisualEffectsService visualEffectsService,
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
        _visualEffectsService = visualEffectsService;
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
            _ => await ApplyUserPresetAsync(preset, cancellationToken),
        };
    }

    private async Task<PresetApplyResult> ApplyUserPresetAsync(
        ProfileDefinition preset,
        CancellationToken cancellationToken)
    {
        if (preset.IsBuiltIn)
            return PresetApplyResult.Fail(preset.Id, preset.Name, null, [], "Built-in Preset ohne Handler.");

        if (preset.Steps.Count == 0)
            return PresetApplyResult.Fail(preset.Id, preset.Name, null, [], "Preset enthält keine Schritte.");

        var steps = new List<PresetStepResult>();
        Guid? snapshotId = null;

        try
        {
            foreach (var stepId in preset.Steps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var stepResult = await ExecuteStepAsync(stepId, preset, snapshotId, cancellationToken);
                if (stepResult.SnapshotId.HasValue)
                    snapshotId = stepResult.SnapshotId;

                steps.Add(stepResult.Step);
                if (!stepResult.Step.Success)
                {
                    return PresetApplyResult.Fail(
                        preset.Id,
                        preset.Name,
                        snapshotId,
                        steps,
                        stepResult.Step.Message ?? $"Schritt '{stepId}' fehlgeschlagen.");
                }
            }

            await _auditLogger.LogAsync("PresetApply", preset.Name, $"Benutzer-Preset angewendet ({preset.Steps.Count} Schritte)", cancellationToken);
            return PresetApplyResult.Ok(preset.Id, preset.Name, snapshotId, steps);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Benutzer-Preset {PresetId} fehlgeschlagen", preset.Id);
            steps.Add(new PresetStepResult("Fehler", false, ex.Message));
            return PresetApplyResult.Fail(preset.Id, preset.Name, snapshotId, steps, ex.Message);
        }
    }

    private async Task<(PresetStepResult Step, Guid? SnapshotId)> ExecuteStepAsync(
        string stepId,
        ProfileDefinition preset,
        Guid? currentSnapshotId,
        CancellationToken cancellationToken)
    {
        return stepId switch
        {
            PresetStepIds.Snapshot => await ExecuteSnapshotStepAsync(preset, cancellationToken),
            PresetStepIds.PowerPlanHighPerformance => await ExecuteOptimizationStepAsync(
                "Energieplan: Höchstleistung",
                () => _powerPlanService.EnsureHighPerformancePlanAsync(cancellationToken),
                "PowerPlan",
                cancellationToken),
            PresetStepIds.PowerPlanBalanced => await ExecuteBalancedPowerPlanStepAsync(cancellationToken),
            PresetStepIds.ProcessPriorityCursor => await ExecuteOptimizationStepAsync(
                "Prozessprioritäten (Cursor)",
                () => _processPriorityService.ApplyCursorPrioritiesAsync(cancellationToken),
                "ProcessPriority",
                cancellationToken),
            PresetStepIds.ProcessPriorityNodeNormal => await ExecuteOptimizationStepAsync(
                "node.exe Priorität: Normal",
                () => _processPriorityService.EnsureNodeNormalPriorityAsync(cancellationToken),
                "ProcessPriority",
                cancellationToken),
            PresetStepIds.IndexerExclusions => await ExecuteIndexerStepAsync(cancellationToken),
            PresetStepIds.DefenderExclusions => await ExecuteDefenderStepAsync(cancellationToken),
            PresetStepIds.CursorOptimize => await ExecuteOptimizationStepAsync(
                "Cursor-Einstellungen optimieren",
                () => _cursorOptimizer.ApplyOptimizationsAsync(cancellationToken),
                "Cursor",
                cancellationToken),
            PresetStepIds.CursorRevert => await ExecuteOptimizationStepAsync(
                "Cursor zurücksetzen",
                () => _cursorOptimizer.RevertOptimizationsAsync(cancellationToken),
                "Cursor",
                cancellationToken),
            _ => (new PresetStepResult(stepId, false, $"Unbekannter Schritt: {stepId}"), currentSnapshotId),
        };
    }

    private async Task<(PresetStepResult Step, Guid? SnapshotId)> ExecuteSnapshotStepAsync(
        ProfileDefinition preset,
        CancellationToken cancellationToken)
    {
        var label = $"before_{SanitizeLabel(preset.Name)}";
        var snapshot = await _snapshotManager.CreateBaselineAsync(label, cancellationToken);
        return (new PresetStepResult("Snapshot erstellen", true, snapshot.Label), snapshot.Id);
    }

    private async Task<(PresetStepResult Step, Guid? SnapshotId)> ExecuteBalancedPowerPlanStepAsync(
        CancellationToken cancellationToken)
    {
        var plans = await _powerPlanService.GetAvailablePlansAsync(cancellationToken);
        var balanced = plans.FirstOrDefault(p =>
            p.Name.Contains("Balanced", StringComparison.OrdinalIgnoreCase) ||
            p.Name.Contains("Ausbalanciert", StringComparison.OrdinalIgnoreCase));

        if (balanced is null)
            return (new PresetStepResult("Energieplan: Ausgewogen", false, "Ausgewogener Plan nicht gefunden"), null);

        var result = await _powerPlanService.SetActivePlanAsync(balanced.Guid, cancellationToken);
        await _auditLogger.LogApplyAsync("PowerPlan", result.Success, null, balanced.Name, cancellationToken);
        return (StepFromResult("Energieplan: Ausgewogen", result), null);
    }

    private async Task<(PresetStepResult Step, Guid? SnapshotId)> ExecuteIndexerStepAsync(
        CancellationToken cancellationToken)
    {
        var paths = await BuildIndexerPathsAsync(cancellationToken);
        var result = await _indexerExclusionService.ApplyExclusionsAsync(paths, cancellationToken);
        await _auditLogger.LogApplyAsync("IndexerExclusion", result.Success, null, null, cancellationToken);
        return (StepFromResult("Suchindexer-Ausschlüsse", result), null);
    }

    private async Task<(PresetStepResult Step, Guid? SnapshotId)> ExecuteDefenderStepAsync(
        CancellationToken cancellationToken)
    {
        if (!_appSettingsService.Current.DefenderOptIn)
            return (new PresetStepResult("Windows Defender Ausnahmen", true, "Übersprungen — Opt-in nicht aktiviert"), null);

        var result = await _defenderExclusionService.ApplyExclusionsAsync(true, cancellationToken);
        await _auditLogger.LogApplyAsync("DefenderExclusion", result.Success, null, null, cancellationToken);
        return (StepFromResult("Windows Defender Ausnahmen", result), null);
    }

    private async Task<(PresetStepResult Step, Guid? SnapshotId)> ExecuteOptimizationStepAsync(
        string stepName,
        Func<Task<OptimizationResult>> action,
        string auditModule,
        CancellationToken cancellationToken)
    {
        var result = await action();
        await _auditLogger.LogApplyAsync(auditModule, result.Success, null, result.Changes?.FirstOrDefault(), cancellationToken);
        return (StepFromResult(stepName, result), null);
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

            var visualResult = await _visualEffectsService.ApplyPresetAsync(VisualEffectsPreset.Performance, cancellationToken);
            steps.Add(StepFromResult("Visuelle Effekte minimieren", visualResult));
            await _auditLogger.LogApplyAsync("VisualEffects", visualResult.Success, null, "Performance", cancellationToken);

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

            var nodeResult = await _processPriorityService.EnsureNodeNormalPriorityAsync(cancellationToken);
            steps.Add(StepFromResult("node.exe Priorität: Normal", nodeResult));
            await _auditLogger.LogApplyAsync("ProcessPriority", nodeResult.Success, null, "node.exe → Normal", cancellationToken);

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

    private static string SanitizeLabel(string name)
    {
        var chars = name
            .Trim()
            .ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '_')
            .ToArray();
        return new string(chars).Trim('_');
    }

    private static PresetStepResult StepFromResult(string name, OptimizationResult result) =>
        new(name, result.Success, result.Success ? result.Changes?.FirstOrDefault() : result.ErrorMessage);
}
