using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.Households;

/// <summary>Strongly-typed identifier for a <see cref="HouseholdMember"/>.</summary>
public sealed class HouseholdMemberId(string value)
: EntityId<HouseholdMemberId>(value, Prefix)
{
    private const string Prefix = "household-member";
}
