using System.Text.Json.Serialization;

namespace CommonGround.SharedKernel.Domain;

/// <summary>Represents the unique identifier for a trust entity.</summary>
[JsonConverter(typeof(EntityIdJsonConverter<TrustId>))]
public readonly record struct TrustId : IEntityId<TrustId>
{
    private TrustId(string value) => Value = value;

    // A const, not a public static property: one would stop Marten and Wolverine recognizing this as a strong-typed id.
    private const string Prefix = "trust";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static TrustId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static TrustId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
