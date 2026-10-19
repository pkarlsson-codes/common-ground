using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.Households;

/// <summary>Strongly-typed identifier for a household.</summary>
public sealed class HouseholdId(string value)
    : EntityId<HouseholdId>(value, Prefix)
{
    private const string Prefix = "household";
}