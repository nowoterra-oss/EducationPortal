using System.Text.Json;
using System.Text.Json.Serialization;

namespace EduPortal.API.Converters;

/// <summary>
/// Custom JSON converter for DayOfWeek that supports both standard (Sunday=0)
/// and ISO (Monday=1, Sunday=7) formats.
/// </summary>
public class DayOfWeekConverter : JsonConverter<DayOfWeek>
{
    public override DayOfWeek Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var value = reader.GetInt32();

            // ISO format: Sunday = 7, convert to 0
            if (value == 7)
                return DayOfWeek.Sunday;

            // Standard format: 0-6
            if (value >= 0 && value <= 6)
                return (DayOfWeek)value;

            throw new JsonException($"Geçersiz gün değeri: {value}. Geçerli değerler: 0-7 (0 veya 7 = Pazar)");
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();

            // Try parsing as enum name
            if (Enum.TryParse<DayOfWeek>(value, ignoreCase: true, out var day))
                return day;

            // Try Turkish day names
            return value?.ToLowerInvariant() switch
            {
                "pazar" => DayOfWeek.Sunday,
                "pazartesi" => DayOfWeek.Monday,
                "salı" or "sali" => DayOfWeek.Tuesday,
                "çarşamba" or "carsamba" => DayOfWeek.Wednesday,
                "perşembe" or "persembe" => DayOfWeek.Thursday,
                "cuma" => DayOfWeek.Friday,
                "cumartesi" => DayOfWeek.Saturday,
                _ => throw new JsonException($"Geçersiz gün adı: {value}")
            };
        }

        throw new JsonException($"Beklenmeyen JSON token tipi: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, DayOfWeek value, JsonSerializerOptions options)
    {
        // Always write as string for clarity
        writer.WriteStringValue(value.ToString());
    }
}
