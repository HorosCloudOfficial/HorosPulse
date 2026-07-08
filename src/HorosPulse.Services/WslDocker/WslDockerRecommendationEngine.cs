namespace HorosPulse.Services.WslDocker;

using HorosPulse.Core.Models;

/// <summary>
/// Empfehlungslogik für WSL2-Ressourcen-Limits beim Dev-/Docker-Setup.
/// </summary>
public static class WslDockerRecommendationEngine
{
    public static WslConfigRecommendedLimits ComputeRecommendedLimits(long systemRamMb, int logicalProcessors)
    {
        var memoryMb = ComputeRecommendedMemoryMb(systemRamMb);
        var processors = ComputeRecommendedProcessors(logicalProcessors);
        var swapMb = ComputeRecommendedSwapMb(memoryMb, systemRamMb);

        return new WslConfigRecommendedLimits
        {
            MemoryMb = memoryMb,
            Processors = processors,
            SwapMb = swapMb,
            LocalhostForwarding = true,
            NestedVirtualization = true,
            PageReporting = true,
        };
    }

    public static IReadOnlyList<WslDockerRecommendation> BuildRecommendations(
        WslConfigLimits current,
        WslConfigRecommendedLimits recommended,
        bool isWslInstalled,
        bool isWsl2Default,
        bool isDockerLikely,
        bool buildDefenderDockerOk,
        bool buildDefenderWslOk)
    {
        var items = new List<WslDockerRecommendation>();

        if (!isWslInstalled)
        {
            items.Add(new WslDockerRecommendation
            {
                Key = "wsl-missing",
                Title = "WSL nicht erkannt",
                Description = "Für Container- und Linux-Dev-Workflows WSL2 installieren (Microsoft Store oder wsl --install).",
                CurrentDisplay = "Nicht installiert",
                RecommendedDisplay = "WSL2 aktiv",
                IsOptimal = false,
                Severity = WslDockerRecommendationSeverity.Warning,
            });
            return items;
        }

        if (!isWsl2Default)
        {
            items.Add(new WslDockerRecommendation
            {
                Key = "wsl-version",
                Title = "WSL-Standardversion",
                Description = ".wslconfig gilt nur für WSL2. Standard auf Version 2 setzen: wsl --set-default-version 2",
                CurrentDisplay = "Nicht WSL2 (Standard)",
                RecommendedDisplay = "WSL2",
                IsOptimal = false,
                Severity = WslDockerRecommendationSeverity.Warning,
            });
        }

        items.Add(CreateLimitRecommendation(
            "memory",
            "Arbeitsspeicher (memory)",
            "Zu wenig RAM verlangsamt Docker-Builds und Compiler in WSL. Zu viel RAM kann Windows unter Last ausbremsen.",
            current.MemoryMb,
            current.MemoryUsesDefault,
            recommended.MemoryMb,
            toleranceMb: 512));

        items.Add(CreateProcessorRecommendation(
            "processors",
            "Prozessoren (processors)",
            "Dedizierte Kerne für parallele Builds (make -j, dotnet, npm). Windows behält Reserve-Kerne für UI und Defender.",
            current.Processors,
            current.ProcessorsUsesDefault,
            recommended.Processors));

        items.Add(CreateLimitRecommendation(
            "swap",
            "Swap (swap)",
            "Swap puffert Speicherspitzen bei Docker-Containern. 0 deaktiviert Swap — nur bei sehr knappem Speicher sinnvoll.",
            current.SwapMb,
            current.SwapUsesDefault,
            recommended.SwapMb,
            toleranceMb: 512));

        items.Add(CreateBoolRecommendation(
            "localhostForwarding",
            "localhostForwarding",
            "Erlaubt localhost:Port vom Windows-Host auf WSL-Dienste (Dev-Server, APIs).",
            current.LocalhostForwarding,
            recommended.LocalhostForwarding));

        items.Add(CreateBoolRecommendation(
            "nestedVirtualization",
            "nestedVirtualization",
            "Benötigt für Docker-in-WSL und verschachtelte VMs. Bei reinem Docker Desktop meist unkritisch.",
            current.NestedVirtualization,
            recommended.NestedVirtualization));

        if (isDockerLikely)
        {
            items.Add(new WslDockerRecommendation
            {
                Key = "build-defender",
                Title = "Build-Schutz (Defender)",
                Description = "docker.exe und wsl.exe sollten als Prozess-Ausschlüsse im Modul „Build-Schutz“ aktiv sein — nicht hier duplizieren.",
                CurrentDisplay = buildDefenderDockerOk && buildDefenderWslOk
                    ? "docker.exe + wsl.exe ausgeschlossen"
                    : $"docker.exe: {(buildDefenderDockerOk ? "OK" : "fehlt")}, wsl.exe: {(buildDefenderWslOk ? "OK" : "fehlt")}",
                RecommendedDisplay = "Build-Schutz öffnen und anwenden",
                IsOptimal = buildDefenderDockerOk && buildDefenderWslOk,
                Severity = buildDefenderDockerOk && buildDefenderWslOk
                    ? WslDockerRecommendationSeverity.Info
                    : WslDockerRecommendationSeverity.Suggestion,
            });
        }

        return items;
    }

    public static bool IsDevSetupOptimal(IReadOnlyList<WslDockerRecommendation> recommendations) =>
        recommendations
            .Where(r => r.Key is not "build-defender")
            .All(r => r.IsOptimal);

    public static string BuildSummary(
        bool isWslInstalled,
        bool isOptimal,
        bool hasHorosPulseChanges,
        WslConfigRecommendedLimits recommended)
    {
        if (!isWslInstalled)
            return "WSL nicht erkannt — Installation empfohlen für Linux-/Docker-Dev-Workflows.";

        if (isOptimal)
            return "WSL2-Limits passen zum empfohlenen Dev-Setup." +
                   (hasHorosPulseChanges ? " HorosPulse-Änderungen können per Rollback zurückgesetzt werden." : string.Empty);

        return $"Empfehlung: memory={WslConfigParser.FormatSizeMb(recommended.MemoryMb)}, " +
               $"processors={recommended.Processors}, swap={WslConfigParser.FormatSizeMb(recommended.SwapMb)}. " +
               "Nach Apply: wsl --shutdown ausführen." +
               (hasHorosPulseChanges ? " Rollback verfügbar." : string.Empty);
    }

    internal static long ComputeRecommendedMemoryMb(long systemRamMb) =>
        systemRamMb switch
        {
            <= 8 * 1024 => RoundDownToGb(Math.Min(4096, (long)(systemRamMb * 0.45))),
            <= 16 * 1024 => RoundDownToGb((long)(systemRamMb * 0.5)),
            <= 32 * 1024 => RoundDownToGb((long)(systemRamMb * 0.55)),
            <= 64 * 1024 => RoundDownToGb((long)(systemRamMb * 0.5)),
            _ => RoundDownToGb(Math.Min((long)(systemRamMb * 0.55), 48 * 1024)),
        };

    internal static int ComputeRecommendedProcessors(int logicalProcessors)
    {
        var reserved = logicalProcessors switch
        {
            <= 4 => 1,
            <= 8 => 2,
            <= 16 => 3,
            _ => Math.Max(4, logicalProcessors / 4),
        };

        return Math.Clamp(logicalProcessors - reserved, 2, logicalProcessors);
    }

    internal static long ComputeRecommendedSwapMb(long recommendedMemoryMb, long systemRamMb)
    {
        var halfMemory = recommendedMemoryMb / 2;
        var quarterSystem = RoundUpToGb((long)Math.Ceiling(systemRamMb * 0.25 / 1024.0) * 1024);
        return Math.Max(4096, Math.Max(halfMemory, quarterSystem));
    }

    private static WslDockerRecommendation CreateLimitRecommendation(
        string key,
        string title,
        string description,
        long? currentValue,
        bool usesDefault,
        long recommendedValue,
        int toleranceMb)
    {
        var currentDisplay = usesDefault || currentValue is null
            ? "Standard"
            : WslConfigParser.FormatSizeMb(currentValue.Value);

        var recommendedDisplay = WslConfigParser.FormatSizeMb(recommendedValue);

        var isOptimal = !usesDefault &&
                        currentValue is not null &&
                        Math.Abs(currentValue.Value - recommendedValue) <= toleranceMb;

        return new WslDockerRecommendation
        {
            Key = key,
            Title = title,
            Description = description,
            CurrentDisplay = currentDisplay,
            RecommendedDisplay = recommendedDisplay,
            IsOptimal = isOptimal,
            Severity = isOptimal ? WslDockerRecommendationSeverity.Info : WslDockerRecommendationSeverity.Suggestion,
        };
    }

    private static WslDockerRecommendation CreateProcessorRecommendation(
        string key,
        string title,
        string description,
        int? currentValue,
        bool usesDefault,
        int recommendedValue)
    {
        var currentDisplay = usesDefault || currentValue is null
            ? "Standard"
            : currentValue.Value.ToString();

        var isOptimal = !usesDefault && currentValue == recommendedValue;

        return new WslDockerRecommendation
        {
            Key = key,
            Title = title,
            Description = description,
            CurrentDisplay = currentDisplay,
            RecommendedDisplay = recommendedValue.ToString(),
            IsOptimal = isOptimal,
            Severity = isOptimal ? WslDockerRecommendationSeverity.Info : WslDockerRecommendationSeverity.Suggestion,
        };
    }

    private static WslDockerRecommendation CreateBoolRecommendation(
        string key,
        string title,
        string description,
        bool? current,
        bool recommended)
    {
        var currentDisplay = current switch
        {
            true => "true",
            false => "false",
            _ => "Standard (true)",
        };

        var isOptimal = current == recommended || (current is null && recommended);

        return new WslDockerRecommendation
        {
            Key = key,
            Title = title,
            Description = description,
            CurrentDisplay = currentDisplay,
            RecommendedDisplay = recommended ? "true" : "false",
            IsOptimal = isOptimal,
            Severity = isOptimal ? WslDockerRecommendationSeverity.Info : WslDockerRecommendationSeverity.Suggestion,
        };
    }

    private static long RoundDownToGb(long megabytes) =>
        Math.Max(2048, megabytes / 1024 * 1024);

    private static long RoundUpToGb(long megabytes) =>
        ((megabytes + 1023) / 1024) * 1024;
}
