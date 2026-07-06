namespace HorosPulse.Tests.Unit;

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Data;
using HorosPulse.Services.Rollback;
using HorosPulse.Services.Snapshots;
using Xunit;

public class SnapshotManagerTests
{
    [Fact]
    public async Task CreateBaselineAsync_StoresSnapshotWithValidChecksum()
    {
        var repository = new Mock<ISnapshotRepository>();
        SnapshotEntry? saved = null;
        repository.Setup(r => r.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        repository.Setup(r => r.InsertAsync(It.IsAny<SnapshotEntry>(), It.IsAny<CancellationToken>()))
            .Callback<SnapshotEntry, CancellationToken>((entry, _) => saved = entry)
            .ReturnsAsync((SnapshotEntry entry, CancellationToken _) => entry);
        repository.Setup(r => r.DeleteOldestBeyondLimitAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var powerPlan = new Mock<IPowerPlanService>();
        powerPlan.Setup(p => p.GetActivePlanAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PowerPlanInfo { Guid = Guid.NewGuid(), Name = "Balanced", IsActive = true });

        var cursor = new Mock<ICursorOptimizer>();
        cursor.SetupGet(c => c.SettingsPath).Returns(Path.Combine(Path.GetTempPath(), "missing-settings.json"));
        cursor.SetupGet(c => c.HasBackup).Returns(false);

        var defender = new Mock<IDefenderExclusionService>();
        defender.Setup(d => d.GetExclusionSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DefenderExclusionSet());

        var indexer = new Mock<IIndexerExclusionService>();
        indexer.Setup(i => i.GetAvailableEntriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<IndexerExcludeEntry>());

        var settings = new Mock<IAppSettingsService>();
        settings.SetupGet(s => s.Current).Returns(new AppSettings { SnapshotRetentionLimit = 50 });

        var windowsServices = new Mock<IWindowsServiceManager>();
        windowsServices.Setup(s => s.GetServicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WindowsServiceInfo>());

        var startup = new Mock<IStartupManagerService>();
        startup.Setup(s => s.GetEntriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<StartupEntry>());

        var visualEffects = new Mock<IVisualEffectsService>();
        visualEffects.Setup(v => v.GetCurrentStateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VisualEffectsState());

        var network = new Mock<INetworkOptimizerService>();
        network.Setup(n => n.GetCurrentSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NetworkSettingsState());

        var manager = new SnapshotManager(
            repository.Object,
            powerPlan.Object,
            cursor.Object,
            defender.Object,
            indexer.Object,
            windowsServices.Object,
            startup.Object,
            visualEffects.Object,
            network.Object,
            settings.Object,
            NullLogger<SnapshotManager>.Instance);

        var result = await manager.CreateBaselineAsync("test_baseline");

        saved.Should().NotBeNull();
        saved!.Label.Should().Be("test_baseline");
        saved.Module.Should().Be("FullSystem");
        saved.IsValid.Should().BeTrue();
        SnapshotCompression.ValidateChecksum(saved.StateJson, saved.Checksum).Should().BeTrue();

        var state = SnapshotManager.DeserializeState(saved);
        state.Should().NotBeNull();
        state!.PowerPlanName.Should().Be("Balanced");
    }

    [Fact]
    public void DeserializeState_ReturnsNull_WhenChecksumInvalid()
    {
        var entry = new SnapshotEntry
        {
            StateJson = SnapshotCompression.Compress("{\"powerPlanGuid\":\"x\"}"),
            Checksum = "INVALID",
            IsValid = false,
        };

        SnapshotManager.DeserializeState(entry).Should().BeNull();
    }
}
