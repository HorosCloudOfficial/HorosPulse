namespace WindowsPerformance.Services.Memory;

using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

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

    public async Task<OptimizationResult> PurgeStandbyListAsync(CancellationToken cancellationToken = default)
    {
        if (!_elevationService.IsHelperAvailable)
            return OptimizationResult.Fail("ElevationHelper nicht verfügbar.");

        var result = await _elevationService.PurgeStandbyListAsync(cancellationToken);
        if (result.Success)
            _logger.LogInformation("Standby-Liste geleert");

        return result;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);

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
}
