using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace DddNamespace;

/// <summary>Strongly-typed identifier for a <see cref="DddEntity"/>.</summary>
/// <remarks>Initializes a new instance of <see cref="DddEntityId"/>.</remarks>
[JsonConverter(typeof(EntityIdJsonConverter<DddEntityId>))]
public readonly record struct DddEntityId : IEntityId<DddEntityId>
{
    private DddEntityId(string value) => Value = value;

    // A const, not a public static property: one would stop Marten and Wolverine recognizing this as a strong-typed id.
    private const string Prefix = "dddprefix";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static DddEntityId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static DddEntityId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
