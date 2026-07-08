namespace HorosPulse.Tests.Unit;

using FluentAssertions;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Services.DevTempCleanup;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

public class DevTempCleanupPathCatalogTests
{
    [Theory]
    [InlineData(@"%TEMP%", true)]
    [InlineData(@"%LOCALAPPDATA%\npm-cache", true)]
    [InlineData(@"%USERPROFILE%\.cargo\registry\cache", true)]
    [InlineData(@"%USERPROFILE%", false)]
    [InlineData(@"C:\Users\test\node_modules", false)]
    [InlineData(@"%USERPROFILE%\.nuget\packages", false)]
    public void IsPathSafeForDeletion_RespectsAllowlist(string pathTemplate, bool expectedSafe)
    {
        var path = DevTempCleanupPathCatalog.ExpandPath(pathTemplate);
        DevTempCleanupPathCatalog.IsPathSafeForDeletion(path, pathTemplate).Should().Be(expectedSafe);
    }

    [Fact]
    public void Definitions_ContainsExpectedCategories()
    {
        var ids = DevTempCleanupPathCatalog.Definitions.Select(d => d.Id).ToList();

        ids.Should().Contain("windows-temp");
        ids.Should().Contain("npm-cache");
        ids.Should().Contain("dotnet-http-cache");
        ids.Should().Contain("nuget-packages-user");
        ids.Should().Contain("dotnet-global-packages");
    }

    [Fact]
    public void NuGetPackagesUser_IsInfoOnly()
    {
        var entry = DevTempCleanupPathCatalog.Definitions.Single(d => d.Id == "nuget-packages-user");
        entry.Safety.Should().Be(DevTempCacheSafety.InfoOnly);
        entry.CleanupMethod.Should().Be(DevTempCacheCleanupMethod.None);
    }

    [Fact]
    public void GlobalPackages_RequiresExtraConfirmation()
    {
        var entry = DevTempCleanupPathCatalog.Definitions.Single(d => d.Id == "dotnet-global-packages");
        entry.Safety.Should().Be(DevTempCacheSafety.RequiresExtraConfirmation);
    }
}

public class DevTempCacheEntryTests
{
    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(1536, "2 KB")]
    [InlineData(5 * 1024 * 1024, "5.0 MB")]
    [InlineData(3L * 1024 * 1024 * 1024, "3.00 GB")]
    public void FormatBytes_FormatsCorrectly(long bytes, string expected)
    {
        DevTempCacheEntry.FormatBytes(bytes).Should().Be(expected);
    }

    [Fact]
    public void IsDeletable_OnlyForSafeEntries()
    {
        var safe = new DevTempCacheEntry
        {
            Id = "test",
            DisplayName = "Test",
            Path = @"C:\temp",
            Safety = DevTempCacheSafety.SafeDeletable,
            CleanupMethod = DevTempCacheCleanupMethod.DirectoryContents,
            SafetyReason = "ok",
        };

        var info = new DevTempCacheEntry
        {
            Id = "test",
            DisplayName = "Test",
            Path = @"C:\temp",
            Safety = DevTempCacheSafety.InfoOnly,
            CleanupMethod = DevTempCacheCleanupMethod.None,
            SafetyReason = "info",
        };

        safe.IsDeletable.Should().BeTrue();
        info.IsDeletable.Should().BeFalse();
    }
}

public class DevTempCleanupServiceTests
{
    [Fact]
    public async Task ScanAsync_ReturnsAllCatalogEntries()
    {
        var sizeCalc = new Mock<IDirectorySizeCalculator>();
        sizeCalc.Setup(s => s.PathExists(It.IsAny<string>())).Returns(false);
        sizeCalc.Setup(s => s.GetDirectorySizeBytes(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(0);

        var service = new DevTempCleanupService(sizeCalc.Object, NullLogger<DevTempCleanupService>.Instance);
        var result = await service.ScanAsync();

        result.Entries.Should().HaveCount(DevTempCleanupPathCatalog.Definitions.Count);
        result.Entries.Should().Contain(e => e.Id == "npm-cache");
    }

    [Fact]
    public async Task ScanAsync_UsesSizeCalculator()
    {
        var npmPath = DevTempCleanupPathCatalog.ExpandPath(@"%LOCALAPPDATA%\npm-cache");

        var sizeCalc = new Mock<IDirectorySizeCalculator>();
        sizeCalc.Setup(s => s.PathExists(npmPath)).Returns(true);
        sizeCalc.Setup(s => s.GetDirectorySizeBytes(npmPath, It.IsAny<CancellationToken>())).Returns(42_000_000);

        var service = new DevTempCleanupService(sizeCalc.Object, NullLogger<DevTempCleanupService>.Instance);
        var result = await service.ScanAsync();

        var npm = result.Entries.Single(e => e.Id == "npm-cache");
        npm.SizeBytes.Should().Be(42_000_000);
        npm.IsDeletable.Should().BeTrue();
    }

    [Fact]
    public async Task CleanupAsync_RejectsInfoOnlyEntries()
    {
        var sizeCalc = new Mock<IDirectorySizeCalculator>();
        sizeCalc.Setup(s => s.PathExists(It.IsAny<string>())).Returns(false);

        var service = new DevTempCleanupService(sizeCalc.Object, NullLogger<DevTempCleanupService>.Instance);
        var result = await service.CleanupAsync(["nuget-packages-user"]);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Information");
    }

    [Fact]
    public async Task CleanupAsync_RejectsGlobalPackagesWithoutExtraFlag()
    {
        var sizeCalc = new Mock<IDirectorySizeCalculator>();
        sizeCalc.Setup(s => s.PathExists(It.IsAny<string>())).Returns(false);

        var service = new DevTempCleanupService(sizeCalc.Object, NullLogger<DevTempCleanupService>.Instance);
        var result = await service.CleanupAsync(["dotnet-global-packages"], allowGlobalPackages: false);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Extra-Bestätigung");
    }

    [Fact]
    public void DeleteDirectoryContents_RemovesFilesInTempFolder()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "horospulse-cleanup-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testDir);
        var file = Path.Combine(testDir, "artifact.tmp");
        File.WriteAllText(file, "test");

        try
        {
            DevTempCleanupPathCatalog.IsPathSafeForDeletion(testDir, testDir).Should().BeTrue();
            var count = DevTempCleanupService.DeleteDirectoryContents(testDir);
            count.Should().BeGreaterThan(0);
            Directory.Exists(testDir).Should().BeTrue();
            Directory.GetFiles(testDir).Should().BeEmpty();
        }
        finally
        {
            if (Directory.Exists(testDir))
                Directory.Delete(testDir, recursive: true);
        }
    }

    [Fact]
    public void ExpandPath_NormalizesEnvironmentVariables()
    {
        var expanded = DevTempCleanupPathCatalog.ExpandPath(@"%TEMP%");
        expanded.Should().NotContain("%");
        Path.IsPathRooted(expanded).Should().BeTrue();
    }
}

public class DevTempCleanupOptimizationModuleTests
{
    [Fact]
    public void ModuleName_IsDevTempCleanup()
    {
        var service = new Mock<IDevTempCleanupService>();
        var module = new DevTempCleanupOptimizationModule(service.Object);
        module.ModuleName.Should().Be("DevTempCleanup");
        module.CanApply.Should().BeTrue();
    }
}
