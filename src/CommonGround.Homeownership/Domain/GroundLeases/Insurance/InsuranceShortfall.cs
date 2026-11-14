using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.Insurance;

/// <summary>A way an insurance certificate falls short of what the lease requires (Model Lease Art 9.4).</summary>
public sealed class InsuranceShortfall : Enumeration<InsuranceShortfall>
{
    /// <summary>The dwelling coverage is below the home's required replacement value.</summary>
    public static readonly InsuranceShortfall DwellingBelowReplacementValue = new(1, "DwellingBelowReplacementValue");

    /// <summary>The liability coverage per occurrence is below the required amount.</summary>
    public static readonly InsuranceShortfall LiabilityBelowRequired = new(2, "LiabilityBelowRequired");

    /// <summary>The trust isn't named as additional insured.</summary>
    public static readonly InsuranceShortfall TrustNotAdditionalInsured = new(3, "TrustNotAdditionalInsured");

    private InsuranceShortfall(int value, string displayName) : base(value, displayName) { }
}
