using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommonGround.SharedKernel.Domain;

/// <summary>
/// Reads and writes any <see cref="Enumeration{T}"/> as its display name, so an event or document
/// carrying one stores a readable string and reads back to the same instance. Register it once on a
/// serializer's options; the enumerations themselves need no attributes.
/// </summary>
/// <remarks>
/// The display name is the stored key, so renaming one breaks the events that hold the old name.
/// Keep display names stable; when one has to change, keep accepting the old name on read.
/// </remarks>
public sealed class EnumerationJsonConverter : JsonConverterFactory
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        return !typeToConvert.IsAbstract && EnumerationBase(typeToConvert) is not null;
    }

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter)Activator.CreateInstance(typeof(Converter<>).MakeGenericType(typeToConvert))!;

    private static Type? EnumerationBase(Type type)
    {
        for (var t = type.BaseType; t is not null; t = t.BaseType)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Enumeration<>))
            {
                return t;
            }
        }

        return null;
    }

    private sealed class Converter<T> : JsonConverter<T>
        where T : Enumeration<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var displayName = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
            return displayName is not null && Enumeration<T>.TryFromDisplayName(displayName, out var result)
                ? result
                : throw new JsonException($"'{displayName}' is not a {typeof(T).Name}.");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.DisplayName);
    }
}
