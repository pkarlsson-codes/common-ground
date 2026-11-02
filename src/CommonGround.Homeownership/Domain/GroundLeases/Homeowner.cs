using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases;

/// <summary>Value object representing a homeowner's contact information.</summary>
public sealed class Homeowner : ValueObject
{
    /// <summary>Gets the homeowner's full name.</summary>
    public PersonName FullName { get; }

    /// <summary>Gets the homeowner's email address.</summary>
    public EmailAddress Email { get; }

    /// <summary>Gets the homeowner's phone number, if provided.</summary>
    public PhoneNumber? Phone { get; }

    /// <summary>Creates a new homeowner value object.</summary>
    /// <param name="fullName">Full name.</param>
    /// <param name="email">Email address.</param>
    /// <param name="phone">Phone number, if provided.</param>
    public Homeowner(PersonName fullName, EmailAddress email, PhoneNumber? phone = null)
    {
        ArgumentNullException.ThrowIfNull(fullName);
        ArgumentNullException.ThrowIfNull(email);

        FullName = fullName;
        Email = email;
        Phone = phone;
    }

    /// <summary>Creates a homeowner from contact details as recorded or entered.</summary>
    /// <param name="fullName">Full name; see <see cref="PersonName"/>.</param>
    /// <param name="email">Email address; see <see cref="EmailAddress"/>.</param>
    /// <param name="phone">Phone number in any common format, if provided.</param>
    /// <returns>The homeowner.</returns>
    /// <exception cref="SharedKernel.Domain.Exceptions.BusinessRuleViolationException">A detail is missing or invalid.</exception>
    public static Homeowner From(string? fullName, string? email, string? phone = null) =>
        new(PersonName.Create(fullName), EmailAddress.Create(email), PhoneNumber.FromOptional(phone));

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FullName;
        yield return Email;
        if (Phone is not null)
        {
            yield return Phone;
        }
    }
}
