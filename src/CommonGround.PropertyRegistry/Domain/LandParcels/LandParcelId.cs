using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.LandParcels;

/// <summary>Strongly-typed identifier for a LandParcel.</summary>
/// <remarks>Initializes a new instance of <see cref="LandParcelId"/>.</remarks>
[JsonConverter(typeof(EntityIdJsonConverter<LandParcelId>))]
public readonly record struct LandParcelId : IEntityId<LandParcelId>
{
    private LandParcelId(string value) => Value = value;

    /// <summary>Gets the prefix every value starts with, before a hyphen.</summary>
    public static string Prefix => "parcel";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static LandParcelId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static LandParcelId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
