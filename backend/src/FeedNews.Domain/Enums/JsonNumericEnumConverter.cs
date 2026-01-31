using System.Text.Json;
using System.Text.Json.Serialization;

namespace FeedNews.Domain.Enums;

public class JsonNumericEnumConverter : JsonConverter<Enum>
{
    public override Enum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number) throw new JsonException();

        int intValue = reader.GetInt32();
        return (Enum)Enum.ToObject(typeToConvert, intValue);
    }

    public override void Write(Utf8JsonWriter writer, Enum value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(Convert.ToInt32(value));
    }
}