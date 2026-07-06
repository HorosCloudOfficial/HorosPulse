namespace HorosPulse.Data;

using System.Text.Json;
using System.Text.Json.Serialization;
using HorosPulse.Data.JsonConverters;

public static class JsonDefaults
{
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    public static JsonSerializerOptions CreateOptions() => new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        Converters =
        {
            new SanitizedDoubleJsonConverter(),
            new SanitizedFloatJsonConverter(),
        },
    };

    /// <summary>Replaces non-finite values with 0 for metric capture paths.</summary>
    public static double SanitizeMetric(double value) =>
        double.IsNaN(value) || double.IsInfinity(value) ? 0 : value;
}
