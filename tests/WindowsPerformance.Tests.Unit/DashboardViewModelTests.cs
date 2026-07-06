namespace WindowsPerformance.Tests.Unit;

using FluentAssertions;
using Moq;
using WindowsPerformance.Core;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;
using WindowsPerformance.ViewModels;
using Xunit;

public class DashboardViewModelTests
{
    [Fact]
    public void Title_IsDashboard()
    {
        var vm = CreateViewModel();
        vm.Title.Should().Be("Dashboard");
    }

    [Fact]
    public async Task RefreshAsync_PopulatesHealthFactors()
    {
        var vm = CreateViewModel();
        await vm.RefreshCommand.ExecuteAsync(null);
        vm.HealthFactors.Should().NotBeEmpty();
    }

    private static DashboardViewModel CreateViewModel()
    {
        var metrics = new Mock<IMetricsCollector>();
        metrics.Setup(m => m.GetCurrentMetricAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PerformanceMetric(DateTimeOffset.UtcNow, 25, 8000, 16000, 10));

        var presets = new Mock<IPresetOrchestrator>();
        var rollback = new Mock<IRollbackEngine>();
        var snapshots = new Mock<ISnapshotManager>();
        snapshots.Setup(s => s.GetSnapshotsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<SnapshotEntry>());

        var confirmation = new Mock<IUserConfirmationService>();
        var cursor = new Mock<ICursorOptimizer>();
        cursor.SetupGet(c => c.HasBackup).Returns(false);

        var powerPlan = new Mock<IPowerPlanService>();
        powerPlan.Setup(p => p.GetActivePlanAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PowerPlanInfo { Name = "Balanced", Guid = Guid.NewGuid() });

        var health = new Mock<IHealthScorerService>();
        health.Setup(h => h.CalculateScoreAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HealthScoreResult
            {
                Score = 80,
                ColorHex = "#9ECE6A",
                Factors = [new HealthScoreFactor { Name = "Test", EarnedPoints = 10, MaxPoints = 10, Detail = "OK" }],
            });

        var recommendations = new Mock<IRecommendationEngine>();
        recommendations.Setup(r => r.GetRecommendationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PerformanceRecommendation { Title = "Test", Message = "OK" }]);

        var module = new Mock<IOptimizationModule>();
        module.SetupGet(m => m.ModuleName).Returns("Test");
        module.SetupGet(m => m.CanApply).Returns(true);

        return new DashboardViewModel(
            metrics.Object,
            presets.Object,
            rollback.Object,
            snapshots.Object,
            confirmation.Object,
            cursor.Object,
            powerPlan.Object,
            health.Object,
            recommendations.Object,
            [module.Object]);
    }
}
