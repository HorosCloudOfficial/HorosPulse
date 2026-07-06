namespace HorosPulse.Services.Settings;

using System.Text.Json;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Data;

public sealed class AppSettingsService : IAppSettingsService
{
    private AppSettings _current = new();

    public AppSettings Current => _current;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        DataPaths.EnsureDirectories();

        if (!File.Exists(DataPaths.SettingsPath))
        {
            _current = new AppSettings();
            await SaveAsync(cancellationToken);
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(DataPaths.SettingsPath, cancellationToken);
            _current = JsonSerializer.Deserialize<AppSettings>(json, JsonDefaults.Options) ?? new AppSettings();
        }
        catch
        {
            _current = new AppSettings();
        }
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        DataPaths.EnsureDirectories();
        var json = JsonSerializer.Serialize(_current, JsonDefaults.Options);
        await File.WriteAllTextAsync(DataPaths.SettingsPath, json, cancellationToken);
    }
}
