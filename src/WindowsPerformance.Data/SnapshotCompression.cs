namespace WindowsPerformance.Data;

using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public static class SnapshotCompression
{
    public static string Compress(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            gzip.Write(bytes);
        }

        return Convert.ToBase64String(output.ToArray());
    }

    public static string Decompress(string compressed)
    {
        var bytes = Convert.FromBase64String(compressed);
        using var input = new MemoryStream(bytes);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public static string ComputeChecksum(string payload)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash);
    }

    public static bool ValidateChecksum(string payload, string checksum) =>
        string.Equals(ComputeChecksum(payload), checksum, StringComparison.OrdinalIgnoreCase);

    public static string SerializeState<T>(T state) =>
        JsonSerializer.Serialize(state, JsonDefaults.Options);
}
