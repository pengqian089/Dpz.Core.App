using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dpz.Core.App.Models;

public class EnumConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (Enum.TryParse(reader.GetString(), out T value))
        {
            return value;
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
