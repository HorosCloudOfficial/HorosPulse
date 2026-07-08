namespace HorosPulse.Services.Memory;

using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed class MemoryOptimizerService : IMemoryOptimizerService
{
    private readonly IElevationService _elevationService;
    private readonly ILogger<MemoryOptimizerService> _logger;

    public MemoryOptimizerService(IElevationService elevationService, ILogger<MemoryOptimizerService> logger)
    {
        _elevationService = elevationService;
        _logger = logger;
    }

    public Task<long> GetAvailableMemoryMbAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var mem = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (GlobalMemoryStatusEx(ref mem))
            return Task.FromResult((long)(mem.AvailPhys / 1024 / 1024));

        return Task.FromResult(0L);
    }

    public Task<MemoryStatusSnapshot> GetMemoryStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var mem = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (!GlobalMemoryStatusEx(ref mem))
            return Task.FromResult(MemoryStatusSnapshot.Empty);

        var physicalTotalMb = (long)(mem.TotalPhys / 1024 / 1024);
        var physicalAvailableMb = (long)(mem.AvailPhys / 1024 / 1024);
        var pageFileTotalMb = (long)(mem.TotalPageFile / 1024 / 1024);
        var pageFileAvailableMb = (long)(mem.AvailPageFile / 1024 / 1024);

        var (systemReservedTotalMb, systemReservedAvailableMb) = GetSystemReservedMb(mem);

        return Task.FromResult(new MemoryStatusSnapshot(
            physicalTotalMb,
            physicalAvailableMb,
            pageFileTotalMb,
            pageFileAvailableMb,
            systemReservedTotalMb,
            systemReservedAvailableMb));
    }

    public async Task<OptimizationResult> PurgeMemoryAsync(
        MemoryPurgeOptions options,
        CancellationToken cancellationToken = default)
    {
        if (!_elevationService.IsHelperAvailable)
            return OptimizationResult.Fail("HorosPulse.Elevation.exe nicht verfügbar.");

        if (options.GetEnabledAreas().Count == 0)
            return OptimizationResult.Fail("Keine Speicherbereiche ausgewählt.");

        var result = await _elevationService.PurgeMemoryAsync(options, cancellationToken);
        if (result.Success)
            _logger.LogInformation("Speicherbereiche geleert: {Areas}", string.Join(", ", options.GetEnabledAreas()));

        return result;
    }

    public Task<OptimizationResult> PurgeStandbyListAsync(CancellationToken cancellationToken = default) =>
        PurgeMemoryAsync(new MemoryPurgeOptions { PurgeStandbyList = true }, cancellationToken);

    private static (long TotalMb, long AvailableMb) GetSystemReservedMb(MemoryStatusEx mem)
    {
        var perf = new PerformanceInformation
        {
            Size = (uint)Marshal.SizeOf<PerformanceInformation>(),
        };

        if (!GetPerformanceInfo(ref perf, perf.Size) || perf.PageSize == 0)
            return (0, 0);

        var pageSize = (ulong)perf.PageSize;
        var systemPages = (ulong)perf.SystemCache + perf.KernelPaged + perf.KernelNonPaged;
        var systemReservedMb = (long)(systemPages * pageSize / 1024 / 1024);
        if (systemReservedMb <= 0)
            return (0, 0);

        var physicalUsedMb = (long)((mem.TotalPhys - mem.AvailPhys) / 1024 / 1024);
        var systemUsedMb = Math.Min(systemReservedMb, Math.Max(0, physicalUsedMb));
        var systemAvailableMb = Math.Max(0, systemReservedMb - systemUsedMb);

        return (systemReservedMb, systemAvailableMb);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);

    [DllImport("psapi.dll", SetLastError = true)]
    private static extern bool GetPerformanceInfo(ref PerformanceInformation pPerformanceInformation, uint cb);

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PerformanceInformation
    {
        public uint Size;
        public uint CommitTotal;
        public uint CommitLimit;
        public uint CommitPeak;
        public uint PhysicalTotal;
        public uint PhysicalAvailable;
        public uint SystemCache;
        public uint KernelTotal;
        public uint KernelPaged;
        public uint KernelNonPaged;
        public uint PageSize;
        public uint HandleCount;
        public uint ProcessCount;
        public uint ThreadCount;
    }
}
