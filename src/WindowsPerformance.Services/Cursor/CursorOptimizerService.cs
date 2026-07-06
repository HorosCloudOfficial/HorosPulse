namespace WindowsPerformance.Services.Cursor;

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed class CursorOptimizerService : ICursorOptimizer
{
    private static readonly IReadOnlyDictionary<string, object?> PerformanceTemplate = new Dictionary<string, object?>
    {
        ["typescript.tsserver.maxTsServerMemory"] = 8192,
        ["files.watcherExclude"] = new Dictionary<string, object?>
        {
            ["**/node_modules/**"] = true,
            ["**/.git/objects/**"] = true,
            ["**/.git/subtree-cache/**"] = true,
            ["**/dist/**"] = true,
            ["**/build/**"] = true,
        },
        ["search.exclude"] = new Dictionary<string, object?>
        {
            ["**/node_modules"] = true,
            ["**/bower_components"] = true,
            ["**/.git"] = true,
            ["**/dist"] = true,
            ["**/build"] = true,
        },
        ["editor.minimap.enabled"] = false,
        ["telemetry.telemetryLevel"] = "off",
        ["files.autoSave"] = "onFocusChange",
        ["files.encoding"] = "utf8",
        ["files.trimTrailingWhitespace"] = true,
        ["files.insertFinalNewline"] = true,
        ["editor.formatOnSave"] = true,
        ["editor.tabSize"] = 4,
        ["editor.insertSpaces"] = true,
        ["[csharp]"] = new Dictionary<string, object?>
        {
            ["editor.defaultFormatter"] = "ms-dotnettools.csharp",
            ["editor.formatOnSave"] = true,
            ["editor.codeActionsOnSave"] = new Dictionary<string, object?>
            {
                ["source.organizeImports"] = "explicit",
            },
        },
        ["[markdown]"] = new Dictionary<string, object?>
        {
            ["editor.wordWrap"] = "on",
            ["files.trimTrailingWhitespace"] = false,
        },
    };

    private readonly ILogger<CursorOptimizerService> _logger;
    private readonly string _settingsPath;
    private readonly string _backupPath;

    public CursorOptimizerService(ILogger<CursorOptimizerService> logger)
    {
        _logger = logger;
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Cursor", "User", "settings.json");
        _backupPath = _settingsPath + ".windowsperformance.bak";
    }

    public string SettingsPath => _settingsPath;

    public bool HasBackup => File.Exists(_backupPath);

    public async Task<CursorSettingsProfile> PreviewOptimizationsAsync(CancellationToken cancellationToken = default)
    {
        var currentJson = await ReadSettingsJsonAsync(cancellationToken);
        var merged = JsonSettingsMerger.Merge(currentJson, PerformanceTemplate);
        var changedKeys = JsonSettingsMerger.GetChangedKeys(currentJson, merged);

        return new CursorSettingsProfile
        {
            Settings = JsonSettingsMerger.ToDictionary(merged),
            ChangedKeys = changedKeys,
            HasBackup = HasBackup,
        };
    }

    public async Task<OptimizationResult> ApplyOptimizationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            if (File.Exists(_settingsPath) && !File.Exists(_backupPath))
            {
                File.Copy(_settingsPath, _backupPath, overwrite: false);
                _logger.LogInformation("settings.json gesichert nach {Backup}", _backupPath);
            }

            var currentJson = await ReadSettingsJsonAsync(cancellationToken);
            var merged = JsonSettingsMerger.Merge(currentJson, PerformanceTemplate);
            var changedKeys = JsonSettingsMerger.GetChangedKeys(currentJson, merged);

            await File.WriteAllTextAsync(_settingsPath, merged.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), cancellationToken);
            _logger.LogInformation("Cursor settings.json optimiert ({Count} Schlüssel)", changedKeys.Count);

            return OptimizationResult.Ok(changedKeys.Select(k => $"Geändert: {k}").ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cursor-Optimierung fehlgeschlagen");
            return OptimizationResult.Fail(ex.Message);
        }
    }

    public Task<OptimizationResult> RevertOptimizationsAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_backupPath))
            return Task.FromResult(OptimizationResult.Fail("Kein Backup von settings.json vorhanden."));

        try
        {
            File.Copy(_backupPath, _settingsPath, overwrite: true);
            File.Delete(_backupPath);
            _logger.LogInformation("Cursor settings.json aus Backup wiederhergestellt");
            return Task.FromResult(OptimizationResult.Ok("settings.json wiederhergestellt"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cursor-Rollback fehlgeschlagen");
            return Task.FromResult(OptimizationResult.Fail(ex.Message));
        }
    }

    private async Task<JsonObject> ReadSettingsJsonAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_settingsPath))
            return new JsonObject();

        var content = await File.ReadAllTextAsync(_settingsPath, cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
            return new JsonObject();

        return JsonNode.Parse(content)?.AsObject() ?? new JsonObject();
    }
}

internal static class JsonSettingsMerger
{
    public static JsonObject Merge(JsonObject current, IReadOnlyDictionary<string, object?> template)
    {
        var result = current.DeepClone()?.AsObject() ?? new JsonObject();

        foreach (var (key, value) in template)
        {
            if (value is IReadOnlyDictionary<string, object?> dictValue)
            {
                var existing = result[key]?.AsObject() ?? new JsonObject();
                foreach (var (subKey, subValue) in dictValue)
                {
                    existing[subKey] = subValue is IReadOnlyDictionary<string, object?> nested
                        ? DictionaryToJsonObject(nested)
                        : JsonValue.Create(subValue);
                }

                result[key] = existing;
            }
            else
            {
                result[key] = JsonValue.Create(value);
            }
        }

        return result;
    }

    private static JsonObject DictionaryToJsonObject(IReadOnlyDictionary<string, object?> dict)
    {
        var obj = new JsonObject();
        foreach (var (key, value) in dict)
            obj[key] = JsonValue.Create(value);

        return obj;
    }

    public static IReadOnlyList<string> GetChangedKeys(JsonObject before, JsonObject after)
    {
        var changed = new List<string>();
        foreach (var property in after)
        {
            var beforeValue = before[property.Key]?.ToJsonString() ?? "null";
            var afterValue = property.Value?.ToJsonString() ?? "null";
            if (!string.Equals(beforeValue, afterValue, StringComparison.Ordinal))
                changed.Add(property.Key);
        }

        return changed;
    }

    public static IReadOnlyDictionary<string, object?> ToDictionary(JsonObject obj)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var property in obj)
            dict[property.Key] = property.Value?.GetValue<object>();

        return dict;
    }
}
