using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.PurchaseWorkflows;

public class PurchaseWorkflowTests
{
    private static readonly UserId StaffUser = UserId.Generate();
    private static readonly GroundLeaseId LeaseId = GroundLeaseId.Generate();
    private static readonly Homeowner Buyer = new("Alice Johnson", "alice@example.com", "+12025551234");

    private static PurchaseWorkflow CreateWorkflow(decimal price = 250_000m)
    {
        return PurchaseWorkflow.Initiate(LeaseId, Buyer, price, StaffUser);
    }

    private static void SatisfyAllTracks(PurchaseWorkflow workflow)
    {
        workflow.RecordEligibilityVerified(new DateOnly(2025, 3, 1), "Income verified", StaffUser);
        workflow.RecordFinancingCommitted("First National", 240_000m, new DateOnly(2025, 3, 5), StaffUser);
        workflow.RecordInspectionCompleted(new DateOnly(2025, 3, 10), "Bob Inspector", "All clear", StaffUser);
        workflow.RecordTitleCleared(new DateOnly(2025, 3, 12), "Acme Title Co", StaffUser);
        workflow.RecordLeaseExecuted(new DateOnly(2025, 3, 15), StaffUser);
    }

    // --- Initiation ---

    [Fact]
    public void Initiate_ValidInputs_CreatesWorkflowInInitiatedPhase()
    {
        var workflow = CreateWorkflow();

        workflow.GroundLeaseId.Should().Be(LeaseId);
        workflow.Buyer.FullName.Should().Be("Alice Johnson");
        workflow.PurchasePrice.Should().Be(250_000m);
        workflow.Phase.Should().Be(WorkflowPhase.Initiated);
        workflow.InitiatedBy.Should().Be(StaffUser);
        workflow.IsTerminal.Should().BeFalse();
        workflow.AllMandatoryTracksSatisfied.Should().BeFalse();
    }

    [Fact]
    public void Initiate_ZeroPrice_Throws()
    {
        var act = () => PurchaseWorkflow.Initiate(LeaseId, Buyer, 0m, StaffUser);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Initiate_NegativePrice_Throws()
    {
        var act = () => PurchaseWorkflow.Initiate(LeaseId, Buyer, -1m, StaffUser);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Initiate_EmitsPurchaseWorkflowInitiatedEvent()
    {
        var workflow = CreateWorkflow();

        workflow.UncommittedEvents.Should().HaveCount(1);
        workflow.UncommittedEvents.First().Should().BeOfType<PurchaseWorkflowInitiated>();
    }

    // --- Track recording advances phase ---

    [Fact]
    public void RecordEligibilityVerified_FromInitiated_AdvancesToInProgress()
    {
        var workflow = CreateWorkflow();

        workflow.RecordEligibilityVerified(new DateOnly(2025, 3, 1), null, StaffUser);

        workflow.Phase.Should().Be(WorkflowPhase.InProgress);
        workflow.IsEligibilityVerified.Should().BeTrue();
        workflow.EligibilityDetails!.VerifiedDate.Should().Be(new DateOnly(2025, 3, 1));
    }

    [Fact]
    public void RecordFinancingCommitted_SetsDetails()
    {
        var workflow = CreateWorkflow();

        workflow.RecordFinancingCommitted("First National", 240_000m, new DateOnly(2025, 3, 5), StaffUser);

        workflow.IsFinancingCommitted.Should().BeTrue();
        workflow.FinancingDetails!.LenderName.Should().Be("First National");
        workflow.FinancingDetails.CommittedAmount.Should().Be(240_000m);
    }

    [Fact]
    public void RecordInspectionCompleted_SetsDetails()
    {
        var workflow = CreateWorkflow();

        workflow.RecordInspectionCompleted(new DateOnly(2025, 3, 10), "Bob Inspector", "All clear", StaffUser);

        workflow.IsInspectionCompleted.Should().BeTrue();
        workflow.InspectionResult!.InspectorName.Should().Be("Bob Inspector");
    }

    [Fact]
    public void RecordTitleCleared_SetsDetails()
    {
        var workflow = CreateWorkflow();

        workflow.RecordTitleCleared(new DateOnly(2025, 3, 12), "Acme Title Co", StaffUser);

        workflow.IsTitleCleared.Should().BeTrue();
        workflow.TitleDetails!.TitleCompanyName.Should().Be("Acme Title Co");
    }

    [Fact]
    public void RecordLeaseExecuted_SetsDetails()
    {
        var workflow = CreateWorkflow();

        workflow.RecordLeaseExecuted(new DateOnly(2025, 3, 15), StaffUser);

        workflow.IsLeaseExecuted.Should().BeTrue();
        workflow.LeaseExecutionDetails!.SignedDate.Should().Be(new DateOnly(2025, 3, 15));
    }

    // --- Parallel tracks ---

    [Fact]
    public void RecordAllTracks_InAnyOrder_AllTracksSatisfied()
    {
        var workflow = CreateWorkflow();

        // Record in non-sequential order to prove parallel independence
        workflow.RecordTitleCleared(new DateOnly(2025, 3, 12), null, StaffUser);
        workflow.RecordLeaseExecuted(new DateOnly(2025, 3, 15), StaffUser);
        workflow.RecordEligibilityVerified(new DateOnly(2025, 3, 1), null, StaffUser);
        workflow.RecordInspectionCompleted(new DateOnly(2025, 3, 10), "Inspector", null, StaffUser);
        workflow.RecordFinancingCommitted("Bank", 240_000m, new DateOnly(2025, 3, 5), StaffUser);

        workflow.AllMandatoryTracksSatisfied.Should().BeTrue();
        workflow.Phase.Should().Be(WorkflowPhase.InProgress);
    }

    // --- Idempotency ---

    [Fact]
    public void RecordTrack_Twice_IsIdempotent()
    {
        var workflow = CreateWorkflow();

        workflow.RecordEligibilityVerified(new DateOnly(2025, 3, 1), "First", StaffUser);
        var eventCountAfterFirst = workflow.UncommittedEvents.Count();

        workflow.RecordEligibilityVerified(new DateOnly(2025, 4, 1), "Second", StaffUser);

        workflow.UncommittedEvents.Count().Should().Be(eventCountAfterFirst);
        workflow.EligibilityDetails!.Notes.Should().Be("First");
    }

    // --- Closing ---

    [Fact]
    public void ScheduleClosing_WhenAllTracksSatisfied_Succeeds()
    {
        var workflow = CreateWorkflow();
        SatisfyAllTracks(workflow);

        workflow.ScheduleClosing(new DateOnly(2025, 4, 1), StaffUser);

        workflow.Phase.Should().Be(WorkflowPhase.ClosingScheduled);
        workflow.ScheduledClosingDate.Should().Be(new DateOnly(2025, 4, 1));
    }

    [Fact]
    public void ScheduleClosing_WhenTracksMissing_Throws()
    {
        var workflow = CreateWorkflow();
        workflow.RecordEligibilityVerified(new DateOnly(2025, 3, 1), null, StaffUser);

        var act = () => workflow.ScheduleClosing(new DateOnly(2025, 4, 1), StaffUser);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*mandatory tracks*");
    }

    [Fact]
    public void ScheduleClosing_WhenStalledAndAllTracksSatisfied_Succeeds()
    {
        var workflow = CreateWorkflow();
        SatisfyAllTracks(workflow);
        workflow.StallWorkflow("Waiting on docs", new DateOnly(2025, 3, 20), new DateOnly(2025, 4, 15), StaffUser);

        workflow.ScheduleClosing(new DateOnly(2025, 4, 1), StaffUser);

        workflow.Phase.Should().Be(WorkflowPhase.ClosingScheduled);
        workflow.ScheduledClosingDate.Should().Be(new DateOnly(2025, 4, 1));
    }

    [Fact]
    public void ScheduleClosing_WhenNotInProgress_Throws()
    {
        var workflow = CreateWorkflow();
        SatisfyAllTracks(workflow);
        workflow.ScheduleClosing(new DateOnly(2025, 4, 1), StaffUser);

        var act = () => workflow.ScheduleClosing(new DateOnly(2025, 4, 2), StaffUser);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*in progress*");
    }

    [Fact]
    public void RecordPurchaseClosed_WhenClosingScheduled_Succeeds()
    {
        var workflow = CreateWorkflow();
        SatisfyAllTracks(workflow);
        workflow.ScheduleClosing(new DateOnly(2025, 4, 1), StaffUser);

        workflow.RecordPurchaseClosed(new DateOnly(2025, 4, 1), "Smooth closing", StaffUser);

        workflow.Phase.Should().Be(WorkflowPhase.Closed);
        workflow.IsTerminal.Should().BeTrue();
        workflow.ClosingDetails!.ActualClosingDate.Should().Be(new DateOnly(2025, 4, 1));
        workflow.ClosingDetails.Notes.Should().Be("Smooth closing");
    }

    [Fact]
    public void RecordPurchaseClosed_WhenNotScheduled_Throws()
    {
        var workflow = CreateWorkflow();
        SatisfyAllTracks(workflow);

        var act = () => workflow.RecordPurchaseClosed(new DateOnly(2025, 4, 1), null, StaffUser);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*closing is scheduled*");
    }

    // --- Failure ---

    [Fact]
    public void FailPurchase_FromAnyNonTerminal_Succeeds()
    {
        var workflow = CreateWorkflow();

        workflow.FailPurchase("Buyer lost job", new DateOnly(2025, 3, 20), StaffUser);

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
        workflow.FailPurchase("First failure", new DateOnly(2025, 3, 20), StaffUser);

        var act = () => workflow.FailPurchase("Second attempt", new DateOnly(2025, 3, 21), StaffUser);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*terminal*");
    }

    // --- Withdrawal ---

    [Fact]
    public void WithdrawPurchase_FromAnyNonTerminal_Succeeds()
    {
        var workflow = CreateWorkflow();
        workflow.RecordEligibilityVerified(new DateOnly(2025, 3, 1), null, StaffUser);

        workflow.WithdrawPurchase(new DateOnly(2025, 3, 20), "Changed mind", StaffUser);

        workflow.Phase.Should().Be(WorkflowPhase.Withdrawn);
        workflow.IsTerminal.Should().BeTrue();
        workflow.WithdrawalReason.Should().Be("Changed mind");
    }

    // --- Stall/Resume ---

    [Fact]
    public void StallWorkflow_FromInProgress_Succeeds()
    {
        var workflow = CreateWorkflow();
        workflow.RecordEligibilityVerified(new DateOnly(2025, 3, 1), null, StaffUser);

        workflow.StallWorkflow("Waiting on appraisal", new DateOnly(2025, 3, 15), new DateOnly(2025, 4, 15), StaffUser);

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
            .WithMessage("*in-progress*");
    }

    [Fact]
    public void StallWorkflow_ThenRecordTrack_ResumesToInProgress()
    {
        var workflow = CreateWorkflow();
        workflow.RecordEligibilityVerified(new DateOnly(2025, 3, 1), null, StaffUser);
        workflow.StallWorkflow("Waiting", new DateOnly(2025, 3, 15), new DateOnly(2025, 4, 15), StaffUser);
        workflow.Phase.Should().Be(WorkflowPhase.Stalled);

        workflow.RecordFinancingCommitted("Bank", 240_000m, new DateOnly(2025, 3, 20), StaffUser);

        workflow.Phase.Should().Be(WorkflowPhase.InProgress);
    }

    // --- Terminal guard on tracks ---

    [Fact]
    public void RecordTrack_WhenTerminal_Throws()
    {
        var workflow = CreateWorkflow();
        workflow.FailPurchase("Deal fell through", new DateOnly(2025, 3, 20), StaffUser);

        var act = () => workflow.RecordEligibilityVerified(
            new DateOnly(2025, 3, 25), null, StaffUser);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*terminal*");
    }

    // --- Event data verification ---

    [Fact]
    public void EventsEmitted_ContainCorrectData()
    {
        var workflow = CreateWorkflow();

        var initiated = workflow.UncommittedEvents.OfType<PurchaseWorkflowInitiated>().Single();
        initiated.GroundLeaseId.Should().Be(LeaseId.Value);
        initiated.BuyerFullName.Should().Be("Alice Johnson");
        initiated.BuyerEmail.Should().Be("alice@example.com");
        initiated.BuyerPhone.Should().Be("+12025551234");
        initiated.PurchasePrice.Should().Be(250_000m);
    }
}
