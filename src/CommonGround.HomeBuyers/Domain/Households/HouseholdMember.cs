using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.Households;

/// <summary>A person who belongs to a household.</summary>
public sealed class HouseholdMember : Entity<HouseholdMemberId>
{
    private HouseholdMember(HouseholdMemberId id, PersonName name, EmailAddress email, PhoneNumber phone, bool isPrimary)
    {
        Id = id;
        Name = name;
        Email = email;
        Phone = phone;
        IsPrimary = isPrimary;
    }

    /// <summary>Household member phone.</summary>
    public PhoneNumber Phone { get; private set; }

    /// <summary>Household member email.</summary>
    public EmailAddress Email { get; private set; }

    /// <summary>Household member name.</summary>
    public PersonName Name { get; private set; }

    /// <summary>Whether this member is the household's primary point of contact and applicant of record.</summary>
    public bool IsPrimary { get; private set; }

    /// <summary>Creates a new HouseholdMember.</summary>
    public static HouseholdMember Create(PersonName name, EmailAddress email, PhoneNumber phone, bool isPrimary) =>
        Create(HouseholdMemberId.Generate(), name, email, phone, isPrimary);

    /// <summary>Creates a member with an ID chosen by the caller.</summary>
    public static HouseholdMember Create(HouseholdMemberId id, PersonName name, EmailAddress email, PhoneNumber phone, bool isPrimary)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(phone);

        return new HouseholdMember(id, name, email, phone, isPrimary);
    }

    /// <summary>Replaces the member's contact details. Called only when applying a stored event.</summary>
    internal void ChangeContactDetails(PersonName name, EmailAddress email, PhoneNumber phone)
    {
        Name = name;
        Email = email;
        Phone = phone;
    }

    /// <summary>Reconstitutes a <see cref="HouseholdMember"/> from the contact details a stored event recorded.</summary>
    internal static HouseholdMember Reconstitute(HouseholdMemberId id, string name, string email, string phone, bool isPrimary)
        => new(id, PersonName.Create(name), EmailAddress.Create(email), new PhoneNumber(phone), isPrimary);
}
