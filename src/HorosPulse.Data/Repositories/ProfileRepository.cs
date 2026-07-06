namespace HorosPulse.Data.Repositories;

using System.Text.Json;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed class ProfileRepository : IProfileRepository
{
    public async Task<IReadOnlyList<ProfileDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        DataPaths.EnsureDirectories();
        var profiles = new List<ProfileDefinition>();

        foreach (var file in Directory.EnumerateFiles(DataPaths.ProfilesDirectory, "*.json"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var profile = await ReadProfileAsync(file, cancellationToken);
            if (profile is not null)
                profiles.Add(profile);
        }

        return profiles.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<ProfileDefinition?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var path = GetProfilePath(id);
        if (!File.Exists(path))
            return null;

        return await ReadProfileAsync(path, cancellationToken);
    }

    public async Task SaveAsync(ProfileDefinition profile, CancellationToken cancellationToken = default)
    {
        DataPaths.EnsureDirectories();
        var path = GetProfilePath(profile.Id);
        var json = JsonSerializer.Serialize(profile, JsonDefaults.Options);
        await File.WriteAllTextAsync(path, json, cancellationToken);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var path = GetProfilePath(id);
        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    public async Task EnsureDefaultPresetsAsync(CancellationToken cancellationToken = default)
    {
        DataPaths.EnsureDirectories();

        foreach (var preset in DefaultPresets.All)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = GetProfilePath(preset.Id);
            if (!File.Exists(path))
                await SaveAsync(preset, cancellationToken);
        }
    }

    private static string GetProfilePath(string id) =>
        Path.Combine(DataPaths.ProfilesDirectory, $"{id}.json");

    private static async Task<ProfileDefinition?> ReadProfileAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            var json = await File.ReadAllTextAsync(path, cancellationToken);
            return JsonSerializer.Deserialize<ProfileDefinition>(json, JsonDefaults.Options);
        }
        catch
        {
            return null;
        }
    }
}
