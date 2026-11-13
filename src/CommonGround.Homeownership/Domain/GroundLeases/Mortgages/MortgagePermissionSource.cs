using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.Mortgages;

/// <summary>How the trust permitted a mortgage on the home (Model Lease Art 8.1 to 8.3).</summary>
public sealed class MortgagePermissionSource : Enumeration<MortgagePermissionSource>
{
    /// <summary>The purchase loan, permitted by signing the lease (Art 8.2).</summary>
    public static readonly MortgagePermissionSource LeaseSigning = new(1, "LeaseSigning");

    /// <summary>A later loan the trust approved on the homeowner's request (Art 8.3).</summary>
    public static readonly MortgagePermissionSource Request = new(2, "Request");

    /// <summary>A loan the trust permitted outside CommonGround, recorded by staff.</summary>
    public static readonly MortgagePermissionSource Recorded = new(3, "Recorded");

    private MortgagePermissionSource(int value, string displayName) : base(value, displayName) { }
}
