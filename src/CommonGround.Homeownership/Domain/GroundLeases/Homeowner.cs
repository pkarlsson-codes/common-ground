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

    /// <summary>Gets the homeowner's phone number in E.164 format, if provided.</summary>
    public string? Phone { get; }

    /// <summary>Creates a new homeowner value object.</summary>
    /// <param name="fullName">Full name (1-200 characters).</param>
    /// <param name="email">Valid email address.</param>
    /// <param name="phone">Optional phone in E.164 format.</param>
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

        if (phone is not null
            && !System.Text.RegularExpressions.Regex.IsMatch(
                phone, @"^\+[1-9]\d{1,14}$"))
        {
            throw BusinessRuleViolation.Homeowner.Create.PhoneInvalidFormat(phone);
        }

        FullName = fullName;
        Email = email;
        Phone = phone;
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
