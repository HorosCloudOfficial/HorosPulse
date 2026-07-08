namespace HorosPulse.Services.DevDrive;

using System.Runtime.InteropServices;
using System.Text;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;

public sealed class DevDriveVolumeProbe : IDevDriveVolumeProbe
{
    private const uint FileDeviceFileSystem = 0x00000009;
    private const uint MethodBuffered = 0;
    private const uint FileAnyAccess = 0;
    private const uint FsctlQueryPersistentVolumeState =
        (FileDeviceFileSystem << 16) | (FileAnyAccess << 14) | (89u << 2) | MethodBuffered;

    private const uint PersistentVolumeStateDevVolume = 0x00002000;

    private const uint GenericRead = 0x80000000;
    private const uint FileShareRead = 0x00000001;
    private const uint FileShareWrite = 0x00000002;
    private const uint OpenExisting = 3;

    private readonly ILogger<DevDriveVolumeProbe> _logger;

    public DevDriveVolumeProbe(ILogger<DevDriveVolumeProbe> logger) => _logger = logger;

    public IReadOnlyList<DevDriveVolumeInfo> GetVolumes()
    {
        var volumes = new List<DevDriveVolumeInfo>();

        foreach (var drive in DriveInfo.GetDrives())
        {
            if (drive.DriveType is not DriveType.Fixed)
                continue;

            if (string.IsNullOrWhiteSpace(drive.Name))
                continue;

            try
            {
                if (!drive.IsReady)
                    continue;

                var root = NormalizeRoot(drive.Name);
                var (fileSystem, label) = QueryVolumeMetadata(root);
                var isReFs = string.Equals(fileSystem, "ReFS", StringComparison.OrdinalIgnoreCase);
                var isDevDrive = isReFs && QueryIsDevDrive(root);

                volumes.Add(new DevDriveVolumeInfo
                {
                    DriveLetter = root.TrimEnd('\\'),
                    VolumeLabel = label,
                    FileSystem = fileSystem ?? "—",
                    IsReFs = isReFs,
                    IsDevDrive = isDevDrive,
                    FreeBytes = drive.AvailableFreeSpace,
                    TotalBytes = drive.TotalSize,
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Volume-Abfrage fehlgeschlagen für {Drive}", drive.Name);
            }
        }

        return volumes;
    }

    public DevDriveVolumeInfo? GetVolumeForPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var root = Path.GetPathRoot(path);
        if (string.IsNullOrWhiteSpace(root))
            return null;

        var normalized = NormalizeRoot(root);
        return GetVolumes().FirstOrDefault(v =>
            string.Equals(v.DriveLetter, normalized.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase));
    }

    internal static string NormalizeRoot(string root) =>
        root.EndsWith('\\') ? root : root + '\\';

    internal static bool IsDevDriveFlagSet(uint volumeFlags) =>
        (volumeFlags & PersistentVolumeStateDevVolume) != 0;

    private bool QueryIsDevDrive(string rootPath)
    {
        var devicePath = @"\\.\" + rootPath.TrimEnd('\\');
        using var handle = CreateFile(
            devicePath,
            GenericRead,
            FileShareRead | FileShareWrite,
            IntPtr.Zero,
            OpenExisting,
            0,
            IntPtr.Zero);

        if (handle.IsInvalid)
        {
            _logger.LogDebug("CreateFile fehlgeschlagen für {Path}: Win32={Error}", devicePath, Marshal.GetLastWin32Error());
            return false;
        }

        var info = new FileFsPersistentVolumeInformation
        {
            Version = 1,
            FlagMask = PersistentVolumeStateDevVolume,
        };

        var size = Marshal.SizeOf<FileFsPersistentVolumeInformation>();
        var inPtr = Marshal.AllocHGlobal(size);
        var outPtr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(info, inPtr, false);
            if (!DeviceIoControl(
                    handle,
                    FsctlQueryPersistentVolumeState,
                    inPtr,
                    (uint)size,
                    outPtr,
                    (uint)size,
                    out _,
                    IntPtr.Zero))
            {
                _logger.LogDebug(
                    "FSCTL_QUERY_PERSISTENT_VOLUME_STATE fehlgeschlagen für {Path}: Win32={Error}",
                    rootPath,
                    Marshal.GetLastWin32Error());
                return false;
            }

            var result = Marshal.PtrToStructure<FileFsPersistentVolumeInformation>(outPtr);
            return IsDevDriveFlagSet(result.VolumeFlags);
        }
        finally
        {
            Marshal.FreeHGlobal(inPtr);
            Marshal.FreeHGlobal(outPtr);
        }
    }

    private static (string? FileSystem, string? Label) QueryVolumeMetadata(string rootPath)
    {
        var labelBuilder = new StringBuilder(256);
        var fsBuilder = new StringBuilder(256);

        if (!GetVolumeInformation(
                rootPath,
                labelBuilder,
                labelBuilder.Capacity,
                out _,
                out _,
                out _,
                fsBuilder,
                fsBuilder.Capacity))
        {
            return (null, null);
        }

        return (fsBuilder.ToString(), labelBuilder.ToString());
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileFsPersistentVolumeInformation
    {
        public uint VolumeFlags;
        public uint FlagMask;
        public uint Version;
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern SafeFileHandle CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool DeviceIoControl(
        SafeFileHandle hDevice,
        uint dwIoControlCode,
        IntPtr lpInBuffer,
        uint nInBufferSize,
        IntPtr lpOutBuffer,
        uint nOutBufferSize,
        out uint lpBytesReturned,
        IntPtr lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool GetVolumeInformation(
        string lpRootPathName,
        StringBuilder lpVolumeNameBuffer,
        int nVolumeNameSize,
        out uint lpVolumeSerialNumber,
        out uint lpMaximumComponentLength,
        out uint lpFileSystemFlags,
        StringBuilder lpFileSystemNameBuffer,
        int nFileSystemNameSize);
}
