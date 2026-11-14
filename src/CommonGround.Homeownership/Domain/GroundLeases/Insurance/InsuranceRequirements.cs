using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.Insurance;

/// <summary>
/// The insurance a lease requires the homeowner to carry (Model Lease Art 9.4): the home insured for its full
/// replacement value, and liability insurance per occurrence with the trust named as additional insured. An amount
/// left out isn't checked.
/// </summary>
public sealed class InsuranceRequirements : ValueObject
{
    private InsuranceRequirements(Money? replacementValue, Money? liabilityPerOccurrence)
    {
        ReplacementValue = replacementValue;
        LiabilityPerOccurrence = liabilityPerOccurrence;
    }

    /// <summary>Gets requirements that check neither amount.</summary>
    public static InsuranceRequirements None { get; } = new(null, null);

    /// <summary>Creates requirements, refusing an amount below zero.</summary>
    /// <param name="replacementValue">The home's replacement value the dwelling coverage must reach, or null.</param>
    /// <param name="liabilityPerOccurrence">The liability coverage per occurrence required, or null.</param>
    public static InsuranceRequirements Create(Money? replacementValue, Money? liabilityPerOccurrence)
    {
        if (replacementValue is { IsNegative: true })
            throw BusinessRuleViolation.InsuranceRequirements.Create.ReplacementValueNegative(replacementValue.Amount);

        if (liabilityPerOccurrence is { IsNegative: true })
            throw BusinessRuleViolation.InsuranceRequirements.Create.LiabilityPerOccurrenceNegative(liabilityPerOccurrence.Amount);

        return new InsuranceRequirements(replacementValue, liabilityPerOccurrence);
    }

    /// <summary>Gets the replacement value the dwelling coverage must reach, or null when not checked.</summary>
    public Money? ReplacementValue { get; }

    /// <summary>Gets the liability coverage per occurrence required, or null when not checked.</summary>
    public Money? LiabilityPerOccurrence { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ReplacementValue?.Amount as object ?? string.Empty;
        yield return LiabilityPerOccurrence?.Amount as object ?? string.Empty;
    }
}
