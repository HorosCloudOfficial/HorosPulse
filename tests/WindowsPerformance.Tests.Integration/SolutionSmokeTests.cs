namespace WindowsPerformance.Tests.Integration;

using FluentAssertions;
using Xunit;

public class SolutionSmokeTests
{
    [Fact]
    public void Integration_project_loads()
    {
        true.Should().BeTrue();
    }
}
