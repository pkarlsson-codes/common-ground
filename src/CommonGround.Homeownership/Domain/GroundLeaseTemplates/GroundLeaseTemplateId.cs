using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeaseTemplates;

/// <summary>
/// Unique identifier for a ground lease template.
/// </summary>
[JsonConverter(typeof(EntityIdJsonConverter<GroundLeaseTemplateId>))]
public readonly record struct GroundLeaseTemplateId : IEntityId<GroundLeaseTemplateId>
{
    private GroundLeaseTemplateId(string value) => Value = value;

    /// <summary>Gets the prefix every value starts with, before a hyphen.</summary>
    public static string Prefix => "groundleasetemplate";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static GroundLeaseTemplateId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static GroundLeaseTemplateId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
