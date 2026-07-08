namespace HorosPulse.Services.WslDocker;

using System.Globalization;
using System.Text;
using HorosPulse.Core.Models;

/// <summary>
/// Parser und Serializer für %USERPROFILE%\.wslconfig (INI-Format, [wsl2]-Sektion).
/// </summary>
public static class WslConfigParser
{
    public const string Wsl2Section = "wsl2";

    public sealed class WslConfigDocument
    {
        public Dictionary<string, string> Wsl2Settings { get; } = new(StringComparer.OrdinalIgnoreCase);

        public List<ConfigSection> OtherSections { get; } = [];

        public List<string> PreambleLines { get; } = [];
    }

    public sealed class ConfigSection
    {
        public required string Name { get; init; }

        public List<string> Lines { get; } = [];
    }

    public static WslConfigDocument Parse(string? content)
    {
        var document = new WslConfigDocument();
        if (string.IsNullOrWhiteSpace(content))
            return document;

        ConfigSection? currentSection = null;
        var inWsl2 = false;

        foreach (var rawLine in content.Split(['\r', '\n'], StringSplitOptions.None))
        {
            var line = rawLine.TrimEnd();
            var trimmed = line.Trim();

            if (trimmed.Length == 0)
            {
                if (currentSection is null && !inWsl2)
                    document.PreambleLines.Add(line);
                else if (currentSection is not null)
                    currentSection.Lines.Add(line);
                continue;
            }

            if (trimmed.StartsWith('#') || trimmed.StartsWith(';'))
            {
                if (currentSection is null && !inWsl2)
                    document.PreambleLines.Add(line);
                else if (currentSection is not null)
                    currentSection.Lines.Add(line);
                continue;
            }

            if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
            {
                var sectionName = trimmed[1..^1].Trim();
                inWsl2 = sectionName.Equals(Wsl2Section, StringComparison.OrdinalIgnoreCase);
                if (!inWsl2)
                {
                    currentSection = new ConfigSection { Name = sectionName };
                    document.OtherSections.Add(currentSection);
                }
                else
                {
                    currentSection = null;
                }

                continue;
            }

            var equalsIndex = trimmed.IndexOf('=');
            if (equalsIndex <= 0)
            {
                if (currentSection is not null)
                    currentSection.Lines.Add(line);
                else if (!inWsl2)
                    document.PreambleLines.Add(line);
                continue;
            }

            var key = trimmed[..equalsIndex].Trim();
            var value = trimmed[(equalsIndex + 1)..].Trim();
            if (inWsl2)
                document.Wsl2Settings[key] = value;
            else if (currentSection is not null)
                currentSection.Lines.Add(line);
        }

        return document;
    }

    public static WslConfigLimits ResolveEffectiveLimits(
        WslConfigDocument? document,
        long systemRamMb,
        int logicalProcessors)
    {
        var settings = document?.Wsl2Settings ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var memoryExplicit = TryParseSizeMb(settings, "memory", out var memoryMb);
        var processorsExplicit = TryParseInt(settings, "processors", out var processors);
        var swapExplicit = TryParseSizeMb(settings, "swap", out var swapMb);
        var localhostExplicit = TryParseBool(settings, "localhostForwarding", out var localhost);
        var nestedExplicit = TryParseBool(settings, "nestedVirtualization", out var nested);
        var pageReportingExplicit = TryParseBool(settings, "pageReporting", out var pageReporting);

        var defaultMemoryMb = ComputeDefaultMemoryMb(systemRamMb);
        var defaultSwapMb = ComputeDefaultSwapMb(systemRamMb);

        return new WslConfigLimits
        {
            MemoryMb = memoryExplicit ? memoryMb : defaultMemoryMb,
            MemoryUsesDefault = !memoryExplicit,
            Processors = processorsExplicit ? processors : logicalProcessors,
            ProcessorsUsesDefault = !processorsExplicit,
            SwapMb = swapExplicit ? swapMb : defaultSwapMb,
            SwapUsesDefault = !swapExplicit,
            LocalhostForwarding = localhostExplicit ? localhost : true,
            NestedVirtualization = nestedExplicit ? nested : true,
            PageReporting = pageReportingExplicit ? pageReporting : null,
        };
    }

    public static WslConfigLimits CreateDefaultLimits(long systemRamMb, int logicalProcessors) =>
        new()
        {
            MemoryMb = ComputeDefaultMemoryMb(systemRamMb),
            MemoryUsesDefault = true,
            Processors = logicalProcessors,
            ProcessorsUsesDefault = true,
            SwapMb = ComputeDefaultSwapMb(systemRamMb),
            SwapUsesDefault = true,
            LocalhostForwarding = true,
            NestedVirtualization = true,
        };

    public static WslConfigLimits CreateSystemResourceLimits(long systemRamMb, int logicalProcessors) =>
        new()
        {
            MemoryMb = systemRamMb,
            MemoryUsesDefault = false,
            Processors = logicalProcessors,
            ProcessorsUsesDefault = false,
            SwapMb = null,
            SwapUsesDefault = true,
        };

    public static string MergeRecommendedSettings(
        string? existingContent,
        WslConfigRecommendedLimits recommended,
        bool includeHorosPulseHeader = true)
    {
        var document = Parse(existingContent);
        document.Wsl2Settings["memory"] = FormatSizeMb(recommended.MemoryMb);
        document.Wsl2Settings["processors"] = recommended.Processors.ToString(CultureInfo.InvariantCulture);
        document.Wsl2Settings["swap"] = FormatSizeMb(recommended.SwapMb);
        document.Wsl2Settings["localhostForwarding"] = recommended.LocalhostForwarding ? "true" : "false";
        document.Wsl2Settings["nestedVirtualization"] = recommended.NestedVirtualization ? "true" : "false";
        document.Wsl2Settings["pageReporting"] = recommended.PageReporting ? "true" : "false";
        return Serialize(document, includeHorosPulseHeader);
    }

    public static string Serialize(WslConfigDocument document, bool includeHorosPulseHeader)
    {
        var sb = new StringBuilder();

        if (includeHorosPulseHeader)
        {
            sb.AppendLine("# Konfiguration durch HorosPulse (HorosCode) — WSL2 Dev-Tuning");
            sb.AppendLine("# Nach Änderungen: wsl --shutdown ausführen, dann WSL/Docker neu starten.");
            sb.AppendLine("# Dokumentation: https://learn.microsoft.com/windows/wsl/wsl-config");
            sb.AppendLine();
        }
        else
        {
            foreach (var line in document.PreambleLines)
                sb.AppendLine(line);

            if (document.PreambleLines.Count > 0)
                sb.AppendLine();
        }

        sb.AppendLine("[wsl2]");

        AppendSetting(sb, "memory", document.Wsl2Settings);
        AppendSetting(sb, "processors", document.Wsl2Settings);
        AppendSetting(sb, "swap", document.Wsl2Settings);
        AppendSetting(sb, "localhostForwarding", document.Wsl2Settings);
        AppendSetting(sb, "nestedVirtualization", document.Wsl2Settings);
        AppendSetting(sb, "pageReporting", document.Wsl2Settings);

        foreach (var pair in document.Wsl2Settings.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (IsManagedKey(pair.Key))
                continue;

            sb.AppendLine($"{pair.Key}={pair.Value}");
        }

        foreach (var section in document.OtherSections)
        {
            sb.AppendLine();
            sb.AppendLine($"[{section.Name}]");
            foreach (var line in section.Lines)
                sb.AppendLine(line);
        }

        return sb.ToString().TrimEnd() + Environment.NewLine;
    }

    internal static long ComputeDefaultMemoryMb(long systemRamMb) =>
        Math.Max(1024, (long)Math.Round(systemRamMb * 0.5, MidpointRounding.AwayFromZero));

    internal static long ComputeDefaultSwapMb(long systemRamMb)
    {
        var quarterGb = (long)Math.Ceiling(systemRamMb / 1024.0 * 0.25);
        return Math.Max(1024, quarterGb * 1024);
    }

    internal static bool TryParseSizeMb(IReadOnlyDictionary<string, string> settings, string key, out long megabytes)
    {
        megabytes = 0;
        if (!settings.TryGetValue(key, out var raw) || string.IsNullOrWhiteSpace(raw))
            return false;

        raw = raw.Trim();
        if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var plain))
        {
            megabytes = plain >= 1024 ? plain / (1024 * 1024) : plain;
            return true;
        }

        var normalized = raw.Replace(" ", string.Empty, StringComparison.Ordinal);
        if (normalized.EndsWith("GB", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(normalized[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var gb))
        {
            megabytes = (long)Math.Round(gb * 1024, MidpointRounding.AwayFromZero);
            return true;
        }

        if (normalized.EndsWith("MB", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(normalized[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var mb))
        {
            megabytes = (long)Math.Round(mb, MidpointRounding.AwayFromZero);
            return true;
        }

        if (normalized.EndsWith("B", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(normalized[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var bytes))
        {
            megabytes = (long)Math.Round(bytes / (1024.0 * 1024.0), MidpointRounding.AwayFromZero);
            return true;
        }

        return false;
    }

    internal static bool TryParseInt(IReadOnlyDictionary<string, string> settings, string key, out int value)
    {
        value = 0;
        return settings.TryGetValue(key, out var raw) &&
               int.TryParse(raw.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    internal static bool TryParseBool(IReadOnlyDictionary<string, string> settings, string key, out bool value)
    {
        value = false;
        if (!settings.TryGetValue(key, out var raw))
            return false;

        raw = raw.Trim();
        if (bool.TryParse(raw, out value))
            return true;

        if (raw == "1")
        {
            value = true;
            return true;
        }

        if (raw == "0")
        {
            value = false;
            return true;
        }

        return false;
    }

    internal static string FormatSizeMb(long megabytes) =>
        megabytes % 1024 == 0
            ? $"{megabytes / 1024}GB"
            : $"{megabytes}MB";

    private static void AppendSetting(StringBuilder sb, string key, IReadOnlyDictionary<string, string> settings)
    {
        if (settings.TryGetValue(key, out var value))
            sb.AppendLine($"{key}={value}");
    }

    private static bool IsManagedKey(string key) =>
        key.Equals("memory", StringComparison.OrdinalIgnoreCase) ||
        key.Equals("processors", StringComparison.OrdinalIgnoreCase) ||
        key.Equals("swap", StringComparison.OrdinalIgnoreCase) ||
        key.Equals("localhostForwarding", StringComparison.OrdinalIgnoreCase) ||
        key.Equals("nestedVirtualization", StringComparison.OrdinalIgnoreCase) ||
        key.Equals("pageReporting", StringComparison.OrdinalIgnoreCase);
}
