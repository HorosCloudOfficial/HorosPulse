namespace WindowsPerformance.Tests.Unit;

using FluentAssertions;
using System.Text.Json.Nodes;
using WindowsPerformance.Services.Cursor;
using WindowsPerformance.Services.PowerPlan;
using WindowsPerformance.Services.PowerShell;
using Xunit;

public class PowerShellBridgeTests
{
    [Fact]
    public void ScriptSanitizer_BlocksDangerousPatterns()
    {
        ScriptSanitizer.IsScriptSafe("Get-Process").Should().BeTrue();
        ScriptSanitizer.IsScriptSafe("Remove-Item C:\\Windows\\System32").Should().BeFalse();
        ScriptSanitizer.IsScriptSafe("Format-Volume -DriveLetter C").Should().BeFalse();
    }

    [Fact]
    public void ScriptSanitizer_RequireSafeScript_ThrowsOnBlocked()
    {
        var act = () => ScriptSanitizer.RequireSafeScript("Stop-Computer -Force");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task RunProcessAsync_ReturnsOutputForValidCommand()
    {
        var pwsh = PowerShellBridge.ResolveExecutable(new PowerShellOptions());
        if (pwsh is null)
            return;

        var result = await PowerShellBridge.RunProcessAsync(
            pwsh,
            "Write-Output 'hello'",
            TimeSpan.FromSeconds(15),
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StdOut.Trim().Should().Be("hello");
    }
}

public class CursorOptimizerServiceTests
{
    [Fact]
    public void JsonSettingsMerger_PreservesUserKeysAndMergesTemplate()
    {
        var current = JsonNode.Parse("""
            {
              "editor.fontSize": 16,
              "files.watcherExclude": { "**/custom/**": true }
            }
            """)!.AsObject();

        var template = new Dictionary<string, object?>
        {
            ["typescript.tsserver.maxTsServerMemory"] = 8192,
            ["editor.minimap.enabled"] = false,
            ["files.watcherExclude"] = new Dictionary<string, object?>
            {
                ["**/node_modules/**"] = true,
            },
        };

        var merged = JsonSettingsMerger.Merge(current, template);

        merged["editor.fontSize"]!.GetValue<int>().Should().Be(16);
        merged["typescript.tsserver.maxTsServerMemory"]!.GetValue<int>().Should().Be(8192);
        merged["files.watcherExclude"]!["**/custom/**"]!.GetValue<bool>().Should().BeTrue();
        merged["files.watcherExclude"]!["**/node_modules/**"]!.GetValue<bool>().Should().BeTrue();
    }

    [Fact]
    public void JsonSettingsMerger_GetChangedKeys_DetectsDifferences()
    {
        var before = new JsonObject { ["editor.minimap.enabled"] = true };
        var after = new JsonObject { ["editor.minimap.enabled"] = false, ["telemetry.telemetryLevel"] = "off" };

        var changed = JsonSettingsMerger.GetChangedKeys(before, after);
        changed.Should().Contain("editor.minimap.enabled");
        changed.Should().Contain("telemetry.telemetryLevel");
    }
}

public class PowerPlanServiceTests
{
    [Fact]
    public void ParsePowerPlans_ExtractsGuidNameAndActiveFlag()
    {
        const string sample = """
            Existing Power Schemes (* Active)
            -----------------------------------
            Power Scheme GUID: 381b4222-f694-41fe-9125-ee574960750b  (Balanced)
            Power Scheme GUID: 8c5e7fda-e8bf-4a96-9a85-a6e23a67c135  (High performance) *
            """;

        var plans = PowerPlanService.ParsePowerPlans(sample);

        plans.Should().HaveCount(2);
        plans[0].Name.Should().Be("Balanced");
        plans[0].IsActive.Should().BeFalse();
        plans[1].Name.Should().Be("High performance");
        plans[1].IsActive.Should().BeTrue();
        plans[1].Guid.Should().Be(Guid.Parse("8c5e7fda-e8bf-4a96-9a85-a6e23a67c135"));
    }
}
