namespace HorosPulse.Data.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Writes non-finite doubles as JSON null; reads null back as <see cref="double.NaN"/>.
/// </summary>
public sealed class SanitizedDoubleJsonConverter : JsonConverter<double>
{
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.Null => double.NaN,
            JsonTokenType.String => ParseString(reader.GetString()),
            _ => reader.GetDouble(),
        };

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            writer.WriteNullValue();
        else
            writer.WriteNumberValue(value);
    }

    private static double ParseString(string? text) =>
        text switch
        {
            null => double.NaN,
            "NaN" => double.NaN,
            "Infinity" or "+Infinity" => double.PositiveInfinity,
            "-Infinity" => double.NegativeInfinity,
            _ => double.Parse(text, System.Globalization.CultureInfo.InvariantCulture),
        };
}
