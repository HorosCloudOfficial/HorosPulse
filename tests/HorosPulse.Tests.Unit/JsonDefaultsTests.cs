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
}
