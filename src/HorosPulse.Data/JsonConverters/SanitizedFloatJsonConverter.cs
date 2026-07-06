namespace HorosPulse.Data.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Writes non-finite floats as JSON null; reads null back as <see cref="float.NaN"/>.
/// </summary>
public sealed class SanitizedFloatJsonConverter : JsonConverter<float>
{
    public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.Null => float.NaN,
            JsonTokenType.String => ParseString(reader.GetString()),
            _ => reader.GetSingle(),
        };

    public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            writer.WriteNullValue();
        else
            writer.WriteNumberValue(value);
    }

    private static float ParseString(string? text) =>
        text switch
        {
            null => float.NaN,
            "NaN" => float.NaN,
            "Infinity" or "+Infinity" => float.PositiveInfinity,
            "-Infinity" => float.NegativeInfinity,
            _ => float.Parse(text, System.Globalization.CultureInfo.InvariantCulture),
        };
}
