namespace HorosPulse.Services.Snapshots;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Data;
using HorosPulse.Data;
using HorosPulse.Services.Startup;
using HorosPulse.Services.VisualEffects;
using HorosPulse.Services.Network;
using HorosPulse.Services.WindowsServices;

public sealed class SnapshotManager : ISnapshotManager
{
    private readonly ISnapshotRepository _snapshotRepository;
    private readonly IPowerPlanService _powerPlanService;
    private readonly ICursorOptimizer _cursorOptimizer;
    private readonly IDefenderExclusionService _defenderExclusionService;
    private readonly IIndexerExclusionService _indexerExclusionService;
    private readonly IWindowsServiceManager _windowsServiceManager;
    private readonly IStartupManagerService _startupManagerService;
    private readonly IVisualEffectsService _visualEffectsService;
    private readonly INetworkOptimizerService _networkOptimizerService;
    private readonly IAppSettingsService _appSettingsService;
    private readonly ILogger<SnapshotManager> _logger;

    public SnapshotManager(
        ISnapshotRepository snapshotRepository,
        IPowerPlanService powerPlanService,
        ICursorOptimizer cursorOptimizer,
        IDefenderExclusionService defenderExclusionService,
        IIndexerExclusionService indexerExclusionService,
        IWindowsServiceManager windowsServiceManager,
        IStartupManagerService startupManagerService,
        IVisualEffectsService visualEffectsService,
        INetworkOptimizerService networkOptimizerService,
        IAppSettingsService appSettingsService,
        ILogger<SnapshotManager> logger)
    {
        _snapshotRepository = snapshotRepository;
        _powerPlanService = powerPlanService;
        _cursorOptimizer = cursorOptimizer;
        _defenderExclusionService = defenderExclusionService;
        _indexerExclusionService = indexerExclusionService;
        _windowsServiceManager = windowsServiceManager;
        _startupManagerService = startupManagerService;
        _visualEffectsService = visualEffectsService;
        _networkOptimizerService = networkOptimizerService;
        _appSettingsService = appSettingsService;
        _logger = logger;
    }

    public async Task<SnapshotEntry> CreateBaselineAsync(string label, CancellationToken cancellationToken = default)
    {
        await _snapshotRepository.InitializeAsync(cancellationToken);

        var state = await CaptureBaselineStateAsync(cancellationToken);
        var json = SnapshotCompression.SerializeState(state);
        var compressed = SnapshotCompression.Compress(json);
        var checksum = SnapshotCompression.ComputeChecksum(compressed);

        var entry = new SnapshotEntry
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            Label = label,
            Module = "FullSystem",
            StateJson = compressed,
            Checksum = checksum,
            IsValid = true,
            CanRollback = true,
        };

        await _snapshotRepository.InsertAsync(entry, cancellationToken);

        var retention = _appSettingsService.Current.SnapshotRetentionLimit;
        await _snapshotRepository.DeleteOldestBeyondLimitAsync(retention, cancellationToken);

        _logger.LogInformation("Baseline-Snapshot erstellt: {Label} ({Id})", label, entry.Id);
        return entry;
    }

    public async Task<IReadOnlyList<SnapshotEntry>> GetSnapshotsAsync(CancellationToken cancellationToken = default)
    {
        await _snapshotRepository.InitializeAsync(cancellationToken);
        return await _snapshotRepository.GetAllAsync(cancellationToken);
    }

    public async Task<SnapshotEntry?> GetSnapshotAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _snapshotRepository.InitializeAsync(cancellationToken);
        return await _snapshotRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task DeleteSnapshotAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _snapshotRepository.InitializeAsync(cancellationToken);
        await _snapshotRepository.DeleteAsync(id, cancellationToken);
    }

    internal async Task<BaselineState> CaptureBaselineStateAsync(CancellationToken cancellationToken)
    {
        var activePlan = await _powerPlanService.GetActivePlanAsync(cancellationToken);
        string? cursorJson = null;

        if (File.Exists(_cursorOptimizer.SettingsPath))
            cursorJson = await File.ReadAllTextAsync(_cursorOptimizer.SettingsPath, cancellationToken);

        var defenderState = await _defenderExclusionService.GetExclusionSetAsync(cancellationToken);
        var indexerState = await _indexerExclusionService.GetAvailableEntriesAsync(cancellationToken);
        var processPriorityJson = ReadProcessPriorityState();
        var services = await _windowsServiceManager.GetServicesAsync(cancellationToken);
        var serviceSnapshot = services.ToDictionary(s => s.Name, s => s.StartupType);
        var startupEntries = await _startupManagerService.GetEntriesAsync(cancellationToken);
        var visualState = await _visualEffectsService.GetCurrentStateAsync(cancellationToken);
        var networkState = await _networkOptimizerService.GetCurrentSettingsAsync(cancellationToken);

        return new BaselineState
        {
            PowerPlanGuid = activePlan?.Guid.ToString(),
            PowerPlanName = activePlan?.Name,
            CursorSettingsJson = cursorJson,
            CursorHasBackup = _cursorOptimizer.HasBackup,
            DefenderState = defenderState,
            IndexerState = indexerState.ToList(),
            ProcessPriorityStateJson = processPriorityJson,
            ServiceStartupTypesJson = JsonSerializer.Serialize(serviceSnapshot, JsonDefaults.Options),
            StartupEntriesJson = JsonSerializer.Serialize(startupEntries, JsonDefaults.Options),
            VisualEffectsStateJson = JsonSerializer.Serialize(visualState, JsonDefaults.Options),
            NetworkSettingsStateJson = JsonSerializer.Serialize(networkState, JsonDefaults.Options),
        };
    }

    internal static BaselineState? DeserializeState(SnapshotEntry snapshot)
    {
        if (!snapshot.IsValid)
            return null;

        try
        {
            var json = SnapshotCompression.Decompress(snapshot.StateJson);
            return JsonSerializer.Deserialize<BaselineState>(json, JsonDefaults.Options);
        }
        catch
        {
            return null;
        }
    }

    private static string? ReadProcessPriorityState()
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HorosPulse", "process-priority-state.json");

        return File.Exists(path) ? File.ReadAllText(path) : null;
    }
}
