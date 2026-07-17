using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace iOne.Policy.Policies;

/// <summary>
/// Allows PolicyProductAttributeInputDto.Value to deserialize from string, number, or boolean in JSON
/// to avoid "The JSON value could not be converted to System.String" when the client sends a number (e.g. carYear).
/// </summary>
public class PolicyProductAttributeValueJsonConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.Number:
                if (reader.TryGetInt64(out var l))
                    return l.ToString();
                if (reader.TryGetDouble(out var d))
                    return d.ToString(System.Globalization.CultureInfo.InvariantCulture);
                return reader.GetDecimal().ToString(System.Globalization.CultureInfo.InvariantCulture);
            case JsonTokenType.True:
                return "true";
            case JsonTokenType.False:
                return "false";
            case JsonTokenType.Null:
                return null;
            default:
                throw new JsonException($"Unexpected token type {reader.TokenType} for attribute value.");
        }
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value == null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value);
    }
}
