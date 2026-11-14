using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.Insurance;

/// <summary>The insurance a lease requires from a day on, until a later change takes effect.</summary>
public sealed class InsuranceRequirementsChange : ValueObject
{
    private InsuranceRequirementsChange(DateOnly effectiveFrom, InsuranceRequirements requirements)
    {
        EffectiveFrom = effectiveFrom;
        Requirements = requirements;
    }

    /// <summary>Creates a change.</summary>
    /// <param name="effectiveFrom">The first day the requirements apply.</param>
    /// <param name="requirements">The requirements from that day.</param>
    public static InsuranceRequirementsChange Create(DateOnly effectiveFrom, InsuranceRequirements requirements)
    {
        ArgumentNullException.ThrowIfNull(requirements);

        return new InsuranceRequirementsChange(effectiveFrom, requirements);
    }

    /// <summary>Gets the first day the requirements apply.</summary>
    public DateOnly EffectiveFrom { get; }

    /// <summary>Gets the requirements from that day.</summary>
    public InsuranceRequirements Requirements { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return EffectiveFrom;
        yield return Requirements;
    }
}
