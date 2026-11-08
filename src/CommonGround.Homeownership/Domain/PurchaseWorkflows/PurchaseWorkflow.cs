using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.TrackDetails;
using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows;

/// <summary>
/// A long-running purchase workflow modeled as an event-sourced aggregate.
/// Tracks multiple real-world dependencies as parallel tracks that can be
/// satisfied independently and in any order.
/// </summary>
public sealed class PurchaseWorkflow : AggregateRoot<PurchaseWorkflowId>
{
#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private PurchaseWorkflow() { }
#pragma warning restore CS8618

    /// <summary>Gets the ground lease this purchase is for.</summary>
    public GroundLeaseTemplateId GroundLeaseTemplateId { get; private set; }

    /// <summary>Gets the land parcel ID of the property being purchased.</summary>
    public string LandParcelId { get; private set; }

    /// <summary>Gets the home ID of the property being purchased.</summary>
    public string HomeId { get; private set; }

    /// <summary>Gets the homeowner application this workflow was initiated from, or null for manual/resale-initiated workflows.</summary>
    public string? ApplicationId { get; private set; }

    /// <summary>Gets the buying household, or null for manual/resale-initiated workflows.</summary>
    public string? HouseholdId { get; private set; }

    /// <summary>Gets the prospective buyer.</summary>
    public Homeowner Buyer { get; private set; }

    /// <summary>Gets the offer price.</summary>
    public decimal PurchasePrice { get; private set; }

    /// <summary>Gets the current lifecycle phase.</summary>
    public WorkflowPhase Phase { get; private set; }

    /// <summary>Gets when this workflow was created.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Gets the user who initiated this workflow.</summary>
    public UserId InitiatedBy { get; private set; }

    /// <summary>Gets eligibility track details, or null if not yet verified.</summary>
    public EligibilityDetails? EligibilityDetails { get; private set; }

    /// <summary>Gets appraisal track details, or null if not yet completed.</summary>
    public AppraisalDetails? AppraisalDetails { get; private set; }

    /// <summary>Gets financing track details, or null if not yet committed.</summary>
    public FinancingDetails? FinancingDetails { get; private set; }

    /// <summary>Gets inspection track details, or null if not yet completed.</summary>
    public InspectionResult? InspectionResult { get; private set; }

    /// <summary>Gets title track details, or null if not yet cleared.</summary>
    public TitleDetails? TitleDetails { get; private set; }

    /// <summary>Gets lease execution track details, or null if not yet executed.</summary>
    public LeaseExecutionDetails? LeaseExecutionDetails { get; private set; }

    /// <summary>Gets the scheduled closing date, if set.</summary>
    public DateOnly? ScheduledClosingDate { get; private set; }

    /// <summary>Gets closing details, or null if not yet closed.</summary>
    public ClosingDetails? ClosingDetails { get; private set; }

    /// <summary>Gets the failure reason, if the workflow failed.</summary>
    public string? FailureReason { get; private set; }

    /// <summary>Gets the withdrawal reason, if the buyer withdrew.</summary>
    public string? WithdrawalReason { get; private set; }

    /// <summary>Gets the stalled reason, if the workflow is stalled.</summary>
    public string? StalledReason { get; private set; }

    /// <summary>Gets the date the workflow was stalled.</summary>
    public DateOnly? StalledDate { get; private set; }

    /// <summary>Gets the expected resolution target date for a stall.</summary>
    public DateOnly? ResolutionTargetDate { get; private set; }

    /// <summary>Gets whether eligibility has been verified.</summary>
    public bool IsEligibilityVerified => EligibilityDetails is not null;

    /// <summary>Gets whether the appraisal has been completed.</summary>
    public bool IsAppraisalCompleted => AppraisalDetails is not null;

    /// <summary>Gets whether financing has been committed.</summary>
    public bool IsFinancingCommitted => FinancingDetails is not null;

    /// <summary>Gets whether the inspection has been completed.</summary>
    public bool IsInspectionCompleted => InspectionResult is not null;

    /// <summary>Gets whether the title has been cleared.</summary>
    public bool IsTitleCleared => TitleDetails is not null;

    /// <summary>Gets whether the lease has been executed.</summary>
    public bool IsLeaseExecuted => LeaseExecutionDetails is not null;

    /// <summary>Gets whether all six mandatory tracks are satisfied.</summary>
    public bool AllMandatoryTracksSatisfied =>
        IsEligibilityVerified && IsAppraisalCompleted && IsFinancingCommitted &&
        IsInspectionCompleted && IsTitleCleared && IsLeaseExecuted;

    /// <summary>Gets whether the workflow is in a terminal phase.</summary>
    public bool IsTerminal => Phase.IsTerminal;

    /// <summary>Initiates a new purchase workflow.</summary>
    /// <param name="id">The new stream's ID, chosen by the caller.</param>
    /// <param name="groundLeaseTemplateId">The ground lease template to use.</param>
    /// <param name="landParcelId">The land parcel ID of the property being purchased.</param>
    /// <param name="homeId">The home ID of the property being purchased.</param>
    /// <param name="buyer">The prospective buyer.</param>
    /// <param name="purchasePrice">The offer price. Must be positive.</param>
    /// <param name="initiatedBy">The user initiating the workflow.</param>
    /// <param name="applicationId">The homeowner application this workflow originates from, or null for manual/resale-initiated workflows.</param>
    /// <param name="householdId">The buying household, or null for manual/resale-initiated workflows.</param>
    /// <returns>A new purchase workflow instance.</returns>
    public static PurchaseWorkflowInitiated Initiate(
        PurchaseWorkflowId id,
        GroundLeaseTemplateId groundLeaseTemplateId,
        string landParcelId,
        string homeId,
        Homeowner buyer,
        decimal purchasePrice,
        UserId initiatedBy,
        string? applicationId = null,
        string? householdId = null)
    {
        ArgumentNullException.ThrowIfNull(landParcelId);
        ArgumentNullException.ThrowIfNull(homeId);
        ArgumentNullException.ThrowIfNull(buyer);

        if (purchasePrice <= 0)
        {
            throw BusinessRuleViolation.PurchaseWorkflow.Initiate.PurchasePriceNonPositive(purchasePrice);
        }

        return new PurchaseWorkflowInitiated(
            id.Value,
            groundLeaseTemplateId.Value,
            landParcelId,
            homeId,
            buyer.FullName.Value,
            buyer.Email.Value,
            buyer.Phone?.Value,
            purchasePrice,
            initiatedBy.Value,
            applicationId,
            householdId);
    }

    /// <summary>Records that buyer eligibility has been verified. Idempotent.</summary>
    /// <param name="verifiedDate">Date of verification.</param>
    /// <param name="notes">Optional notes.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public BuyerEligibilityVerified? RecordEligibilityVerified(
        DateOnly verifiedDate, string? notes, UserId recordedBy)
    {
        GuardNotTerminal();
        if (IsEligibilityVerified) return null;

        return new BuyerEligibilityVerified(
            Id.Value, verifiedDate, notes,
            recordedBy.Value);
    }

    /// <summary>Records that the home's appraisal has been completed. Idempotent.</summary>
    /// <param name="appraisedValue">The home's market value without the lease's restrictions. Must be above zero.</param>
    /// <param name="appraisalDate">Date of the appraisal.</param>
    /// <param name="appraiserName">Name of the appraiser.</param>
    /// <param name="notes">Optional notes.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public AppraisalCompleted? RecordAppraisalCompleted(
        decimal appraisedValue, DateOnly appraisalDate, string appraiserName,
        string? notes, UserId recordedBy)
    {
        GuardNotTerminal();
        if (IsAppraisalCompleted) return null;

        if (appraisedValue <= 0)
        {
            throw BusinessRuleViolation.PurchaseWorkflow.RecordAppraisalCompleted.AppraisedValueNonPositive(appraisedValue);
        }

        return new AppraisalCompleted(
            Id.Value, appraisedValue, appraisalDate, appraiserName, notes,
            recordedBy.Value);
    }

    /// <summary>Records that financing has been committed by a lender. Idempotent.</summary>
    /// <param name="lenderName">Name of the lender.</param>
    /// <param name="committedAmount">Amount committed.</param>
    /// <param name="preapprovalLetterDate">Date of pre-approval letter.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public FinancingCommitted? RecordFinancingCommitted(
        string lenderName, decimal committedAmount,
        DateOnly preapprovalLetterDate, UserId recordedBy)
    {
        GuardNotTerminal();
        if (IsFinancingCommitted) return null;

        return new FinancingCommitted(
            Id.Value, lenderName, committedAmount,
            preapprovalLetterDate, recordedBy.Value);
    }

    /// <summary>Records that a home inspection has been completed. Idempotent.</summary>
    /// <param name="inspectionDate">Date of the inspection.</param>
    /// <param name="inspectorName">Name of the inspector.</param>
    /// <param name="notes">Optional notes.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public InspectionCompleted? RecordInspectionCompleted(
        DateOnly inspectionDate, string inspectorName,
        string? notes, UserId recordedBy)
    {
        GuardNotTerminal();
        if (IsInspectionCompleted) return null;

        return new InspectionCompleted(
            Id.Value, inspectionDate, inspectorName, notes,
            recordedBy.Value);
    }

    /// <summary>Records that the title has been cleared. Idempotent.</summary>
    /// <param name="clearedDate">Date the title was cleared.</param>
    /// <param name="titleCompanyName">Optional title company name.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public TitleCleared? RecordTitleCleared(
        DateOnly clearedDate, string? titleCompanyName,
        UserId recordedBy)
    {
        GuardNotTerminal();
        if (IsTitleCleared) return null;

        return new TitleCleared(
            Id.Value, clearedDate, titleCompanyName,
            recordedBy.Value);
    }

    /// <summary>Records that both parties have signed the ground lease. Idempotent.</summary>
    /// <param name="signedDate">Date of signing.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public LeaseExecuted? RecordLeaseExecuted(
        DateOnly signedDate, UserId recordedBy)
    {
        GuardNotTerminal();
        if (IsLeaseExecuted) return null;

        return new LeaseExecuted(
            Id.Value, signedDate, recordedBy.Value);
    }

    /// <summary>Schedules the closing date. Requires all mandatory tracks satisfied and InProgress phase.</summary>
    /// <param name="scheduledClosingDate">The date to schedule closing.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public ClosingScheduled ScheduleClosing(
        DateOnly scheduledClosingDate, UserId recordedBy)
    {
        GuardNotTerminal();

        if (!AllMandatoryTracksSatisfied)
        {
            throw BusinessRuleViolation.PurchaseWorkflow.ScheduleClosing.MandatoryTracksIncomplete();
        }

        var canScheduleFromCurrentPhase =
            Phase == WorkflowPhase.InProgress
            || (Phase == WorkflowPhase.Stalled && AllMandatoryTracksSatisfied);

        if (!canScheduleFromCurrentPhase)
        {
            throw BusinessRuleViolation.PurchaseWorkflow.ScheduleClosing.InvalidPhase(Phase);
        }

        return new ClosingScheduled(
            Id.Value, scheduledClosingDate,
            recordedBy.Value);
    }

    /// <summary>Records that the purchase has been closed. Requires ClosingScheduled phase.</summary>
    /// <param name="actualClosingDate">The actual date of closing.</param>
    /// <param name="notes">Optional closing notes.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public PurchaseClosed RecordPurchaseClosed(
        DateOnly actualClosingDate, string? notes,
        UserId recordedBy)
    {
        if (Phase != WorkflowPhase.ClosingScheduled)
        {
            throw BusinessRuleViolation.PurchaseWorkflow.RecordPurchaseClosed.PhaseNotClosingScheduled(Phase);
        }

        return new PurchaseClosed(
            Id.Value, actualClosingDate, notes,
            recordedBy.Value);
    }

    /// <summary>Records that the purchase has failed.</summary>
    /// <param name="failureReason">Reason for failure. Required.</param>
    /// <param name="failedDate">Date of failure.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public PurchaseFailed FailPurchase(
        string failureReason, DateOnly failedDate,
        UserId recordedBy)
    {
        GuardNotTerminal();

        if (string.IsNullOrWhiteSpace(failureReason))
        {
            throw BusinessRuleViolation.PurchaseWorkflow.FailPurchase.FailureReasonRequired();
        }

        return new PurchaseFailed(
            Id.Value, failureReason, failedDate,
            recordedBy.Value);
    }

    /// <summary>Records that the buyer has withdrawn.</summary>
    /// <param name="withdrawnDate">Date of withdrawal.</param>
    /// <param name="reason">Optional withdrawal reason.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public PurchaseWithdrawn WithdrawPurchase(
        DateOnly withdrawnDate, string? reason,
        UserId recordedBy)
    {
        GuardNotTerminal();

        return new PurchaseWithdrawn(
            Id.Value, withdrawnDate, reason,
            recordedBy.Value);
    }

    /// <summary>Records that the workflow is stalled. Only from InProgress phase.</summary>
    /// <param name="stalledReason">Reason for the stall.</param>
    /// <param name="stalledDate">Date the stall occurred.</param>
    /// <param name="resolutionTargetDate">Expected resolution date.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public WorkflowStalled StallWorkflow(
        string stalledReason, DateOnly stalledDate,
        DateOnly resolutionTargetDate, UserId recordedBy)
    {
        if (Phase != WorkflowPhase.InProgress)
        {
            throw BusinessRuleViolation.PurchaseWorkflow.StallWorkflow.PhaseNotInProgress(Phase);
        }

        return new WorkflowStalled(
            Id.Value, stalledReason, stalledDate,
            resolutionTargetDate, recordedBy.Value);
    }

    /// <summary>Starts a purchase workflow from its first event.</summary>
    public static PurchaseWorkflow Create(PurchaseWorkflowInitiated e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var purchaseWorkflow = new PurchaseWorkflow();
        purchaseWorkflow.Apply(e);
        return purchaseWorkflow;
    }

    private void Apply(PurchaseWorkflowInitiated e)
    {
        Id = PurchaseWorkflowId.Create(e.AggregateId);
        GroundLeaseTemplateId = GroundLeaseTemplateId.Create(e.GroundLeaseTemplateId);
        LandParcelId = e.LandParcelId;
        HomeId = e.HomeId;
        Buyer = Homeowner.From(e.BuyerFullName, e.BuyerEmail, e.BuyerPhone);
        PurchasePrice = e.PurchasePrice;
        Phase = WorkflowPhase.Initiated;
        InitiatedBy = UserId.Create(e.InitiatedBy);
        CreatedAt = e.OccurredAt;
        ApplicationId = e.ApplicationId;
        HouseholdId = e.HouseholdId;
    }

    /// <summary>Applies <see cref="BuyerEligibilityVerified"/>.</summary>
    public void Apply(BuyerEligibilityVerified e)
    {
        EligibilityDetails = new EligibilityDetails(e.VerifiedDate, e.EligibilityNotes);
        AdvancePhaseIfNeeded();
    }

    /// <summary>Applies <see cref="AppraisalCompleted"/>.</summary>
    public void Apply(AppraisalCompleted e)
    {
        AppraisalDetails = new AppraisalDetails(
            e.AppraisedValue, e.AppraisalDate, e.AppraiserName, e.AppraisalNotes);
        AdvancePhaseIfNeeded();
    }

    /// <summary>Applies <see cref="FinancingCommitted"/>.</summary>
    public void Apply(FinancingCommitted e)
    {
        FinancingDetails = new FinancingDetails(
            e.LenderName, e.CommittedAmount, e.PreapprovalLetterDate);
        AdvancePhaseIfNeeded();
    }

    /// <summary>Applies <see cref="InspectionCompleted"/>.</summary>
    public void Apply(InspectionCompleted e)
    {
        InspectionResult = new InspectionResult(
            e.InspectionDate, e.InspectorName, e.InspectionNotes);
        AdvancePhaseIfNeeded();
    }

    /// <summary>Applies <see cref="TitleCleared"/>.</summary>
    public void Apply(TitleCleared e)
    {
        TitleDetails = new TitleDetails(e.ClearedDate, e.TitleCompanyName);
        AdvancePhaseIfNeeded();
    }

    /// <summary>Applies <see cref="LeaseExecuted"/>.</summary>
    public void Apply(LeaseExecuted e)
    {
        LeaseExecutionDetails = new LeaseExecutionDetails(e.SignedDate);
        AdvancePhaseIfNeeded();
    }

    /// <summary>Applies <see cref="ClosingScheduled"/>.</summary>
    public void Apply(ClosingScheduled e)
    {
        ScheduledClosingDate = e.ScheduledClosingDate;
        Phase = WorkflowPhase.ClosingScheduled;
    }

    /// <summary>Applies <see cref="PurchaseClosed"/>.</summary>
    public void Apply(PurchaseClosed e)
    {
        ClosingDetails = new ClosingDetails(e.ActualClosingDate, e.ClosingNotes);
        Phase = WorkflowPhase.Closed;
    }

    /// <summary>Applies <see cref="PurchaseFailed"/>.</summary>
    public void Apply(PurchaseFailed e)
    {
        FailureReason = e.FailureReason;
        Phase = WorkflowPhase.Failed;
    }

    /// <summary>Applies <see cref="PurchaseWithdrawn"/>.</summary>
    public void Apply(PurchaseWithdrawn e)
    {
        WithdrawalReason = e.WithdrawalReason;
        Phase = WorkflowPhase.Withdrawn;
    }

    /// <summary>Applies <see cref="WorkflowStalled"/>.</summary>
    public void Apply(WorkflowStalled e)
    {
        StalledReason = e.StalledReason;
        StalledDate = e.StalledDate;
        ResolutionTargetDate = e.ResolutionTargetDate;
        Phase = WorkflowPhase.Stalled;
    }

    private void AdvancePhaseIfNeeded()
    {
        if (Phase.CanAdvanceToInProgress)
        {
            Phase = WorkflowPhase.InProgress;
        }
    }

    private void GuardNotTerminal()
    {
        if (IsTerminal)
        {
            throw BusinessRuleViolation.PurchaseWorkflow.Modify.TerminalPhase(Phase);
        }
    }
}
