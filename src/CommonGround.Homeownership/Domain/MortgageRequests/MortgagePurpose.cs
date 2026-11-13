using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.MortgageRequests;

/// <summary>What a homeowner wants a new loan on the home for (Model Lease Art 8.3).</summary>
public sealed class MortgagePurpose : Enumeration<MortgagePurpose>
{
    /// <summary>A new loan that pays off one or more current mortgages.</summary>
    public static readonly MortgagePurpose Refinance = new(1, "Refinance");

    /// <summary>A further loan next to the current mortgages.</summary>
    public static readonly MortgagePurpose SecondMortgage = new(2, "SecondMortgage");

    /// <summary>A loan for repairs or improvements to the home.</summary>
    public static readonly MortgagePurpose HomeRepair = new(3, "HomeRepair");

    /// <summary>Any other loan secured on the home.</summary>
    public static readonly MortgagePurpose Other = new(4, "Other");

    private MortgagePurpose(int value, string displayName) : base(value, displayName) { }
}
