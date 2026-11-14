using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows;

/// <summary>The steps an open purchase works through, in the order they are usually worked.</summary>
public sealed class PurchaseStep : Enumeration<PurchaseStep>
{
    /// <summary>Verifying the buyer is eligible.</summary>
    public static readonly PurchaseStep Eligibility = new(0, nameof(Eligibility));

    /// <summary>Appraising the home.</summary>
    public static readonly PurchaseStep Appraisal = new(1, nameof(Appraisal));

    /// <summary>The buyer's lender committing to the financing.</summary>
    public static readonly PurchaseStep Financing = new(2, nameof(Financing));

    /// <summary>Inspecting the home.</summary>
    public static readonly PurchaseStep Inspection = new(3, nameof(Inspection));

    /// <summary>Clearing the title.</summary>
    public static readonly PurchaseStep Title = new(4, nameof(Title));

    /// <summary>The buyer signing the ground lease.</summary>
    public static readonly PurchaseStep LeaseSigning = new(5, nameof(LeaseSigning));

    /// <summary>Recording the buyer's signed disclosure acknowledgment.</summary>
    public static readonly PurchaseStep Disclosure = new(6, nameof(Disclosure));

    /// <summary>Scheduling the closing, then closing.</summary>
    public static readonly PurchaseStep Closing = new(7, nameof(Closing));

    private PurchaseStep(int value, string displayName) : base(value, displayName) { }

    /// <summary>
    /// The step a purchase waits on: the first of the seven tracks, in order, it hasn't recorded, or
    /// <see cref="Closing"/> once it has recorded them all. Tracks can be recorded in any order, so later ones may
    /// already be done.
    /// </summary>
    /// <param name="eligibilityVerified">Whether the buyer's eligibility is recorded.</param>
    /// <param name="appraisalCompleted">Whether the appraisal is recorded.</param>
    /// <param name="financingCommitted">Whether the financing commitment is recorded.</param>
    /// <param name="inspectionCompleted">Whether the inspection is recorded.</param>
    /// <param name="titleCleared">Whether title clearance is recorded.</param>
    /// <param name="leaseExecuted">Whether the lease signing is recorded.</param>
    /// <param name="disclosureAcknowledged">Whether the buyer's signed disclosure acknowledgment is recorded.</param>
    /// <returns>The step the purchase waits on.</returns>
    public static PurchaseStep WaitingOn(
        bool eligibilityVerified,
        bool appraisalCompleted,
        bool financingCommitted,
        bool inspectionCompleted,
        bool titleCleared,
        bool leaseExecuted,
        bool disclosureAcknowledged)
    {
        if (!eligibilityVerified)
            return Eligibility;
        if (!appraisalCompleted)
            return Appraisal;
        if (!financingCommitted)
            return Financing;
        if (!inspectionCompleted)
            return Inspection;
        if (!titleCleared)
            return Title;
        if (!leaseExecuted)
            return LeaseSigning;
        return disclosureAcknowledged ? Closing : Disclosure;
    }
}
