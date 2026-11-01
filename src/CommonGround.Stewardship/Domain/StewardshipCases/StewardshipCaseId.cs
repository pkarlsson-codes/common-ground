using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Stewardship.Domain.StewardshipCases;

/// <summary>Unique identifier for a stewardship case.</summary>
[JsonConverter(typeof(EntityIdJsonConverter<StewardshipCaseId>))]
public readonly record struct StewardshipCaseId : IEntityId<StewardshipCaseId>
{
    private StewardshipCaseId(string value) => Value = value;

    // A const, not a public static property: one would stop Marten and Wolverine recognizing this as a strong-typed id.
    private const string Prefix = "stewardshipcase";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
        /// <returns>The identifier.</returns>
    public static StewardshipCaseId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static StewardshipCaseId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
