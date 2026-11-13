using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.MortgageRequests;

/// <summary>Whether a loan's interest rate is fixed for its term or can change.</summary>
public sealed class MortgageRateType : Enumeration<MortgageRateType>
{
    /// <summary>The rate stays the same for the loan's term.</summary>
    public static readonly MortgageRateType Fixed = new(1, "Fixed");

    /// <summary>The rate can change during the loan's term.</summary>
    public static readonly MortgageRateType Adjustable = new(2, "Adjustable");

    private MortgageRateType(int value, string displayName) : base(value, displayName) { }
}
