using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

/// <summary>Unique identifier for a foreclosure prevention case.</summary>
[JsonConverter(typeof(EntityIdJsonConverter<ForeclosurePreventionCaseId>))]
public readonly record struct ForeclosurePreventionCaseId : IEntityId<ForeclosurePreventionCaseId>
{
    private ForeclosurePreventionCaseId(string value) => Value = value;

    // A const, not a public static property: one would stop Marten and Wolverine recognizing this as a strong-typed id.
    private const string Prefix = "foreclosureprevention";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
        /// <returns>The identifier.</returns>
    public static ForeclosurePreventionCaseId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static ForeclosurePreventionCaseId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
