using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartLock.DBApi.Utils
{
    public class NullableDateTimeJsonConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s))
                    return null;

                if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var parsed))
                {
                    return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
                }

                if (long.TryParse(s, out var epoch))
                {
                    return epoch >= 1_000_000_000_000
                        ? DateTimeOffset.FromUnixTimeMilliseconds(epoch).UtcDateTime
                        : DateTimeOffset.FromUnixTimeSeconds(epoch).UtcDateTime;
                }

                throw new JsonException($"Invalid DateTime format: {s}");
            }

            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out long value))
            {
                return value >= 1_000_000_000_000
                    ? DateTimeOffset.FromUnixTimeMilliseconds(value).UtcDateTime
                    : DateTimeOffset.FromUnixTimeSeconds(value).UtcDateTime;
            }

            throw new JsonException($"Unexpected token parsing DateTime. TokenType: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToUniversalTime().ToString("o"));
            else
                writer.WriteNullValue();
        }
    }
}