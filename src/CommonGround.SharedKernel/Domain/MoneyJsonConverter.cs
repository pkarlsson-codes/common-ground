using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommonGround.SharedKernel.Domain;

/// <summary>Reads and writes <see cref="Money"/> as a plain JSON number of dollars.</summary>
public sealed class MoneyJsonConverter : JsonConverter<Money>
{
    /// <inheritdoc/>
    public override Money Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.Number
            ? Money.Create(reader.GetDecimal())
            : throw new JsonException($"Expected a number for {nameof(Money)}, got {reader.TokenType}.");

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Money value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        writer.WriteNumberValue(value.Amount);
    }
}
