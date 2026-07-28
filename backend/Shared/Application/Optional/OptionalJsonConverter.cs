using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Conduit.Shared.Application.Optional;

public class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
    public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Only invoked when the key is present in the JSON (even if its value is null).
        if (reader.TokenType == JsonTokenType.Null)
        {
            if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) is null)
            {
                throw new JsonException($"Cannot assign null to non-nullable type '{typeof(T).Name}'.");
            }

            return new Optional<T>(default!);
        }

        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return new Optional<T>(value!);
    }

    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
    {
        if (value.Value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            JsonSerializer.Serialize(writer, value.Value, options);
        }
    }
}
