using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.Insurance;

/// <summary>
/// How a certificate's coverage compares with the insurance the lease requires: each shortfall, and which amounts
/// weren't checked because the lease sets none. An amount not checked isn't a shortfall.
/// </summary>
public sealed class InsuranceCheck : ValueObject
{
    private readonly InsuranceShortfall[] _shortfalls;

    private InsuranceCheck(InsuranceRequirements requirements, InsuranceShortfall[] shortfalls)
    {
        Requirements = requirements;
        _shortfalls = shortfalls;
    }

    /// <summary>Checks coverage against requirements.</summary>
    /// <param name="coverage">The certificate's coverage.</param>
    /// <param name="requirements">The requirements in effect on the policy start.</param>
    public static InsuranceCheck Of(InsuranceCoverage coverage, InsuranceRequirements requirements)
    {
        ArgumentNullException.ThrowIfNull(coverage);
        ArgumentNullException.ThrowIfNull(requirements);

        var shortfalls = new List<InsuranceShortfall>();
        if (requirements.ReplacementValue is { } replacementValue && coverage.DwellingCoverage < replacementValue)
            shortfalls.Add(InsuranceShortfall.DwellingBelowReplacementValue);
        if (requirements.LiabilityPerOccurrence is { } liability && coverage.LiabilityPerOccurrence < liability)
            shortfalls.Add(InsuranceShortfall.LiabilityBelowRequired);
        if (!coverage.TrustNamedAdditionalInsured)
            shortfalls.Add(InsuranceShortfall.TrustNotAdditionalInsured);

        return new InsuranceCheck(requirements, [.. shortfalls]);
    }

    /// <summary>Gets the requirements the coverage was checked against.</summary>
    public InsuranceRequirements Requirements { get; }

    /// <summary>Gets each way the coverage falls short, in the order dwelling, liability, additional insured.</summary>
    public IReadOnlyList<InsuranceShortfall> Shortfalls => _shortfalls;

    /// <summary>Gets whether the coverage meets every requirement that is set.</summary>
    public bool MeetsLease => _shortfalls.Length == 0;

    /// <summary>Gets whether the dwelling coverage was checked, which needs a replacement value on the lease.</summary>
    public bool ReplacementValueChecked => Requirements.ReplacementValue is not null;

    /// <summary>Gets whether the liability coverage was checked, which needs a required amount on the lease.</summary>
    public bool LiabilityChecked => Requirements.LiabilityPerOccurrence is not null;

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Requirements;
        foreach (var shortfall in _shortfalls)
            yield return shortfall;
    }
}
