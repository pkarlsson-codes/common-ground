using CommonGround.SharedKernel.Domain.Errors;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.SharedKernel.Domain;

/// <summary>
/// An email address, checked only for its shape: one <c>@</c> with something on each side and no
/// whitespace. Whether it reaches anyone is only known by sending to it.
/// </summary>
public sealed class EmailAddress : ValueObject
{
    /// <summary>The longest address SMTP can deliver to (RFC 5321).</summary>
    public const int MaxLength = 254;

    private EmailAddress(string value)
    {
        Value = value;
    }

    /// <summary>Gets the address, trimmed.</summary>
    public string Value { get; }

    /// <summary>Creates and validates a new <see cref="EmailAddress"/>.</summary>
    /// <param name="value">The address as entered; surrounding whitespace is ignored.</param>
    /// <returns>The email address.</returns>
    /// <exception cref="BusinessRuleViolationException">The value is empty or isn't shaped like an address.</exception>
    public static EmailAddress Create(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw BusinessRuleViolation.Email.Required();
        }

        var at = trimmed.IndexOf('@', StringComparison.Ordinal);
        var shaped = trimmed.Length <= MaxLength
            && at > 0
            && at < trimmed.Length - 1
            && at == trimmed.LastIndexOf('@')
            && !trimmed.Any(char.IsWhiteSpace);
        if (!shaped)
        {
            throw BusinessRuleViolation.Email.InvalidFormat(trimmed);
        }

        return new EmailAddress(trimmed);
    }

    /// <summary>Returns the address.</summary>
    /// <returns>The address.</returns>
    public override string ToString() => Value;

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
