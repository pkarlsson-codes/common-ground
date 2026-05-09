using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.TrackDetails;
using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows;

/// <summary>
/// A long-running purchase workflow modeled as an event-sourced aggregate.
/// Tracks multiple real-world dependencies as parallel tracks that can be
/// satisfied independently and in any order.
/// </summary>
public sealed class PurchaseWorkflow : AggregateRoot<PurchaseWorkflowId>
{
#pragma warning disable CS8618
    private PurchaseWorkflow() { }
#pragma warning restore CS8618

    /// <summary>Gets the ground lease this purchase is for.</summary>
    public GroundLeaseId GroundLeaseId { get; private set; }

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

    /// <summary>Gets whether financing has been committed.</summary>
    public bool IsFinancingCommitted => FinancingDetails is not null;

    /// <summary>Gets whether the inspection has been completed.</summary>
    public bool IsInspectionCompleted => InspectionResult is not null;

    /// <summary>Gets whether the title has been cleared.</summary>
    public bool IsTitleCleared => TitleDetails is not null;

    /// <summary>Gets whether the lease has been executed.</summary>
    public bool IsLeaseExecuted => LeaseExecutionDetails is not null;

    /// <summary>Gets whether all five mandatory tracks are satisfied.</summary>
    public bool AllMandatoryTracksSatisfied =>
        IsEligibilityVerified && IsFinancingCommitted &&
        IsInspectionCompleted && IsTitleCleared && IsLeaseExecuted;

    /// <summary>Gets whether the workflow is in a terminal phase.</summary>
    public bool IsTerminal =>
        Phase is WorkflowPhase.Closed
            or WorkflowPhase.Failed
            or WorkflowPhase.Withdrawn;

    /// <summary>Initiates a new purchase workflow.</summary>
    /// <param name="groundLeaseId">The ground lease to purchase.</param>
    /// <param name="buyer">The prospective buyer.</param>
    /// <param name="purchasePrice">The offer price. Must be positive.</param>
    /// <param name="initiatedBy">The user initiating the workflow.</param>
    /// <returns>A new purchase workflow instance.</returns>
    public static PurchaseWorkflow Initiate(
        GroundLeaseId groundLeaseId,
        Homeowner buyer,
        decimal purchasePrice,
        UserId initiatedBy)
    {
        ArgumentNullException.ThrowIfNull(groundLeaseId);
        ArgumentNullException.ThrowIfNull(buyer);
        ArgumentNullException.ThrowIfNull(initiatedBy);

        if (purchasePrice <= 0)
        {
            throw BusinessRuleViolation.PurchaseWorkflow.Initiate.PurchasePriceNonPositive(purchasePrice);
        }

        var workflow = new PurchaseWorkflow();
        workflow.ApplyChange(new PurchaseWorkflowInitiated(
            PurchaseWorkflowId.Generate().Value,
            groundLeaseId.Value,
            buyer.FullName,
            buyer.Email,
            buyer.Phone,
            purchasePrice,
            initiatedBy.Value));

        return workflow;
    }

    /// <summary>Records that buyer eligibility has been verified. Idempotent.</summary>
    /// <param name="verifiedDate">Date of verification.</param>
    /// <param name="notes">Optional notes.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public void RecordEligibilityVerified(
        DateOnly verifiedDate, string? notes, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);
        GuardNotTerminal();
        if (IsEligibilityVerified) return;

        ApplyChange(new BuyerEligibilityVerified(
            Id.Value, verifiedDate, notes,
            recordedBy.Value));
    }

    /// <summary>Records that financing has been committed by a lender. Idempotent.</summary>
    /// <param name="lenderName">Name of the lender.</param>
    /// <param name="committedAmount">Amount committed.</param>
    /// <param name="preapprovalLetterDate">Date of pre-approval letter.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public void RecordFinancingCommitted(
        string lenderName, decimal committedAmount,
        DateOnly preapprovalLetterDate, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);
        GuardNotTerminal();
        if (IsFinancingCommitted) return;

        ApplyChange(new FinancingCommitted(
            Id.Value, lenderName, committedAmount,
            preapprovalLetterDate, recordedBy.Value));
    }

    /// <summary>Records that a home inspection has been completed. Idempotent.</summary>
    /// <param name="inspectionDate">Date of the inspection.</param>
    /// <param name="inspectorName">Name of the inspector.</param>
    /// <param name="notes">Optional notes.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public void RecordInspectionCompleted(
        DateOnly inspectionDate, string inspectorName,
        string? notes, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);
        GuardNotTerminal();
        if (IsInspectionCompleted) return;

        ApplyChange(new InspectionCompleted(
            Id.Value, inspectionDate, inspectorName, notes,
            recordedBy.Value));
    }

    /// <summary>Records that the title has been cleared. Idempotent.</summary>
    /// <param name="clearedDate">Date the title was cleared.</param>
    /// <param name="titleCompanyName">Optional title company name.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public void RecordTitleCleared(
        DateOnly clearedDate, string? titleCompanyName,
        UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);
        GuardNotTerminal();
        if (IsTitleCleared) return;

        ApplyChange(new TitleCleared(
            Id.Value, clearedDate, titleCompanyName,
            recordedBy.Value));
    }

    /// <summary>Records that both parties have signed the ground lease. Idempotent.</summary>
    /// <param name="signedDate">Date of signing.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public void RecordLeaseExecuted(
        DateOnly signedDate, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);
        GuardNotTerminal();
        if (IsLeaseExecuted) return;

        ApplyChange(new LeaseExecuted(
            Id.Value, signedDate, recordedBy.Value));
    }

    /// <summary>Schedules the closing date. Requires all mandatory tracks satisfied and InProgress phase.</summary>
    /// <param name="scheduledClosingDate">The date to schedule closing.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public void ScheduleClosing(
        DateOnly scheduledClosingDate, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);
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

        ApplyChange(new ClosingScheduled(
            Id.Value, scheduledClosingDate,
            recordedBy.Value));
    }

    /// <summary>Records that the purchase has been closed. Requires ClosingScheduled phase.</summary>
    /// <param name="actualClosingDate">The actual date of closing.</param>
    /// <param name="notes">Optional closing notes.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public void RecordPurchaseClosed(
        DateOnly actualClosingDate, string? notes,
        UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);

        if (Phase != WorkflowPhase.ClosingScheduled)
        {
            throw BusinessRuleViolation.PurchaseWorkflow.RecordPurchaseClosed.PhaseNotClosingScheduled(Phase);
        }

        ApplyChange(new PurchaseClosed(
            Id.Value, actualClosingDate, notes,
            recordedBy.Value));
    }

    /// <summary>Records that the purchase has failed.</summary>
    /// <param name="failureReason">Reason for failure. Required.</param>
    /// <param name="failedDate">Date of failure.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public void FailPurchase(
        string failureReason, DateOnly failedDate,
        UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);
        GuardNotTerminal();

        if (string.IsNullOrWhiteSpace(failureReason))
        {
            throw BusinessRuleViolation.PurchaseWorkflow.FailPurchase.FailureReasonRequired();
        }

        ApplyChange(new PurchaseFailed(
            Id.Value, failureReason, failedDate,
            recordedBy.Value));
    }

    /// <summary>Records that the buyer has withdrawn.</summary>
    /// <param name="withdrawnDate">Date of withdrawal.</param>
    /// <param name="reason">Optional withdrawal reason.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public void WithdrawPurchase(
        DateOnly withdrawnDate, string? reason,
        UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);
        GuardNotTerminal();

        ApplyChange(new PurchaseWithdrawn(
            Id.Value, withdrawnDate, reason,
            recordedBy.Value));
    }

    /// <summary>Records that the workflow is stalled. Only from InProgress phase.</summary>
    /// <param name="stalledReason">Reason for the stall.</param>
    /// <param name="stalledDate">Date the stall occurred.</param>
    /// <param name="resolutionTargetDate">Expected resolution date.</param>
    /// <param name="recordedBy">The user recording this fact.</param>
    public void StallWorkflow(
        string stalledReason, DateOnly stalledDate,
        DateOnly resolutionTargetDate, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);

        if (Phase != WorkflowPhase.InProgress)
        {
            throw BusinessRuleViolation.PurchaseWorkflow.StallWorkflow.PhaseNotInProgress(Phase);
        }

        ApplyChange(new WorkflowStalled(
            Id.Value, stalledReason, stalledDate,
            resolutionTargetDate, recordedBy.Value));
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case PurchaseWorkflowInitiated e: Apply(e); break;
            case BuyerEligibilityVerified e: Apply(e); break;
            case FinancingCommitted e: Apply(e); break;
            case InspectionCompleted e: Apply(e); break;
            case TitleCleared e: Apply(e); break;
            case LeaseExecuted e: Apply(e); break;
            case ClosingScheduled e: Apply(e); break;
            case PurchaseClosed e: Apply(e); break;
            case PurchaseFailed e: Apply(e); break;
            case PurchaseWithdrawn e: Apply(e); break;
            case WorkflowStalled e: Apply(e); break;
            default: throw new InvalidOperationException(
                $"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(PurchaseWorkflowInitiated e)
    {
        Id = PurchaseWorkflowId.Create(e.AggregateId);
        GroundLeaseId = GroundLeaseId.Create(e.GroundLeaseId);
        Buyer = new Homeowner(e.BuyerFullName, e.BuyerEmail, e.BuyerPhone);
        PurchasePrice = e.PurchasePrice;
        Phase = WorkflowPhase.Initiated;
        InitiatedBy = UserId.Create(e.InitiatedBy);
        CreatedAt = e.OccurredAt;
    }

    private void Apply(BuyerEligibilityVerified e)
    {
        EligibilityDetails = new EligibilityDetails(e.VerifiedDate, e.EligibilityNotes);
        AdvancePhaseIfNeeded();
    }

    private void Apply(FinancingCommitted e)
    {
        FinancingDetails = new FinancingDetails(
            e.LenderName, e.CommittedAmount, e.PreapprovalLetterDate);
        AdvancePhaseIfNeeded();
    }

    private void Apply(InspectionCompleted e)
    {
        InspectionResult = new InspectionResult(
            e.InspectionDate, e.InspectorName, e.InspectionNotes);
        AdvancePhaseIfNeeded();
    }

    private void Apply(TitleCleared e)
    {
        TitleDetails = new TitleDetails(e.ClearedDate, e.TitleCompanyName);
        AdvancePhaseIfNeeded();
    }

    private void Apply(LeaseExecuted e)
    {
        LeaseExecutionDetails = new LeaseExecutionDetails(e.SignedDate);
        AdvancePhaseIfNeeded();
    }

    private void Apply(ClosingScheduled e)
    {
        ScheduledClosingDate = e.ScheduledClosingDate;
        Phase = WorkflowPhase.ClosingScheduled;
    }

    private void Apply(PurchaseClosed e)
    {
        ClosingDetails = new ClosingDetails(e.ActualClosingDate, e.ClosingNotes);
        Phase = WorkflowPhase.Closed;
    }

    private void Apply(PurchaseFailed e)
    {
        FailureReason = e.FailureReason;
        Phase = WorkflowPhase.Failed;
    }

    private void Apply(PurchaseWithdrawn e)
    {
        WithdrawalReason = e.WithdrawalReason;
        Phase = WorkflowPhase.Withdrawn;
    }

    private void Apply(WorkflowStalled e)
    {
        StalledReason = e.StalledReason;
        StalledDate = e.StalledDate;
        ResolutionTargetDate = e.ResolutionTargetDate;
        Phase = WorkflowPhase.Stalled;
    }

    private void AdvancePhaseIfNeeded()
    {
        if (Phase is WorkflowPhase.Initiated or WorkflowPhase.Stalled)
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
