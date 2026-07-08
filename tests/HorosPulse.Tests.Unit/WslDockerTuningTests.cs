namespace HorosPulse.Tests.Unit;

using FluentAssertions;
using HorosPulse.Core.Models;
using HorosPulse.Services.WslDocker;
using Xunit;

public class WslConfigParserTests
{
    [Fact]
    public void Parse_ReadsWsl2SectionCaseInsensitive()
    {
        const string content = """
            # comment
            [wsl2]
            memory=8GB
            processors=6
            localhostForwarding=true
            [experimental]
            autoMemoryReclaim=gradual
            """;

        var document = WslConfigParser.Parse(content);

        document.Wsl2Settings.Should().ContainKey("memory").WhoseValue.Should().Be("8GB");
        document.Wsl2Settings.Should().ContainKey("processors").WhoseValue.Should().Be("6");
        document.Wsl2Settings.Should().ContainKey("localhostForwarding").WhoseValue.Should().Be("true");
        document.OtherSections.Should().ContainSingle(s => s.Name == "experimental");
    }

    [Theory]
    [InlineData("8GB", 8 * 1024)]
    [InlineData("512MB", 512)]
    [InlineData("8589934592", 8192)]
    public void TryParseSizeMb_ParsesCommonFormats(string raw, long expectedMb)
    {
        var settings = new Dictionary<string, string> { ["memory"] = raw };

        WslConfigParser.TryParseSizeMb(settings, "memory", out var mb).Should().BeTrue();
        mb.Should().Be(expectedMb);
    }

    [Fact]
    public void ResolveEffectiveLimits_UsesMicrosoftDefaultsWhenUnset()
    {
        var document = WslConfigParser.Parse(string.Empty);
        var limits = WslConfigParser.ResolveEffectiveLimits(document, systemRamMb: 16 * 1024, logicalProcessors: 8);

        limits.MemoryUsesDefault.Should().BeTrue();
        limits.MemoryMb.Should().Be(8 * 1024);
        limits.ProcessorsUsesDefault.Should().BeTrue();
        limits.Processors.Should().Be(8);
        limits.SwapUsesDefault.Should().BeTrue();
        limits.LocalhostForwarding.Should().BeTrue();
        limits.NestedVirtualization.Should().BeTrue();
    }

    [Fact]
    public void MergeRecommendedSettings_PreservesOtherSections()
    {
        const string existing = """
            [wsl2]
            memory=4GB
            kernel=C:\\temp\\custom

            [experimental]
            sparseVhd=true
            """;

        var recommended = new WslConfigRecommendedLimits
        {
            MemoryMb = 12 * 1024,
            Processors = 6,
            SwapMb = 8 * 1024,
            LocalhostForwarding = true,
            NestedVirtualization = true,
            PageReporting = true,
        };

        var merged = WslConfigParser.MergeRecommendedSettings(existing, recommended);

        merged.Should().Contain("memory=12GB");
        merged.Should().Contain("processors=6");
        merged.Should().Contain("kernel=C:\\\\temp\\\\custom");
        merged.Should().Contain("[experimental]");
        merged.Should().Contain("sparseVhd=true");
    }

    [Fact]
    public void Serialize_RoundTripsManagedKeys()
    {
        var document = WslConfigParser.Parse("[wsl2]\nmemory=4GB\n");
        document.Wsl2Settings["processors"] = "4";
        document.Wsl2Settings["swap"] = "8GB";

        var serialized = WslConfigParser.Serialize(document, includeHorosPulseHeader: false);

        var reparsed = WslConfigParser.Parse(serialized);
        reparsed.Wsl2Settings["memory"].Should().Be("4GB");
        reparsed.Wsl2Settings["processors"].Should().Be("4");
        reparsed.Wsl2Settings["swap"].Should().Be("8GB");
    }
}

public class WslDockerRecommendationEngineTests
{
    [Theory]
    [InlineData(16 * 1024, 8 * 1024)]
    [InlineData(32 * 1024, 17 * 1024)]
    [InlineData(8 * 1024, 3 * 1024)]
    public void ComputeRecommendedMemoryMb_ScalesWithSystemRam(long systemRamMb, long expectedMb)
    {
        WslDockerRecommendationEngine.ComputeRecommendedMemoryMb(systemRamMb).Should().Be(expectedMb);
    }

    [Theory]
    [InlineData(16, 13)]
    [InlineData(8, 6)]
    [InlineData(4, 3)]
    public void ComputeRecommendedProcessors_LeavesReserveCores(int logicalProcessors, int expected)
    {
        WslDockerRecommendationEngine.ComputeRecommendedProcessors(logicalProcessors).Should().Be(expected);
    }

    [Fact]
    public void BuildRecommendations_FlagsSuboptimalMemory()
    {
        var current = new WslConfigLimits
        {
            MemoryMb = 4096,
            MemoryUsesDefault = false,
            Processors = 6,
            ProcessorsUsesDefault = false,
            SwapMb = 8192,
            SwapUsesDefault = false,
            LocalhostForwarding = true,
            NestedVirtualization = true,
        };

        var recommended = WslDockerRecommendationEngine.ComputeRecommendedLimits(16 * 1024, 8);

        var items = WslDockerRecommendationEngine.BuildRecommendations(
            current,
            recommended,
            isWslInstalled: true,
            isWsl2Default: true,
            isDockerLikely: true,
            buildDefenderDockerOk: true,
            buildDefenderWslOk: true);

        items.First(r => r.Key == "memory").IsOptimal.Should().BeFalse();
        items.First(r => r.Key == "processors").IsOptimal.Should().BeTrue();
        WslDockerRecommendationEngine.IsDevSetupOptimal(items).Should().BeFalse();
    }

    [Fact]
    public void BuildSummary_MentionsShutdownWhenNotOptimal()
    {
        var recommended = WslDockerRecommendationEngine.ComputeRecommendedLimits(16 * 1024, 8);
        var summary = WslDockerRecommendationEngine.BuildSummary(
            isWslInstalled: true,
            isOptimal: false,
            hasHorosPulseChanges: false,
            recommended);

        summary.Should().Contain("wsl --shutdown");
        summary.Should().Contain("memory=8GB");
    }
}

public class WslDockerMergeRollbackTests
{
    [Fact]
    public void MergeThenParse_MatchesRecommendedLimits()
    {
        var recommended = new WslConfigRecommendedLimits
        {
            MemoryMb = 8 * 1024,
            Processors = 6,
            SwapMb = 8 * 1024,
            LocalhostForwarding = true,
            NestedVirtualization = true,
            PageReporting = true,
        };

        var merged = WslConfigParser.MergeRecommendedSettings(null, recommended, includeHorosPulseHeader: false);
        var limits = WslConfigParser.ResolveEffectiveLimits(WslConfigParser.Parse(merged), 16 * 1024, 8);

        limits.MemoryMb.Should().Be(8 * 1024);
        limits.Processors.Should().Be(6);
        limits.SwapMb.Should().Be(8 * 1024);
        limits.LocalhostForwarding.Should().BeTrue();
        limits.NestedVirtualization.Should().BeTrue();
        limits.PageReporting.Should().BeTrue();
    }

    [Fact]
    public void TrackingData_SupportsRollbackScenario()
    {
        const string original = "[wsl2]\nmemory=4GB\n";
        var tracking = new WslDockerTrackingData
        {
            PreviousFileExisted = true,
            PreviousWslConfigContent = original,
            BackupFilePath = "C:\\temp\\wslconfig.bak",
            AppliedAt = DateTimeOffset.UtcNow,
        };

        tracking.PreviousWslConfigContent.Should().Be(original);
        tracking.PreviousFileExisted.Should().BeTrue();
    }
}
