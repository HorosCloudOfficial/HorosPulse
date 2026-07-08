namespace HorosPulse.Tests.Unit;

using FluentAssertions;
using HorosPulse.Services.ProcessPriority;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class ProcessPriorityServiceTests
{
    [Fact]
    public async Task ApplyCursorPrioritiesAsync_WhenCursorNotRunning_ReturnsSuccessWithSkipMessage()
    {
        if (System.Diagnostics.Process.GetProcessesByName("Cursor").Length > 0)
            return;

        var service = new ProcessPriorityService(NullLogger<ProcessPriorityService>.Instance);

        var result = await service.ApplyCursorPrioritiesAsync();

        result.Success.Should().BeTrue();
        result.Changes.Should().ContainSingle()
            .Which.Should().Contain("Keine Cursor-Prozesse");
    }
}
