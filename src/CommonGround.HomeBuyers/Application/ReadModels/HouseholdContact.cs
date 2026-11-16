namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>Contact details for a household's primary member.</summary>
public sealed record HouseholdContact(string Name, string Email, string Phone)
{
    public static HouseholdContact From(HouseholdListItemView household)
    {
        ArgumentNullException.ThrowIfNull(household);
        return new(household.PrimaryMemberName, household.PrimaryMemberEmail, household.PrimaryMemberPhone);
    }
}
