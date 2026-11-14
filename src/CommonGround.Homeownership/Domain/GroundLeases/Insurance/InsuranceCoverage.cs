using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.Insurance;

/// <summary>
/// What an insurance certificate says the homeowner's policies cover: the policy period, the dwelling and liability
/// amounts, and whether the trust is named as additional insured. Also the day the trust received it, which breaks a
/// tie between two certificates ending the same day. The lease checks the rules before recording one.
/// </summary>
public sealed class InsuranceCoverage : ValueObject
{
    private InsuranceCoverage(
        DateOnly policyStart,
        DateOnly policyEnd,
        Money dwellingCoverage,
        Money liabilityPerOccurrence,
        bool trustNamedAdditionalInsured,
        DateOnly receivedOn)
    {
        PolicyStart = policyStart;
        PolicyEnd = policyEnd;
        DwellingCoverage = dwellingCoverage;
        LiabilityPerOccurrence = liabilityPerOccurrence;
        TrustNamedAdditionalInsured = trustNamedAdditionalInsured;
        ReceivedOn = receivedOn;
    }

    /// <summary>Creates coverage from a certificate's recorded facts.</summary>
    /// <param name="policyStart">The first day of the policy period.</param>
    /// <param name="policyEnd">The last day of the policy period.</param>
    /// <param name="dwellingCoverage">The dwelling coverage.</param>
    /// <param name="liabilityPerOccurrence">The liability coverage per occurrence.</param>
    /// <param name="trustNamedAdditionalInsured">Whether the trust is named as additional insured.</param>
    /// <param name="receivedOn">The day the trust received the certificate.</param>
    public static InsuranceCoverage Create(
        DateOnly policyStart,
        DateOnly policyEnd,
        Money dwellingCoverage,
        Money liabilityPerOccurrence,
        bool trustNamedAdditionalInsured,
        DateOnly receivedOn)
    {
        ArgumentNullException.ThrowIfNull(dwellingCoverage);
        ArgumentNullException.ThrowIfNull(liabilityPerOccurrence);

        return new InsuranceCoverage(
            policyStart, policyEnd, dwellingCoverage, liabilityPerOccurrence, trustNamedAdditionalInsured, receivedOn);
    }

    /// <summary>Gets the first day of the policy period.</summary>
    public DateOnly PolicyStart { get; }

    /// <summary>Gets the last day of the policy period.</summary>
    public DateOnly PolicyEnd { get; }

    /// <summary>Gets the dwelling coverage.</summary>
    public Money DwellingCoverage { get; }

    /// <summary>Gets the liability coverage per occurrence.</summary>
    public Money LiabilityPerOccurrence { get; }

    /// <summary>Gets whether the trust is named as additional insured.</summary>
    public bool TrustNamedAdditionalInsured { get; }

    /// <summary>Gets the day the trust received the certificate.</summary>
    public DateOnly ReceivedOn { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PolicyStart;
        yield return PolicyEnd;
        yield return DwellingCoverage;
        yield return LiabilityPerOccurrence;
        yield return TrustNamedAdditionalInsured;
        yield return ReceivedOn;
    }
}
