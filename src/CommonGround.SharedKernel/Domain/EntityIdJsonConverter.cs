using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommonGround.SharedKernel.Domain;

/// <summary>
/// Reads and writes an <see cref="IEntityId{TSelf}"/> as its plain string value, so a message or
/// document carrying one round-trips (for example a scheduled command in Wolverine's durable store).
/// </summary>
/// <typeparam name="TId">The identifier type.</typeparam>
public sealed class EntityIdJsonConverter<TId> : JsonConverter<TId>
    where TId : struct, IEntityId<TId>
{
    /// <inheritdoc/>
    public override TId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        TId.Create(reader.GetString() ?? throw new JsonException($"{typeof(TId).Name} cannot be null."));

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(value.Value);
    }
}
