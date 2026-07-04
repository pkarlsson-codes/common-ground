using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases;

/// <summary>Value object representing a homeowner's contact information.</summary>
public sealed class Homeowner : ValueObject
{
    /// <summary>Gets the homeowner's full name.</summary>
    public string FullName { get; }

    /// <summary>Gets the homeowner's email address.</summary>
    public string Email { get; }

    /// <summary>Gets the homeowner's phone number, if provided.</summary>
    public PhoneNumber? Phone { get; }

    /// <summary>Creates a new homeowner value object.</summary>
    /// <param name="fullName">Full name (1-200 characters).</param>
    /// <param name="email">Valid email address.</param>
    /// <param name="phone">Optional phone number in any common format.</param>
    public Homeowner(string fullName, string email, string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 200)
        {
            throw BusinessRuleViolation.Homeowner.Create.FullNameInvalid(fullName);
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw BusinessRuleViolation.Homeowner.Create.EmailInvalid(email);
        }

        FullName = fullName;
        Email = email;
        Phone = PhoneNumber.FromOptional(phone);
    }

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
