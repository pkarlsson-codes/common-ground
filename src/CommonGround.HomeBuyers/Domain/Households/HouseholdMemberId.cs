using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.Households;

/// <summary>Strongly-typed identifier for a <see cref="HouseholdMember"/>.</summary>
[JsonConverter(typeof(EntityIdJsonConverter<HouseholdMemberId>))]
public readonly record struct HouseholdMemberId : IEntityId<HouseholdMemberId>
{
    private HouseholdMemberId(string value) => Value = value;

    /// <summary>Gets the prefix every value starts with, before a hyphen.</summary>
    public static string Prefix => "household-member";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static HouseholdMemberId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static HouseholdMemberId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
