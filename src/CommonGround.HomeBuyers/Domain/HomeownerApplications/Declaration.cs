using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications;

/// <summary>Household characteristics used for eligibility assessment and home matching.</summary>
public sealed class Declaration(int householdSize, HouseholdCircumstances circumstances) : ValueObject
{
    public int HouseholdSize { get; init; } = householdSize;

    public HouseholdCircumstances Circumstances { get; init; } = circumstances;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return HouseholdSize;
        yield return Circumstances;
    }
}