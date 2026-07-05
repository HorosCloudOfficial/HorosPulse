namespace WindowsPerformance.Services.Rollback;

using Microsoft.Extensions.Logging;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;
using WindowsPerformance.Services.Snapshots;

public sealed class RollbackEngine : IRollbackEngine
{
    private readonly ISnapshotManager _snapshotManager;
    private readonly IReadOnlyDictionary<string, IOptimizationModule> _modules;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<RollbackEngine> _logger;

    public RollbackEngine(
        ISnapshotManager snapshotManager,
        IEnumerable<IOptimizationModule> modules,
        IAuditLogger auditLogger,
        ILogger<RollbackEngine> logger)
    {
        _snapshotManager = snapshotManager;
        _modules = modules.ToDictionary(m => m.ModuleName, StringComparer.OrdinalIgnoreCase);
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async Task<OptimizationResult> RollbackSnapshotAsync(
        SnapshotEntry snapshot,
        CancellationToken cancellationToken = default)
    {
        if (!snapshot.CanRollback || !snapshot.IsValid)
            return OptimizationResult.Fail("Snapshot ist ungültig oder kann nicht zurückgesetzt werden.");

        var changes = new List<string>();
        var errors = new List<string>();

        if (string.Equals(snapshot.Module, "FullSystem", StringComparison.OrdinalIgnoreCase))
        {
            var moduleOrder = new[]
            {
                "Cursor",
                "ProcessPriority",
                "IndexerExclusion",
                "DefenderExclusion",
                "PowerPlan",
            };

            foreach (var moduleName in moduleOrder)
            {
                if (!_modules.TryGetValue(moduleName, out var module))
                    continue;

                try
                {
                    await module.RollbackAsync(cancellationToken);
                    changes.Add($"{moduleName}: zurückgesetzt");
                    await _auditLogger.LogRollbackAsync(moduleName, true, $"Snapshot {snapshot.Label}", cancellationToken);
                }
                catch (Exception ex)
                {
                    errors.Add($"{moduleName}: {ex.Message}");
                    await _auditLogger.LogRollbackAsync(moduleName, false, ex.Message, cancellationToken);
                    _logger.LogWarning(ex, "Rollback für Modul {Module} fehlgeschlagen", moduleName);
                }
            }
        }
        else if (_modules.TryGetValue(snapshot.Module, out var singleModule))
        {
            try
            {
                await singleModule.RollbackAsync(cancellationToken);
                changes.Add($"{snapshot.Module}: zurückgesetzt");
                await _auditLogger.LogRollbackAsync(snapshot.Module, true, $"Snapshot {snapshot.Label}", cancellationToken);
            }
            catch (Exception ex)
            {
                errors.Add($"{snapshot.Module}: {ex.Message}");
                await _auditLogger.LogRollbackAsync(snapshot.Module, false, ex.Message, cancellationToken);
            }
        }
        else
        {
            return OptimizationResult.Fail($"Kein Modul für Snapshot '{snapshot.Module}' gefunden.");
        }

        if (errors.Count > 0 && changes.Count == 0)
            return OptimizationResult.Fail(string.Join("; ", errors));

        if (errors.Count > 0)
            return new OptimizationResult(true, string.Join("; ", errors), changes);

        return OptimizationResult.Ok(changes.ToArray());
    }

    public async Task<OptimizationResult> RollbackModuleAsync(
        string moduleName,
        CancellationToken cancellationToken = default)
    {
        if (!_modules.TryGetValue(moduleName, out var module))
            return OptimizationResult.Fail($"Modul '{moduleName}' nicht gefunden.");

        try
        {
            await module.RollbackAsync(cancellationToken);
            await _auditLogger.LogRollbackAsync(moduleName, true, "Manueller Modul-Rollback", cancellationToken);
            return OptimizationResult.Ok($"{moduleName} zurückgesetzt");
        }
        catch (Exception ex)
        {
            await _auditLogger.LogRollbackAsync(moduleName, false, ex.Message, cancellationToken);
            return OptimizationResult.Fail(ex.Message);
        }
    }

    public async Task<OptimizationResult> RollbackLatestAsync(CancellationToken cancellationToken = default)
    {
        var snapshots = await _snapshotManager.GetSnapshotsAsync(cancellationToken);
        var latest = snapshots.FirstOrDefault();
        if (latest is null)
            return OptimizationResult.Fail("Kein Snapshot zum Zurücksetzen vorhanden.");

        return await RollbackSnapshotAsync(latest, cancellationToken);
    }
}
