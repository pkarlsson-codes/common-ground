using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.TrackDetails;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.PurchaseWorkflows;

public class PurchaseWorkflowTests
{
    private const string ParcelId = "land-parcel-1234";
    private const string HomeId = "home-5678";
    private static readonly UserId StaffUser = UserId.Generate();
    private static readonly GroundLeaseTemplateId LeaseTemplateId = GroundLeaseTemplateId.Generate();
    private static readonly Homeowner Buyer = Homeowner.From("Alice Johnson", "alice@example.com", "+12025551234");

    private static PurchaseWorkflow CreateWorkflow(decimal price = 250_000m)
    {
        return EventReplay.Replay<PurchaseWorkflow>(PurchaseWorkflow.Initiate(PurchaseWorkflowId.Generate(), LeaseTemplateId, ParcelId, HomeId, Buyer, price, StaffUser));
    }

    private static PurchaseWorkflow SatisfyAllTracks(PurchaseWorkflow workflow)
    {
        workflow = workflow.Decide(a => a.RecordEligibilityVerified(new DateOnly(2025, 3, 1), "Income verified", StaffUser)).Aggregate;
        workflow = workflow.Decide(a => a.RecordAppraisalCompleted(300_000m, new DateOnly(2025, 3, 3), "Jane Appraiser", null, StaffUser)).Aggregate;
        workflow = workflow.Decide(a => a.RecordFinancingCommitted("First National", 240_000m, new DateOnly(2025, 3, 5), StaffUser)).Aggregate;
        workflow = workflow.Decide(a => a.RecordInspectionCompleted(new DateOnly(2025, 3, 10), "Bob Inspector", "All clear", StaffUser)).Aggregate;
        workflow = workflow.Decide(a => a.RecordTitleCleared(new DateOnly(2025, 3, 12), "Acme Title Co", StaffUser)).Aggregate;
        workflow = workflow.Decide(a => a.RecordLeaseExecuted(new DateOnly(2025, 3, 15), StaffUser)).Aggregate;
        return workflow;
    }

    // --- Initiation ---

    [Fact]
    public void Initiate_ValidInputs_CreatesWorkflowInInitiatedPhase()
    {
        var workflow = CreateWorkflow();

        workflow.GroundLeaseTemplateId.Should().Be(LeaseTemplateId);
        workflow.Buyer.FullName.Value.Should().Be("Alice Johnson");
        workflow.PurchasePrice.Should().Be(250_000m);
        workflow.Phase.Should().Be(WorkflowPhase.Initiated);
        workflow.InitiatedBy.Should().Be(StaffUser);
        workflow.IsTerminal.Should().BeFalse();
        workflow.AllMandatoryTracksSatisfied.Should().BeFalse();
    }

    [Fact]
    public void Initiate_ZeroPrice_Throws()
    {
        var act = () => PurchaseWorkflow.Initiate(PurchaseWorkflowId.Generate(), LeaseTemplateId, ParcelId, HomeId, Buyer, 0m, StaffUser);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Initiate_NegativePrice_Throws()
    {
        var act = () => PurchaseWorkflow.Initiate(PurchaseWorkflowId.Generate(), LeaseTemplateId, ParcelId, HomeId, Buyer, -1m, StaffUser);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    // --- Track recording advances phase ---

    [Fact]
    public void RecordEligibilityVerified_FromInitiated_AdvancesToInProgress()
    {
        var workflow = CreateWorkflow();

        workflow = workflow.Decide(a => a.RecordEligibilityVerified(new DateOnly(2025, 3, 1), null, StaffUser)).Aggregate;

        workflow.Phase.Should().Be(WorkflowPhase.InProgress);
        workflow.IsEligibilityVerified.Should().BeTrue();
        workflow.EligibilityDetails!.VerifiedDate.Should().Be(new DateOnly(2025, 3, 1));
    }

    [Fact]
    public void RecordAppraisalCompleted_SetsDetails()
    {
        var workflow = CreateWorkflow();

        workflow = workflow.Decide(a => a.RecordAppraisalCompleted(310_000m, new DateOnly(2025, 3, 3), "Jane Appraiser", "Good condition", StaffUser)).Aggregate;

        workflow.IsAppraisalCompleted.Should().BeTrue();
        workflow.AppraisalDetails.Should().Be(new AppraisalDetails(310_000m, new DateOnly(2025, 3, 3), "Jane Appraiser", "Good condition"));
        workflow.Phase.Should().Be(WorkflowPhase.InProgress);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RecordAppraisalCompleted_ValueNotAboveZero_Throws(int value)
    {
        var workflow = CreateWorkflow();

        var act = () => workflow.RecordAppraisalCompleted(value, new DateOnly(2025, 3, 3), "Jane Appraiser", null, StaffUser);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("homeownership.purchase_workflow.record_appraisal_completed.appraised_value_non_positive");
    }

    [Fact]
    public void RecordAppraisalCompleted_Twice_IsIdempotent()
    {
        var workflow = CreateWorkflow();
        workflow = workflow.Decide(a => a.RecordAppraisalCompleted(310_000m, new DateOnly(2025, 3, 3), "First", null, StaffUser)).Aggregate;

        var (after, events) = workflow.Decide(a => a.RecordAppraisalCompleted(999_000m, new DateOnly(2025, 4, 3), "Second", null, StaffUser));

        events.Should().BeEmpty();
        after.AppraisalDetails!.AppraiserName.Should().Be("First");
    }

    [Fact]
    public void ScheduleClosing_WhenOnlyTheAppraisalIsMissing_Throws()
    {
        var workflow = CreateWorkflow();
        workflow = workflow.Decide(a => a.RecordEligibilityVerified(new DateOnly(2025, 3, 1), null, StaffUser)).Aggregate;
        workflow = workflow.Decide(a => a.RecordFinancingCommitted("Bank", 240_000m, new DateOnly(2025, 3, 5), StaffUser)).Aggregate;
        workflow = workflow.Decide(a => a.RecordInspectionCompleted(new DateOnly(2025, 3, 10), "Inspector", null, StaffUser)).Aggregate;
        workflow = workflow.Decide(a => a.RecordTitleCleared(new DateOnly(2025, 3, 12), null, StaffUser)).Aggregate;
        workflow = workflow.Decide(a => a.RecordLeaseExecuted(new DateOnly(2025, 3, 15), StaffUser)).Aggregate;

        var act = () => workflow.ScheduleClosing(new DateOnly(2025, 4, 1), StaffUser);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("homeownership.purchase_workflow.schedule_closing.mandatory_tracks_incomplete");
    }

    [Fact]
    public void RecordFinancingCommitted_SetsDetails()
    {
        var workflow = CreateWorkflow();

        workflow = workflow.Decide(a => a.RecordFinancingCommitted("First National", 240_000m, new DateOnly(2025, 3, 5), StaffUser)).Aggregate;

        workflow.IsFinancingCommitted.Should().BeTrue();
        workflow.FinancingDetails!.LenderName.Should().Be("First National");
        workflow.FinancingDetails.CommittedAmount.Should().Be(240_000m);
    }

    [Fact]
    public void RecordInspectionCompleted_SetsDetails()
    {
        var workflow = CreateWorkflow();

        workflow = workflow.Decide(a => a.RecordInspectionCompleted(new DateOnly(2025, 3, 10), "Bob Inspector", "All clear", StaffUser)).Aggregate;

        workflow.IsInspectionCompleted.Should().BeTrue();
        workflow.InspectionResult!.InspectorName.Should().Be("Bob Inspector");
    }

    [Fact]
    public void RecordTitleCleared_SetsDetails()
    {
        var workflow = CreateWorkflow();

        workflow = workflow.Decide(a => a.RecordTitleCleared(new DateOnly(2025, 3, 12), "Acme Title Co", StaffUser)).Aggregate;

        workflow.IsTitleCleared.Should().BeTrue();
        workflow.TitleDetails!.TitleCompanyName.Should().Be("Acme Title Co");
    }

    [Fact]
    public void RecordLeaseExecuted_SetsDetails()
    {
        var workflow = CreateWorkflow();

        workflow = workflow.Decide(a => a.RecordLeaseExecuted(new DateOnly(2025, 3, 15), StaffUser)).Aggregate;

        workflow.IsLeaseExecuted.Should().BeTrue();
        workflow.LeaseExecutionDetails!.SignedDate.Should().Be(new DateOnly(2025, 3, 15));
    }

    // --- Parallel tracks ---

    [Fact]
    public void RecordAllTracks_InAnyOrder_AllTracksSatisfied()
    {
        var workflow = CreateWorkflow();

        // Record in non-sequential order to prove parallel independence
        workflow = workflow.Decide(a => a.RecordTitleCleared(new DateOnly(2025, 3, 12), null, StaffUser)).Aggregate;
        workflow = workflow.Decide(a => a.RecordLeaseExecuted(new DateOnly(2025, 3, 15), StaffUser)).Aggregate;
        workflow = workflow.Decide(a => a.RecordEligibilityVerified(new DateOnly(2025, 3, 1), null, StaffUser)).Aggregate;
        workflow = workflow.Decide(a => a.RecordInspectionCompleted(new DateOnly(2025, 3, 10), "Inspector", null, StaffUser)).Aggregate;
        workflow = workflow.Decide(a => a.RecordFinancingCommitted("Bank", 240_000m, new DateOnly(2025, 3, 5), StaffUser)).Aggregate;
        workflow = workflow.Decide(a => a.RecordAppraisalCompleted(300_000m, new DateOnly(2025, 3, 3), "Appraiser", null, StaffUser)).Aggregate;

        workflow.AllMandatoryTracksSatisfied.Should().BeTrue();
        workflow.Phase.Should().Be(WorkflowPhase.InProgress);
    }

    // --- Idempotency ---

    [Fact]
    public void RecordTrack_Twice_IsIdempotent()
    {
        var workflow = CreateWorkflow();

        workflow = workflow.Decide(a => a.RecordEligibilityVerified(new DateOnly(2025, 3, 1), "First", StaffUser)).Aggregate;

        var (after, events) = workflow.Decide(a => a.RecordEligibilityVerified(new DateOnly(2025, 4, 1), "Second", StaffUser));

        events.Should().BeEmpty();
        after.EligibilityDetails!.Notes.Should().Be("First");
    }

    // --- Closing ---

    [Fact]
    public void ScheduleClosing_WhenAllTracksSatisfied_Succeeds()
    {
        var workflow = CreateWorkflow();
        workflow = SatisfyAllTracks(workflow);

        workflow = workflow.Decide(a => a.ScheduleClosing(new DateOnly(2025, 4, 1), StaffUser)).Aggregate;

        workflow.Phase.Should().Be(WorkflowPhase.ClosingScheduled);
        workflow.ScheduledClosingDate.Should().Be(new DateOnly(2025, 4, 1));
    }

    [Fact]
    public void ScheduleClosing_WhenTracksMissing_Throws()
    {
        var workflow = CreateWorkflow();
        workflow = workflow.Decide(a => a.RecordEligibilityVerified(new DateOnly(2025, 3, 1), null, StaffUser)).Aggregate;

        var act = () => workflow.ScheduleClosing(new DateOnly(2025, 4, 1), StaffUser);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("homeownership.purchase_workflow.schedule_closing.mandatory_tracks_incomplete");
    }

    [Fact]
    public void ScheduleClosing_WhenStalledAndAllTracksSatisfied_Succeeds()
    {
        var workflow = CreateWorkflow();
        workflow = SatisfyAllTracks(workflow);
        workflow = workflow.Decide(a => a.StallWorkflow("Waiting on docs", new DateOnly(2025, 3, 20), new DateOnly(2025, 4, 15), StaffUser)).Aggregate;

        workflow = workflow.Decide(a => a.ScheduleClosing(new DateOnly(2025, 4, 1), StaffUser)).Aggregate;

        workflow.Phase.Should().Be(WorkflowPhase.ClosingScheduled);
        workflow.ScheduledClosingDate.Should().Be(new DateOnly(2025, 4, 1));
    }

    [Fact]
    public void ScheduleClosing_WhenNotInProgress_Throws()
    {
        var workflow = CreateWorkflow();
        workflow = SatisfyAllTracks(workflow);
        workflow = workflow.Decide(a => a.ScheduleClosing(new DateOnly(2025, 4, 1), StaffUser)).Aggregate;

        var act = () => workflow.ScheduleClosing(new DateOnly(2025, 4, 2), StaffUser);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("homeownership.purchase_workflow.schedule_closing.invalid_phase");
    }

    [Fact]
    public void RecordPurchaseClosed_WhenClosingScheduled_Succeeds()
    {
        var workflow = CreateWorkflow();
        workflow = SatisfyAllTracks(workflow);
        workflow = workflow.Decide(a => a.ScheduleClosing(new DateOnly(2025, 4, 1), StaffUser)).Aggregate;

        workflow = workflow.Decide(a => a.RecordPurchaseClosed(new DateOnly(2025, 4, 1), "Smooth closing", StaffUser)).Aggregate;

        workflow.Phase.Should().Be(WorkflowPhase.Closed);
        workflow.IsTerminal.Should().BeTrue();
        workflow.ClosingDetails!.ActualClosingDate.Should().Be(new DateOnly(2025, 4, 1));
        workflow.ClosingDetails.Notes.Should().Be("Smooth closing");
    }

    [Fact]
    public void RecordPurchaseClosed_WhenNotScheduled_Throws()
    {
        var workflow = CreateWorkflow();
        workflow = SatisfyAllTracks(workflow);

        var act = () => workflow.RecordPurchaseClosed(new DateOnly(2025, 4, 1), null, StaffUser);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("homeownership.purchase_workflow.record_purchase_closed.phase_not_closing_scheduled");
    }

    // --- Failure ---

    [Fact]
    public void FailPurchase_FromAnyNonTerminal_Succeeds()
    {
        var workflow = CreateWorkflow();

        workflow = workflow.Decide(a => a.FailPurchase("Buyer lost job", new DateOnly(2025, 3, 20), StaffUser)).Aggregate;

        workflow.Phase.Should().Be(WorkflowPhase.Failed);
        workflow.IsTerminal.Should().BeTrue();
        workflow.FailureReason.Should().Be("Buyer lost job");
    }

    [Fact]
    public void FailPurchase_EmptyReason_Throws()
    {
        var workflow = CreateWorkflow();

        var act = () => workflow.FailPurchase("", new DateOnly(2025, 3, 20), StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void FailPurchase_WhenAlreadyTerminal_Throws()
    {
        var workflow = CreateWorkflow();
        workflow = workflow.Decide(a => a.FailPurchase("First failure", new DateOnly(2025, 3, 20), StaffUser)).Aggregate;

        var act = () => workflow.FailPurchase("Second attempt", new DateOnly(2025, 3, 21), StaffUser);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*terminal*");
    }

    // --- Withdrawal ---

    [Fact]
    public void WithdrawPurchase_FromAnyNonTerminal_Succeeds()
    {
        var workflow = CreateWorkflow();
        workflow = workflow.Decide(a => a.RecordEligibilityVerified(new DateOnly(2025, 3, 1), null, StaffUser)).Aggregate;

        workflow = workflow.Decide(a => a.WithdrawPurchase(new DateOnly(2025, 3, 20), "Changed mind", StaffUser)).Aggregate;

        workflow.Phase.Should().Be(WorkflowPhase.Withdrawn);
        workflow.IsTerminal.Should().BeTrue();
        workflow.WithdrawalReason.Should().Be("Changed mind");
    }

    // --- Stall/Resume ---

    [Fact]
    public void StallWorkflow_FromInProgress_Succeeds()
    {
        var workflow = CreateWorkflow();
        workflow = workflow.Decide(a => a.RecordEligibilityVerified(new DateOnly(2025, 3, 1), null, StaffUser)).Aggregate;

        workflow = workflow.Decide(a => a.StallWorkflow("Waiting on appraisal", new DateOnly(2025, 3, 15), new DateOnly(2025, 4, 15), StaffUser)).Aggregate;

        workflow.Phase.Should().Be(WorkflowPhase.Stalled);
        workflow.StalledReason.Should().Be("Waiting on appraisal");
        workflow.StalledDate.Should().Be(new DateOnly(2025, 3, 15));
        workflow.ResolutionTargetDate.Should().Be(new DateOnly(2025, 4, 15));
    }

    [Fact]
    public void StallWorkflow_FromInitiated_Throws()
    {
        var workflow = CreateWorkflow();

        var act = () => workflow.StallWorkflow(
            "Reason", new DateOnly(2025, 3, 15), new DateOnly(2025, 4, 15), StaffUser);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("homeownership.purchase_workflow.stall_workflow.phase_not_in_progress");
    }

    [Fact]
    public void StallWorkflow_ThenRecordTrack_ResumesToInProgress()
    {
        var workflow = CreateWorkflow();
        workflow = workflow.Decide(a => a.RecordEligibilityVerified(new DateOnly(2025, 3, 1), null, StaffUser)).Aggregate;
        workflow = workflow.Decide(a => a.StallWorkflow("Waiting", new DateOnly(2025, 3, 15), new DateOnly(2025, 4, 15), StaffUser)).Aggregate;
        workflow.Phase.Should().Be(WorkflowPhase.Stalled);

        workflow = workflow.Decide(a => a.RecordFinancingCommitted("Bank", 240_000m, new DateOnly(2025, 3, 20), StaffUser)).Aggregate;

        workflow.Phase.Should().Be(WorkflowPhase.InProgress);
    }

    // --- Terminal guard on tracks ---

    [Fact]
    public void RecordTrack_WhenTerminal_Throws()
    {
        var workflow = CreateWorkflow();
        workflow = workflow.Decide(a => a.FailPurchase("Deal fell through", new DateOnly(2025, 3, 20), StaffUser)).Aggregate;

        var act = () => workflow.RecordEligibilityVerified(
            new DateOnly(2025, 3, 25), null, StaffUser);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*terminal*");
    }

    // --- Event data verification ---

    [Fact]
    public void EventsEmitted_ContainCorrectData()
    {
        var initiated = PurchaseWorkflow.Initiate(PurchaseWorkflowId.Generate(), LeaseTemplateId, ParcelId, HomeId, Buyer, 250_000m, StaffUser);

        initiated.GroundLeaseTemplateId.Should().Be(LeaseTemplateId.Value);
        initiated.BuyerFullName.Should().Be("Alice Johnson");
        initiated.BuyerEmail.Should().Be("alice@example.com");
        initiated.BuyerPhone.Should().Be("+12025551234");
        initiated.PurchasePrice.Should().Be(250_000m);
    }
}
