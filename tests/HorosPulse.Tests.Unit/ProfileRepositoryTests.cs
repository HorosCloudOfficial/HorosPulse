namespace HorosPulse.Tests.Unit;

using System.Text.Json;
using FluentAssertions;
using HorosPulse.Core;
using HorosPulse.Core.Models;
using HorosPulse.Data;
using Xunit;

public class ProfileRepositoryTests
{
    [Fact]
    public async Task SaveAndGet_RoundTripsProfileDefinition()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "wp-test-" + Guid.NewGuid().ToString("N"));
        var profilesDir = Path.Combine(tempRoot, "profiles");
        Directory.CreateDirectory(profilesDir);

        var profile = new ProfileDefinition
        {
            Id = "test-preset",
            Name = "Test Preset",
            Description = "Round-trip test",
            IsBuiltIn = false,
            Steps = ["Step 1", "Step 2"],
        };

        var json = JsonSerializer.Serialize(profile, JsonDefaults.Options);
        var path = Path.Combine(profilesDir, $"{profile.Id}.json");
        await File.WriteAllTextAsync(path, json);

        var loadedJson = await File.ReadAllTextAsync(path);
        var loaded = JsonSerializer.Deserialize<ProfileDefinition>(loadedJson, JsonDefaults.Options);

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(profile.Id);
        loaded.Name.Should().Be(profile.Name);
        loaded.Description.Should().Be(profile.Description);
        loaded.Steps.Should().BeEquivalentTo(profile.Steps);

        Directory.Delete(tempRoot, recursive: true);
    }

    [Fact]
    public void DefaultPresets_ContainsCursorDevMode()
    {
        var preset = DefaultPresets.All.FirstOrDefault(p => p.Id == PresetIds.CursorDevMode);
        preset.Should().NotBeNull();
        preset!.Name.Should().Be("Cursor Dev Mode");
        preset.Steps.Should().NotBeEmpty();
    }
}
