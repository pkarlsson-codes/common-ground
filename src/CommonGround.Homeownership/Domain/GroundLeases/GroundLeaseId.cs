using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases;

/// <summary>
/// Unique identifier for a ground lease.
/// </summary>
[JsonConverter(typeof(EntityIdJsonConverter<GroundLeaseId>))]
public readonly record struct GroundLeaseId : IEntityId<GroundLeaseId>
{
    private GroundLeaseId(string value) => Value = value;

    /// <summary>Gets the prefix every value starts with, before a hyphen.</summary>
    public static string Prefix => "groundlease";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static GroundLeaseId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static GroundLeaseId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
