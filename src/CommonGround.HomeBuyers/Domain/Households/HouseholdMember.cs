using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.Households;

/// <summary>A person who belongs to a household.</summary>
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
    public static HouseholdMember Create(string name, string email, string phone, bool isPrimary) =>
        Create(HouseholdMemberId.Generate(), name, email, phone, isPrimary);

    /// <summary>Creates a member with an ID chosen by the caller, validating contact details.</summary>
    public static HouseholdMember Create(HouseholdMemberId id, string name, string email, string phone, bool isPrimary)
    {
        ValidateContactDetails(name, email, phone);
        return new HouseholdMember(id, name, email, phone, isPrimary);
    }

    /// <summary>Validates contact details for a household member.</summary>
    internal static void ValidateContactDetails(string name, string email, string phone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);
    }

    /// <summary>Replaces the member's contact details. Called only when applying a stored event.</summary>
    internal void ChangeContactDetails(string name, string email, string phone)
    {
        Name = name;
        Email = email;
        Phone = phone;
    }

    /// <summary>Reconstitutes a <see cref="HouseholdMember"/> from a stored event without revalidating.</summary>
    internal static HouseholdMember Reconstitute(HouseholdMemberId id, string name, string email, string phone, bool isPrimary)
        => new(id, name, email, phone, isPrimary);
}
