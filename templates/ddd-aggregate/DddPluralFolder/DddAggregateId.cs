using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace DddNamespace.DddPluralFolder;

/// <summary>Strongly-typed identifier for a <see cref="DddAggregate"/>.</summary>
/// <remarks>Initializes a new instance of <see cref="DddAggregateId"/>.</remarks>
[JsonConverter(typeof(EntityIdJsonConverter<DddAggregateId>))]
public readonly record struct DddAggregateId : IEntityId<DddAggregateId>
{
    private DddAggregateId(string value) => Value = value;

    // A const, not a public static property: one would stop Marten and Wolverine recognizing this as a strong-typed id.
    private const string Prefix = "dddprefix";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static DddAggregateId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static DddAggregateId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
