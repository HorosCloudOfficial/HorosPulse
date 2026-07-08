namespace HorosPulse.Services.CodingBoost;

/// <summary>
/// Parst und merged DirectXUserGlobalSettings (SwapEffectUpgradeEnable, VRROptimizeEnable, …).
/// </summary>
public static class CodingBoostDirectXSettingsHelper
{
    public const string SwapEffectUpgradeKey = "SwapEffectUpgradeEnable";

    public static IReadOnlyDictionary<string, string> Parse(string? value)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(value))
            return result;

        foreach (var part in value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = part.IndexOf('=');
            if (separator <= 0)
                continue;

            var key = part[..separator].Trim();
            var settingValue = part[(separator + 1)..].Trim();
            if (key.Length > 0)
                result[key] = settingValue;
        }

        return result;
    }

    public static string Merge(IReadOnlyDictionary<string, string> settings)
    {
        if (settings.Count == 0)
            return string.Empty;

        return string.Join(";", settings.Select(kv => $"{kv.Key}={kv.Value}")) + ";";
    }

    public static string SetSetting(string? current, string key, string value)
    {
        var dict = new Dictionary<string, string>(Parse(current), StringComparer.OrdinalIgnoreCase);
        dict[key] = value;
        return Merge(dict);
    }

    public static bool IsSettingEnabled(string? current, string key) =>
        Parse(current).TryGetValue(key, out var value) && value == "1";

    public static bool IsWindowedOptimizationEnabled(string? directXUserGlobalSettings, int? swapEffectUpgradeCache) =>
        IsSettingEnabled(directXUserGlobalSettings, SwapEffectUpgradeKey) || swapEffectUpgradeCache == 1;
}
