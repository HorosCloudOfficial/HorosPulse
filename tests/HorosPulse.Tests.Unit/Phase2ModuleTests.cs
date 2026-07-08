namespace HorosPulse.Tests.Unit;

using FluentAssertions;
using Moq;
using HorosPulse.Core.Enums;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Services.Health;
using HorosPulse.Services.Startup;
using HorosPulse.Services.Trends;
using Xunit;

public class HealthScorerServiceTests
{
    [Fact]
    public async Task CalculateScore_ReturnsWeightedScore()
    {
        var powerPlan = new Mock<IPowerPlanService>();
        powerPlan.Setup(s => s.GetActivePlanAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PowerPlanInfo { Name = "High performance", Guid = Guid.NewGuid(), IsActive = true });

        var processPriority = new Mock<IProcessPriorityService>();
        processPriority.Setup(s => s.GetCursorProcessStatus()).Returns("Cursor (PID 1, High)");

        var indexer = new Mock<IIndexerExclusionService>();
        indexer.Setup(s => s.GetAvailableEntriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<IndexerExcludeEntry>
            {
                new() { Path = "a", IsApplied = true },
                new() { Path = "b", IsApplied = true },
            });

        var defender = new Mock<IDefenderExclusionService>();
        defender.Setup(s => s.GetExclusionSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DefenderExclusionSet());

        var services = new Mock<IWindowsServiceManager>();
        services.Setup(s => s.GetServicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WindowsServiceInfo>
            {
                new() { Name = "SysMain", DisplayName = "SysMain", StartupType = "Manual", Status = "Running" },
            });

        var settings = new Mock<IAppSettingsService>();
        settings.Setup(s => s.Current).Returns(new AppSettings { DefenderOptIn = false });

        var scorer = new HealthScorerService(
            powerPlan.Object,
            processPriority.Object,
            indexer.Object,
            defender.Object,
            services.Object,
            settings.Object);

        var result = await scorer.CalculateScoreAsync();

        result.Score.Should().BeInRange(0, 100);
        result.Factors.Should().HaveCount(5);
        result.Factors.First(f => f.Name == "Energieplan").EarnedPoints.Should().Be(25);
        result.Factors.First(f => f.Name == "Suchindexer-Ausschlüsse").EarnedPoints.Should().Be(20);
    }
}

public class TrendAnalysisServiceTests
{
    [Fact]
    public void AggregateBuckets_AveragesMetricsPerBucket()
    {
        var now = DateTimeOffset.UtcNow;
        var metrics = new List<PerformanceMetric>
        {
            new(now, 10, 1000, 2000, 5),
            new(now.AddMinutes(1), 30, 1500, 2000, 15),
            new(now.AddMinutes(6), 50, 1200, 2000, 25),
        };

        var buckets = TrendAnalysisService.AggregateBuckets(metrics, TimeSpan.FromMinutes(5));

        buckets.Should().HaveCount(2);
        buckets[0].CpuPercent.Should().Be(20);
        buckets[1].CpuPercent.Should().Be(50);
    }

    [Fact]
    public async Task GetAnnotations_FiltersApplyOperations()
    {
        var metricRepo = new Mock<IPerformanceMetricRepository>();
        var auditRepo = new Mock<IAuditRepository>();
        auditRepo.Setup(r => r.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        auditRepo.Setup(r => r.GetSinceAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditEntry>
            {
                new() { Operation = "Apply", Module = "PowerPlan", Details = "High performance" },
                new() { Operation = "Info", Module = "System", Details = "ignored" },
            });

        var service = new TrendAnalysisService(metricRepo.Object, auditRepo.Object);
        var annotations = await service.GetAnnotationsAsync(TrendTimeRange.OneHour);

        annotations.Should().HaveCount(1);
        annotations[0].Module.Should().Be("PowerPlan");
    }
}

public class StartupManagerServiceTests
{
    [Fact]
    public async Task GetEntries_ReadsRegistryRunKeys()
    {
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<StartupManagerService>>();
        var service = new StartupManagerService(logger.Object);
        var entries = await service.GetEntriesAsync();

        entries.Should().NotBeNull();
    }
}
