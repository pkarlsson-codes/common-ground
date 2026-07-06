using CommonGround.SharedKernel.Domain;

namespace DddNamespace;

/// <summary>TODO: Describe the DddValueObject value object.</summary>
public sealed class DddValueObject : ValueObject
{
    private DddValueObject(string value)
    {
        Value = value;
    }

    /// <summary>Creates and validates a new <see cref="DddValueObject"/>.</summary>
    public static DddValueObject Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new DddValueObject(value);
    }

    /// <summary>Gets the value.</summary>
    public string Value { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
