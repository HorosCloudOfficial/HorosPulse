namespace HorosPulse.Tests.Unit;

using FluentAssertions;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Services.DevDrive;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

public class DevDriveAdvisorServiceTests
{
    [Fact]
    public async Task GetAssessmentAsync_WhenNoDevDrive_ReturnsSetupRecommendation()
    {
        var probe = CreateProbe(
        [
            new DevDriveVolumeInfo
            {
                DriveLetter = "C:",
                FileSystem = "NTFS",
                IsReFs = false,
                IsDevDrive = false,
                FreeBytes = 100_000_000_000,
                TotalBytes = 500_000_000_000,
            },
        ]);

        var service = new DevDriveAdvisorService(probe.Object, NullLogger<DevDriveAdvisorService>.Instance);
        var state = await service.GetAssessmentAsync();

        state.HasDevDrive.Should().BeFalse();
        state.Recommendations.Should().Contain(r => r.Title == "Dev Drive einrichten");
        state.SummaryText.Should().Contain("Kein Dev Drive");
    }

    [Fact]
    public void GetAssessmentAsync_WhenPathOnDevDrive_CountsAsOptimal()
    {
        var devPath = @"D:\Dev\source";
        Directory.CreateDirectory(devPath);

        try
        {
            var probe = CreateProbe(
            [
                new DevDriveVolumeInfo
                {
                    DriveLetter = "D:",
                    FileSystem = "ReFS",
                    IsReFs = true,
                    IsDevDrive = true,
                    FreeBytes = 200_000_000_000,
                    TotalBytes = 400_000_000_000,
                },
            ],
            path => path.StartsWith(@"D:\", StringComparison.OrdinalIgnoreCase)
                ? new DevDriveVolumeInfo
                {
                    DriveLetter = "D:",
                    FileSystem = "ReFS",
                    IsReFs = true,
                    IsDevDrive = true,
                    FreeBytes = 200_000_000_000,
                    TotalBytes = 400_000_000_000,
                }
                : null);

            var service = new DevDriveAdvisorService(probe.Object, NullLogger<DevDriveAdvisorService>.Instance);
            var placement = service.EvaluatePath("Test", devPath, hasDevDrive: true);

            placement.Status.Should().Be(DevPathPlacementStatus.OnDevDrive);
            placement.IsOnDevDrive.Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(devPath))
                Directory.Delete(devPath, recursive: true);
        }
    }

    [Fact]
    public void EvaluatePath_OnNtfsWithoutDevDrive_ReturnsSlowVolume()
    {
        var testPath = Path.Combine(Path.GetTempPath(), "horospulse-devdrive-test", "npm-cache");
        Directory.CreateDirectory(testPath);

        try
        {
            var probe = CreateProbe(
            [
                new DevDriveVolumeInfo
                {
                    DriveLetter = "D:",
                    FileSystem = "ReFS",
                    IsReFs = true,
                    IsDevDrive = true,
                    FreeBytes = 200_000_000_000,
                    TotalBytes = 400_000_000_000,
                },
                new DevDriveVolumeInfo
                {
                    DriveLetter = $"{Path.GetPathRoot(testPath)!.TrimEnd('\\')}:",
                    FileSystem = "NTFS",
                    IsReFs = false,
                    IsDevDrive = false,
                    FreeBytes = 100_000_000_000,
                    TotalBytes = 500_000_000_000,
                },
            ],
            path =>
            {
                var root = Path.GetPathRoot(path)!.TrimEnd('\\') + ":";
                return root.Equals("D:", StringComparison.OrdinalIgnoreCase)
                    ? new DevDriveVolumeInfo
                    {
                        DriveLetter = "D:",
                        FileSystem = "ReFS",
                        IsReFs = true,
                        IsDevDrive = true,
                        FreeBytes = 200_000_000_000,
                        TotalBytes = 400_000_000_000,
                    }
                    : new DevDriveVolumeInfo
                    {
                        DriveLetter = root,
                        FileSystem = "NTFS",
                        IsReFs = false,
                        IsDevDrive = false,
                        FreeBytes = 100_000_000_000,
                        TotalBytes = 500_000_000_000,
                    };
            });

            var service = new DevDriveAdvisorService(probe.Object, NullLogger<DevDriveAdvisorService>.Instance);
            var placement = service.EvaluatePath("npm-Cache", testPath, hasDevDrive: true);

            placement.Status.Should().Be(DevPathPlacementStatus.OnSlowVolume);
        }
        finally
        {
            if (Directory.Exists(testPath))
                Directory.Delete(testPath, recursive: true);
        }
    }

    [Theory]
    [InlineData(0x00002000u, true)]
    [InlineData(0x00000000u, false)]
    [InlineData(0x00002001u, true)]
    public void IsDevDriveFlagSet_DetectsDevVolumeFlag(uint flags, bool expected)
    {
        DevDriveVolumeProbe.IsDevDriveFlagSet(flags).Should().Be(expected);
    }

    [Fact]
    public void ExpandPath_ExpandsEnvironmentVariables()
    {
        var expanded = DevDriveAdvisorService.ExpandPath(@"%USERPROFILE%\source");
        expanded.Should().Contain("source");
        expanded.Should().NotContain("%USERPROFILE%");
    }

    [Fact]
    public void BuildRecommendations_WithSlowPaths_IncludesMigrationHints()
    {
        var placements = new List<DevPathPlacement>
        {
            new()
            {
                DisplayName = "npm-Cache",
                Path = @"C:\Users\test\AppData\Local\npm-cache",
                PathExists = true,
                ResolvedDriveLetter = "C:",
                IsOnDevDrive = false,
                IsOnReFs = false,
                Status = DevPathPlacementStatus.OnSlowVolume,
            },
        };

        var devVolume = new DevDriveVolumeInfo
        {
            DriveLetter = "D:",
            FileSystem = "ReFS",
            IsReFs = true,
            IsDevDrive = true,
            FreeBytes = 100_000_000_000,
            TotalBytes = 200_000_000_000,
        };

        var recommendations = DevDriveAdvisorService.BuildRecommendations(
            hasDevDrive: true,
            [devVolume],
            devVolume,
            placements);

        recommendations.Should().Contain(r => r.Title.Contains("npm-Cache", StringComparison.Ordinal));
        recommendations.Should().Contain(r => r.Description.Contains("NPM_CONFIG_CACHE", StringComparison.Ordinal));
    }

    private static Mock<IDevDriveVolumeProbe> CreateProbe(
        IReadOnlyList<DevDriveVolumeInfo> volumes,
        Func<string, DevDriveVolumeInfo?>? resolver = null)
    {
        var probe = new Mock<IDevDriveVolumeProbe>();
        probe.Setup(p => p.GetVolumes()).Returns(volumes);
        probe.Setup(p => p.GetVolumeForPath(It.IsAny<string>()))
            .Returns((string path) => resolver?.Invoke(path) ?? volumes.FirstOrDefault(v =>
                path.StartsWith(v.DriveLetter, StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(v.DriveLetter + "\\", StringComparison.OrdinalIgnoreCase)));
        return probe;
    }
}
