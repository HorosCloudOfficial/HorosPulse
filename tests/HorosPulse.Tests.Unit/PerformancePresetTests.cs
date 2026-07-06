namespace HorosPulse.Tests.Unit;

using FluentAssertions;
using HorosPulse.Core.Models;
using Xunit;

public class PerformancePresetTests
{
    [Fact]
    public void PerformancePreset_HasDefaultId()
    {
        var preset = new PerformancePreset { Name = "Test" };
        preset.Id.Should().NotBeNullOrWhiteSpace();
        preset.Name.Should().Be("Test");
    }
}
