namespace WindowsPerformance.Services.PowerPlan;

using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed partial class PowerPlanService : IPowerPlanService
{
    private static readonly Guid HighPerformanceGuid = Guid.Parse("8c5e7fda-e8bf-4a96-9a85-a6e23a67c135");
    private static readonly Guid UltimatePerformanceGuid = Guid.Parse("e9a42b02-d5df-448d-aa00-03f14749ebe5");

    private readonly ILogger<PowerPlanService> _logger;
    private Guid? _previousActiveGuid;

    public PowerPlanService(ILogger<PowerPlanService> logger) => _logger = logger;

    public async Task<IReadOnlyList<PowerPlanInfo>> GetAvailablePlansAsync(CancellationToken cancellationToken = default)
    {
        var output = await RunPowerCfgAsync("/list", cancellationToken);
        return ParsePowerPlans(output);
    }

    public async Task<PowerPlanInfo?> GetActivePlanAsync(CancellationToken cancellationToken = default)
    {
        var plans = await GetAvailablePlansAsync(cancellationToken);
        return plans.FirstOrDefault(p => p.IsActive);
    }

    public async Task<OptimizationResult> SetActivePlanAsync(Guid planGuid, CancellationToken cancellationToken = default)
    {
        var active = await GetActivePlanAsync(cancellationToken);
        _previousActiveGuid ??= active?.Guid;

        var output = await RunPowerCfgAsync($"/setactive {planGuid:D}", cancellationToken);
        if (output.Contains("Unable", StringComparison.OrdinalIgnoreCase) ||
            output.Contains("Fehler", StringComparison.OrdinalIgnoreCase))
        {
            return OptimizationResult.Fail($"Energieplan konnte nicht gesetzt werden: {output.Trim()}");
        }

        _logger.LogInformation("Energieplan gesetzt: {Guid}", planGuid);
        return OptimizationResult.Ok($"Energieplan {planGuid:D} aktiviert");
    }

    public async Task<OptimizationResult> EnsureHighPerformancePlanAsync(CancellationToken cancellationToken = default)
    {
        var plans = await GetAvailablePlansAsync(cancellationToken);
        var highPerf = plans.FirstOrDefault(p =>
            p.Name.Contains("High performance", StringComparison.OrdinalIgnoreCase) ||
            p.Name.Contains("Höchstleistung", StringComparison.OrdinalIgnoreCase) ||
            p.Guid == HighPerformanceGuid);

        if (highPerf is not null)
            return await SetActivePlanAsync(highPerf.Guid, cancellationToken);

        var duplicateOutput = await RunPowerCfgAsync($"/duplicatescheme {HighPerformanceGuid:D}", cancellationToken);
        var match = DuplicateGuidRegex().Match(duplicateOutput);
        if (!match.Success)
            return OptimizationResult.Fail("High-Performance-Plan konnte nicht erstellt werden.");

        var newGuid = Guid.Parse(match.Groups[1].Value);
        return await SetActivePlanAsync(newGuid, cancellationToken);
    }

    public async Task<OptimizationResult> EnsureUltimatePerformancePlanAsync(CancellationToken cancellationToken = default)
    {
        var plans = await GetAvailablePlansAsync(cancellationToken);
        var ultimate = plans.FirstOrDefault(p =>
            p.Name.Contains("Ultimate Performance", StringComparison.OrdinalIgnoreCase) ||
            p.Name.Contains("Ultimative Leistung", StringComparison.OrdinalIgnoreCase) ||
            p.Guid == UltimatePerformanceGuid);

        if (ultimate is not null)
            return await SetActivePlanAsync(ultimate.Guid, cancellationToken);

        var duplicateOutput = await RunPowerCfgAsync($"/duplicatescheme {UltimatePerformanceGuid:D}", cancellationToken);
        var match = DuplicateGuidRegex().Match(duplicateOutput);
        if (!match.Success)
            return OptimizationResult.Fail("Ultimate-Performance-Plan konnte nicht erstellt werden.");

        var newGuid = Guid.Parse(match.Groups[1].Value);
        return await SetActivePlanAsync(newGuid, cancellationToken);
    }

    public async Task<OptimizationResult> RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_previousActiveGuid is null)
            return OptimizationResult.Fail("Kein vorheriger Energieplan gespeichert.");

        return await SetActivePlanAsync(_previousActiveGuid.Value, cancellationToken);
    }

    internal static IReadOnlyList<PowerPlanInfo> ParsePowerPlans(string output)
    {
        var plans = new List<PowerPlanInfo>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var match = PlanLineRegex().Match(line);
            if (!match.Success)
                continue;

            plans.Add(new PowerPlanInfo
            {
                Guid = Guid.Parse(match.Groups[1].Value),
                Name = match.Groups[2].Value.Trim(),
                IsActive = match.Groups[3].Success,
            });
        }

        return plans;
    }

    private static async Task<string> RunPowerCfgAsync(string arguments, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powercfg.exe",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("powercfg.exe konnte nicht gestartet werden.");
        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(stdout) ? stderr : stdout;
    }

    [GeneratedRegex(@"Power Scheme GUID:\s*([0-9a-fA-F-]{36})\s+\((.+?)\)(\s+\*)?", RegexOptions.Compiled)]
    private static partial Regex PlanLineRegex();

    [GeneratedRegex(@"GUID:\s*([0-9a-fA-F-]{36})", RegexOptions.Compiled)]
    private static partial Regex DuplicateGuidRegex();
}
