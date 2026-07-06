namespace HorosPulse.Tests.Unit;

using FluentAssertions;
using HorosPulse.Core.Scripts;
using Xunit;

public class ScriptHashValidatorTests
{
    [Fact]
    public void IsAllowed_AcceptsStaticLibraryScripts()
    {
        var script = PowerShellScriptLibrary.GetDefenderExclusions;
        var hash = ScriptHashValidator.ComputeHash(script);

        ScriptHashValidator.IsAllowed(script, hash).Should().BeTrue();
    }

    [Fact]
    public void IsAllowed_RejectsUnknownScript()
    {
        const string script = "Write-Output 'not allowed'";
        var hash = ScriptHashValidator.ComputeHash(script);

        ScriptHashValidator.IsAllowed(script, hash).Should().BeFalse();
    }

    [Fact]
    public void IsAllowed_AcceptsParameterizedDefenderScript()
    {
        var script = PowerShellScriptLibrary.AddDefenderExclusion(@"C:\Test\Cursor");
        var hash = ScriptHashValidator.ComputeHash(script);

        ScriptHashValidator.IsAllowed(script, hash).Should().BeTrue();
    }

    [Fact]
    public void IsAllowed_RejectsHashMismatch()
    {
        var script = PowerShellScriptLibrary.ElevationTestScript;

        ScriptHashValidator.IsAllowed(script, "INVALID").Should().BeFalse();
    }
}
