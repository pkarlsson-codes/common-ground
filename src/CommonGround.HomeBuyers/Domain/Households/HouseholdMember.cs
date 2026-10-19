using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.Households;

/// <summary>TODO: Describe the HouseholdMember entity.</summary>
public sealed class HouseholdMember : Entity<HouseholdMemberId>
{
    private HouseholdMember(HouseholdMemberId id, string name, string email, string phone, bool isPrimary)
    {
        Id = id;
        Name = name;
        Email = email;
        Phone = phone;
        IsPrimary = isPrimary;
    }

    /// <summary>Household member phone.</summary>
    public string Phone { get; private set; }

    /// <summary>Household member email.</summary>
    public string Email { get; private set; }

    /// <summary>Household member name.</summary>
    public string Name { get; private set; }

    /// <summary>Whether this member is the household's primary point of contact and applicant of record.</summary>
    public bool IsPrimary { get; private set; }

    /// <summary>Creates a new HouseholdMember.</summary>
    public static HouseholdMember Create(string name, string email, string phone, bool isPrimary)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);
        return new HouseholdMember(HouseholdMemberId.Generate(), name, email, phone, isPrimary);
    }
}
