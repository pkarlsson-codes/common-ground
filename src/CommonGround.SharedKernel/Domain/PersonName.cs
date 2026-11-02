using CommonGround.SharedKernel.Domain.Errors;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.SharedKernel.Domain;

/// <summary>A person's full name, as they give it.</summary>
public sealed class PersonName : ValueObject
{
    /// <summary>The longest name accepted.</summary>
    public const int MaxLength = 200;

    private PersonName(string value)
    {
        Value = value;
    }

    /// <summary>Gets the name, trimmed.</summary>
    public string Value { get; }

    /// <summary>Creates and validates a new <see cref="PersonName"/>.</summary>
    /// <param name="value">The name as entered; surrounding whitespace is ignored.</param>
    /// <returns>The name.</returns>
    /// <exception cref="BusinessRuleViolationException">The value is empty or longer than <see cref="MaxLength"/>.</exception>
    public static PersonName Create(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw BusinessRuleViolation.PersonName.Required();
        }

        if (trimmed.Length > MaxLength)
        {
            throw BusinessRuleViolation.PersonName.TooLong(trimmed.Length);
        }

        return new PersonName(trimmed);
    }

    /// <summary>Returns the name.</summary>
    /// <returns>The name.</returns>
    public override string ToString() => Value;

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
