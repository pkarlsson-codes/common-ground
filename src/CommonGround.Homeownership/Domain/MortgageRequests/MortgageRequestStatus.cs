using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.MortgageRequests;

/// <summary>Where a homeowner's request to mortgage the home stands.</summary>
public sealed class MortgageRequestStatus : Enumeration<MortgageRequestStatus>
{
    /// <summary>Received and waiting for the trust's decision.</summary>
    public static readonly MortgageRequestStatus Pending = new(1, "Pending");

    /// <summary>The trust gave written permission; the loan hasn't been recorded closed.</summary>
    public static readonly MortgageRequestStatus Approved = new(2, "Approved");

    /// <summary>The trust refused the loan.</summary>
    public static readonly MortgageRequestStatus Denied = new(3, "Denied");

    /// <summary>The loan closed and is a permitted mortgage on the lease.</summary>
    public static readonly MortgageRequestStatus Closed = new(4, "Closed");

    /// <summary>The homeowner dropped the loan before it closed.</summary>
    public static readonly MortgageRequestStatus Withdrawn = new(5, "Withdrawn");

    private MortgageRequestStatus(int value, string displayName) : base(value, displayName) { }
}
