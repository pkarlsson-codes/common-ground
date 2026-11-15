using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.HeirTransfers;

/// <summary>Strongly-typed identifier for a <see cref="HeirTransfer"/>.</summary>
/// <remarks>Initializes a new instance of <see cref="HeirTransferId"/>.</remarks>
[JsonConverter(typeof(EntityIdJsonConverter<HeirTransferId>))]
public readonly record struct HeirTransferId : IEntityId<HeirTransferId>
{
    private HeirTransferId(string value) => Value = value;

    // A const, not a public static property: one would stop Marten and Wolverine recognizing this as a strong-typed id.
    private const string Prefix = "heirtransfer";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static HeirTransferId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static HeirTransferId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
