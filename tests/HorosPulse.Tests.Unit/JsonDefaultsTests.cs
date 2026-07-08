namespace HorosPulse.Tests.Unit;

using System.Text.Json;
using FluentAssertions;
using HorosPulse.Core.Models;
using HorosPulse.Data;
using Xunit;

public class JsonDefaultsTests
{
    [Fact]
    public void Serialize_AppSettings_WithNaNWindowPosition_DoesNotThrow()
    {
        var settings = new AppSettings
        {
            WindowLeft = double.NaN,
            WindowTop = double.NaN,
        };

        var act = () => JsonSerializer.Serialize(settings, JsonDefaults.Options);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Serialize_AndDeserialize_NonFiniteDoubles_PreservesNaNSentinel(double value)
    {
        var settings = new AppSettings
        {
            WindowLeft = value,
            WindowTop = value,
        };

        var json = JsonSerializer.Serialize(settings, JsonDefaults.Options);

        json.Should().NotContain("Infinity");
        json.Should().NotContain("NaN");

        var restored = JsonSerializer.Deserialize<AppSettings>(json, JsonDefaults.Options);
        restored.Should().NotBeNull();
        double.IsNaN(restored!.WindowLeft).Should().BeTrue();
        double.IsNaN(restored.WindowTop).Should().BeTrue();
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void SanitizeMetric_ReplacesNonFiniteWithZero(double value)
    {
        JsonDefaults.SanitizeMetric(value).Should().Be(0);
    }

    [Fact]
    public void Serialize_AppSettings_WithCompactWindow_PreservesNestedSettings()
    {
        var settings = new AppSettings
        {
            CompactWindow = new CompactWindowSettings
            {
                ShowRamStats = false,
                ShowCpuStats = true,
                ShowDiskStats = false,
                ShowMemoryCleanAction = true,
                ShowCursorDevModeAction = false,
                ShowDiskOptimizeAction = true,
                ShowVisualEffectsAction = true,
                OpenOnStartup = true,
                WindowWidth = 400,
                WindowHeight = 500,
                WindowLeft = 120,
                WindowTop = 80,
            },
        };

        var json = JsonSerializer.Serialize(settings, JsonDefaults.Options);
        json.Should().Contain("compactWindow");
        json.Should().Contain("openOnStartup");

        var restored = JsonSerializer.Deserialize<AppSettings>(json, JsonDefaults.Options);
        restored.Should().NotBeNull();
        restored!.CompactWindow.ShowRamStats.Should().BeFalse();
        restored.CompactWindow.OpenOnStartup.Should().BeTrue();
        restored.CompactWindow.WindowWidth.Should().Be(400);
        restored.CompactWindow.WindowLeft.Should().Be(120);
    }

    [Fact]
    public void Serialize_CompactWindowSettings_WithNaNPosition_DoesNotThrow()
    {
        var settings = new CompactWindowSettings
        {
            WindowLeft = double.NaN,
            WindowTop = double.NaN,
        };

        var act = () => JsonSerializer.Serialize(settings, JsonDefaults.Options);

        act.Should().NotThrow();
    }
}
