using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows;

/// <summary>The steps an open purchase works through, in the order they are usually worked.</summary>
public sealed class PurchaseStep : Enumeration<PurchaseStep>
{
    /// <summary>Verifying the buyer is eligible.</summary>
    public static readonly PurchaseStep Eligibility = new(0, nameof(Eligibility));

    /// <summary>The buyer's lender committing to the financing.</summary>
    public static readonly PurchaseStep Financing = new(1, nameof(Financing));

    /// <summary>Inspecting the home.</summary>
    public static readonly PurchaseStep Inspection = new(2, nameof(Inspection));

    /// <summary>Clearing the title.</summary>
    public static readonly PurchaseStep Title = new(3, nameof(Title));

    /// <summary>The buyer signing the ground lease.</summary>
    public static readonly PurchaseStep LeaseSigning = new(4, nameof(LeaseSigning));

    /// <summary>Scheduling the closing, then closing.</summary>
    public static readonly PurchaseStep Closing = new(5, nameof(Closing));

    private PurchaseStep(int value, string displayName) : base(value, displayName) { }

    /// <summary>
    /// The step a purchase waits on: the first of the five tracks, in order, it hasn't recorded, or
    /// <see cref="Closing"/> once it has recorded them all. Tracks can be recorded in any order, so later ones may
    /// already be done.
    /// </summary>
    /// <param name="eligibilityVerified">Whether the buyer's eligibility is recorded.</param>
    /// <param name="financingCommitted">Whether the financing commitment is recorded.</param>
    /// <param name="inspectionCompleted">Whether the inspection is recorded.</param>
    /// <param name="titleCleared">Whether title clearance is recorded.</param>
    /// <param name="leaseExecuted">Whether the lease signing is recorded.</param>
    /// <returns>The step the purchase waits on.</returns>
    public static PurchaseStep WaitingOn(
        bool eligibilityVerified,
        bool financingCommitted,
        bool inspectionCompleted,
        bool titleCleared,
        bool leaseExecuted)
    {
        if (!eligibilityVerified)
            return Eligibility;
        if (!financingCommitted)
            return Financing;
        if (!inspectionCompleted)
            return Inspection;
        if (!titleCleared)
            return Title;
        return leaseExecuted ? Closing : LeaseSigning;
    }
}
