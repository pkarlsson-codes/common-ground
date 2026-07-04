using CommonGround.SharedKernel.Domain.Errors;
using CommonGround.SharedKernel.Domain.Exceptions;
using PhoneNumbers;

namespace CommonGround.SharedKernel.Domain;

/// <summary>
/// Phone number value object. Parses any common input format (local numbers default to the
/// US region) via libphonenumber and stores the canonical E.164 representation.
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    private const string DefaultRegion = "US";

    /// <summary>
    /// Initializes a new instance of the PhoneNumber class by parsing the input.
    /// </summary>
    /// <param name="input">A phone number in any common format, e.g. "(202) 555-1234" or "+12025551234".</param>
    /// <exception cref="BusinessRuleViolationException">The input is not a valid phone number.</exception>
    public PhoneNumber(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var util = PhoneNumberUtil.GetInstance();
        PhoneNumbers.PhoneNumber parsed;
        try
        {
            parsed = util.Parse(input, DefaultRegion);
        }
        catch (NumberParseException)
        {
            throw BusinessRuleViolation.Phone.InvalidFormat(input);
        }

        if (!util.IsValidNumber(parsed))
        {
            throw BusinessRuleViolation.Phone.InvalidFormat(input);
        }

        Value = util.Format(parsed, PhoneNumberFormat.E164);
    }

    /// <summary>
    /// Gets the canonical E.164 representation, e.g. "+12025551234".
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Parses optional input into a PhoneNumber, returning null for null or whitespace input.
    /// </summary>
    /// <param name="input">A phone number in any common format, or null.</param>
    /// <returns>The parsed phone number, or null when no input was provided.</returns>
    /// <exception cref="BusinessRuleViolationException">The input is not a valid phone number.</exception>
    public static PhoneNumber? FromOptional(string? input)
        => string.IsNullOrWhiteSpace(input) ? null : new PhoneNumber(input);

    /// <summary>
    /// Returns the canonical E.164 representation.
    /// </summary>
    /// <returns>The E.164 phone number string.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Gets the equality components for this value object.
    /// </summary>
    /// <returns>The equality components.</returns>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
