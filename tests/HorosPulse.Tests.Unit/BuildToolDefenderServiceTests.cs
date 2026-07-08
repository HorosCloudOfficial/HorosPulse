namespace HorosPulse.Tests.Unit;

using FluentAssertions;
using HorosPulse.Core.Scripts;
using HorosPulse.Services.BuildToolDefender;
using Xunit;

public class BuildToolDefenderServiceTests
{
    [Fact]
    public void GetDefaultProcesses_IncludesDotNetAndNodeToolchain()
    {
        var service = new BuildToolDefenderService(null!, null!);
        var processes = service.GetDefaultProcesses();

        processes.Select(p => p.ProcessName).Should().Contain("dotnet.exe");
        processes.Select(p => p.ProcessName).Should().Contain("MSBuild.exe");
        processes.Select(p => p.ProcessName).Should().Contain("node.exe");
        processes.Select(p => p.ProcessName).Should().Contain("Cursor.exe");
        processes.Should().HaveCountGreaterThanOrEqualTo(10);
    }

    [Theory]
    [InlineData("[\"dotnet.exe\",\"node.exe\"]", 2)]
    [InlineData("\"dotnet.exe\"", 1)]
    [InlineData("null", 0)]
    public void ParseJsonStringArray_ParsesDefenderOutput(string json, int expectedCount)
    {
        var parsed = BuildToolDefenderService.ParseJsonStringArray(json);

        parsed.Should().HaveCount(expectedCount);
    }

    [Fact]
    public void ScriptHashValidator_AcceptsBuildToolProcessScripts()
    {
        var script = PowerShellScriptLibrary.AddDefenderProcessExclusion("dotnet.exe");
        var hash = ScriptHashValidator.ComputeHash(script);

        ScriptHashValidator.IsAllowed(script, hash).Should().BeTrue();
    }

    [Fact]
    public void AddDefenderProcessExclusion_RejectsPathLikeProcessName()
    {
        var act = () => PowerShellScriptLibrary.AddDefenderProcessExclusion(@"C:\evil\dotnet.exe");
        act.Should().Throw<ArgumentException>();
    }
}
