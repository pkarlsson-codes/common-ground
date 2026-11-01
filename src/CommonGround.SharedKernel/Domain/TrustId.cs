using System.Text.Json.Serialization;

namespace CommonGround.SharedKernel.Domain;

/// <summary>Represents the unique identifier for a trust entity.</summary>
[JsonConverter(typeof(EntityIdJsonConverter<TrustId>))]
public readonly record struct TrustId : IEntityId<TrustId>
{
    private TrustId(string value) => Value = value;

    /// <summary>Gets the prefix every value starts with, before a hyphen.</summary>
    public static string Prefix => "trust";

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
