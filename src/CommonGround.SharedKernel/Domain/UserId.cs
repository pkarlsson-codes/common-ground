using System.Text.Json.Serialization;

namespace CommonGround.SharedKernel.Domain;

/// <summary>
/// Unique identifier for a User.
/// </summary>
[JsonConverter(typeof(EntityIdJsonConverter<UserId>))]
public readonly record struct UserId : IEntityId<UserId>
{
    private UserId(string value) => Value = value;

    // A const, not a public static property: one would stop Marten and Wolverine recognizing this as a strong-typed id.
    private const string Prefix = "user";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static UserId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static UserId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
