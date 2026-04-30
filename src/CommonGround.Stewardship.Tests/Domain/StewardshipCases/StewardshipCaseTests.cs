using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Stewardship.Domain.StewardshipCases;
using CommonGround.Stewardship.Domain.StewardshipCases.Events;
using FluentAssertions;

namespace CommonGround.Stewardship.Tests.Domain.StewardshipCases;

public class StewardshipCaseTests
{
    private static readonly UserId Staff = UserId.Generate();
    private static readonly string GroundLeaseIdValue = "groundlease-01JRGV0000000000000000000A";
    private static readonly DateOnly Today = new(2026, 4, 24);

    private static StewardshipCase OpenCase() =>
        StewardshipCase.Open(GroundLeaseIdValue, Today, Staff);

    // --- Open ---

    [Fact]
    public void Open_ValidInputs_CreatesActiveCase()
    {
        var sc = OpenCase();

        sc.GroundLeaseId.Should().Be(GroundLeaseIdValue);
        sc.OpenedDate.Should().Be(Today);
        sc.Status.Should().Be(StewardshipCaseStatus.Active);
        sc.ClosedDate.Should().BeNull();
        sc.OccupancyVerifications.Should().BeEmpty();
        sc.PropertyInspections.Should().BeEmpty();
        sc.LeaseViolations.Should().BeEmpty();
        sc.UncommittedEvents.Should().ContainSingle(e => e is StewardshipCaseOpened);
    }

    [Fact]
    public void Open_EmptyGroundLeaseId_Throws()
    {
        var act = () => StewardshipCase.Open("", Today, Staff);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    // --- OccupancyVerification lifecycle ---

    [Fact]
    public void InitiateOccupancyVerification_ActiveCase_CreatesPendingVerification()
    {
        var sc = OpenCase();
        sc.InitiateOccupancyVerification(2026, new DateOnly(2026, 6, 1), Staff);

        sc.OccupancyVerifications.Should().ContainSingle();
        sc.OccupancyVerifications[0].PeriodYear.Should().Be(2026);
        sc.OccupancyVerifications[0].Status.Should().Be(OccupancyVerificationStatus.Pending);
    }

    [Fact]
    public void InitiateOccupancyVerification_ClosedCase_Throws()
    {
        var sc = OpenCase();
        sc.Close("Terminated", Today, Staff);

        var act = () => sc.InitiateOccupancyVerification(2026, null, Staff);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void SubmitOccupancyVerification_WhenPending_TransitionsToSubmitted()
    {
        var sc = OpenCase();
        sc.InitiateOccupancyVerification(2026, null, Staff);
        var id = sc.OccupancyVerifications[0].Id;

        sc.SubmitOccupancyVerification(id, true, false, null, Today);

        sc.OccupancyVerifications[0].Status.Should().Be(OccupancyVerificationStatus.Submitted);
        sc.OccupancyVerifications[0].IsPrimaryResident.Should().Be(true);
        sc.OccupancyVerifications[0].SubletDisclosed.Should().Be(false);
    }

    [Fact]
    public void SubmitOccupancyVerification_WhenOverdue_Accepted()
    {
        var sc = OpenCase();
        sc.InitiateOccupancyVerification(2026, null, Staff);
        var id = sc.OccupancyVerifications[0].Id;
        sc.MarkOccupancyVerificationOverdue(id, Today);

        sc.SubmitOccupancyVerification(id, true, false, null, Today);

        sc.OccupancyVerifications[0].Status.Should().Be(OccupancyVerificationStatus.Submitted);
    }

    [Fact]
    public void SubmitOccupancyVerification_WhenVerified_Throws()
    {
        var sc = OpenCase();
        sc.InitiateOccupancyVerification(2026, null, Staff);
        var id = sc.OccupancyVerifications[0].Id;
        sc.SubmitOccupancyVerification(id, true, false, null, Today);
        sc.VerifyOccupancy(id, Today, Staff);

        var act = () => sc.SubmitOccupancyVerification(id, true, false, null, Today);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void VerifyOccupancy_WhenSubmitted_TransitionsToVerified()
    {
        var sc = OpenCase();
        sc.InitiateOccupancyVerification(2026, null, Staff);
        var id = sc.OccupancyVerifications[0].Id;
        sc.SubmitOccupancyVerification(id, true, false, null, Today);

        sc.VerifyOccupancy(id, Today, Staff);

        sc.OccupancyVerifications[0].Status.Should().Be(OccupancyVerificationStatus.Verified);
        sc.OccupancyVerifications[0].DecisionDate.Should().Be(Today);
    }

    [Fact]
    public void VerifyOccupancy_WhenPending_Throws()
    {
        var sc = OpenCase();
        sc.InitiateOccupancyVerification(2026, null, Staff);
        var id = sc.OccupancyVerifications[0].Id;

        var act = () => sc.VerifyOccupancy(id, Today, Staff);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void FailOccupancy_WhenSubmitted_TransitionsToFailed()
    {
        var sc = OpenCase();
        sc.InitiateOccupancyVerification(2026, null, Staff);
        var id = sc.OccupancyVerifications[0].Id;
        sc.SubmitOccupancyVerification(id, true, false, null, Today);

        sc.FailOccupancy(id, "Not primary resident", Today, Staff);

        sc.OccupancyVerifications[0].Status.Should().Be(OccupancyVerificationStatus.Failed);
        sc.OccupancyVerifications[0].DecisionDate.Should().Be(Today);
    }

    [Fact]
    public void MarkOccupancyVerificationOverdue_WhenPending_TransitionsToOverdue()
    {
        var sc = OpenCase();
        sc.InitiateOccupancyVerification(2026, null, Staff);
        var id = sc.OccupancyVerifications[0].Id;

        sc.MarkOccupancyVerificationOverdue(id, Today);

        sc.OccupancyVerifications[0].Status.Should().Be(OccupancyVerificationStatus.Overdue);
    }

    [Fact]
    public void MarkOccupancyVerificationOverdue_WhenSubmitted_Throws()
    {
        var sc = OpenCase();
        sc.InitiateOccupancyVerification(2026, null, Staff);
        var id = sc.OccupancyVerifications[0].Id;
        sc.SubmitOccupancyVerification(id, true, false, null, Today);

        var act = () => sc.MarkOccupancyVerificationOverdue(id, Today);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    // --- PropertyInspection lifecycle ---

    [Fact]
    public void SchedulePropertyInspection_ActiveCase_CreatesScheduledInspection()
    {
        var sc = OpenCase();
        sc.SchedulePropertyInspection(Today, "Inspector Bob", Staff);

        sc.PropertyInspections.Should().ContainSingle();
        sc.PropertyInspections[0].Status.Should().Be(PropertyInspectionStatus.Scheduled);
        sc.PropertyInspections[0].InspectorName.Should().Be("Inspector Bob");
    }

    [Fact]
    public void CompletePropertyInspection_WhenScheduled_TransitionsToCompleted()
    {
        var sc = OpenCase();
        sc.SchedulePropertyInspection(Today, "Inspector Bob", Staff);
        var id = sc.PropertyInspections[0].Id;

        sc.CompletePropertyInspection(id, true, "All good", Today, Staff);

        sc.PropertyInspections[0].Status.Should().Be(PropertyInspectionStatus.Completed);
        sc.PropertyInspections[0].OccupancyConfirmed.Should().Be(true);
        sc.PropertyInspections[0].CompletedDate.Should().Be(Today);
    }

    [Fact]
    public void CancelPropertyInspection_WhenScheduled_TransitionsToCancelled()
    {
        var sc = OpenCase();
        sc.SchedulePropertyInspection(Today, "Inspector Bob", Staff);
        var id = sc.PropertyInspections[0].Id;

        sc.CancelPropertyInspection(id, "Homeowner unavailable", Staff);

        sc.PropertyInspections[0].Status.Should().Be(PropertyInspectionStatus.Cancelled);
    }

    [Fact]
    public void CompletePropertyInspection_WhenCancelled_Throws()
    {
        var sc = OpenCase();
        sc.SchedulePropertyInspection(Today, "Inspector Bob", Staff);
        var id = sc.PropertyInspections[0].Id;
        sc.CancelPropertyInspection(id, "Cancelled", Staff);

        var act = () => sc.CompletePropertyInspection(id, true, null, Today, Staff);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    // --- LeaseViolation lifecycle ---

    [Fact]
    public void IdentifyViolation_ActiveCase_CreatesIdentifiedViolation()
    {
        var sc = OpenCase();
        sc.IdentifyViolation(LeaseViolationType.Occupancy, "Sublet without disclosure", Today, Staff);

        sc.LeaseViolations.Should().ContainSingle();
        sc.LeaseViolations[0].Status.Should().Be(LeaseViolationStatus.Identified);
        sc.LeaseViolations[0].Type.Should().Be(LeaseViolationType.Occupancy);
    }

    [Fact]
    public void IssueViolationNotice_WhenIdentified_TransitionsToNoticeIssued()
    {
        var sc = OpenCase();
        sc.IdentifyViolation(LeaseViolationType.Occupancy, "Violation", Today, Staff);
        var id = sc.LeaseViolations[0].Id;

        sc.IssueViolationNotice(id, Today, Today.AddDays(30), Staff);

        sc.LeaseViolations[0].Status.Should().Be(LeaseViolationStatus.NoticeIssued);
        sc.LeaseViolations[0].CureDeadline.Should().Be(Today.AddDays(30));
    }

    [Fact]
    public void IssueViolationNotice_WhenEscalated_Throws()
    {
        var sc = OpenCase();
        sc.IdentifyViolation(LeaseViolationType.Occupancy, "Violation", Today, Staff);
        var id = sc.LeaseViolations[0].Id;
        sc.IssueViolationNotice(id, Today, Today.AddDays(30), Staff);
        sc.EscalateViolation(id, "No cure", Today, Staff);

        var act = () => sc.IssueViolationNotice(id, Today, Today.AddDays(30), Staff);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void CureViolation_WhenNoticeIssued_TransitionsToCured()
    {
        var sc = OpenCase();
        sc.IdentifyViolation(LeaseViolationType.Occupancy, "Violation", Today, Staff);
        var id = sc.LeaseViolations[0].Id;
        sc.IssueViolationNotice(id, Today, Today.AddDays(30), Staff);

        sc.CureViolation(id, Today.AddDays(10), Staff);

        sc.LeaseViolations[0].Status.Should().Be(LeaseViolationStatus.Cured);
        sc.LeaseViolations[0].ResolutionDate.Should().Be(Today.AddDays(10));
    }

    [Fact]
    public void EscalateViolation_WhenNoticeIssued_TransitionsToEscalated()
    {
        var sc = OpenCase();
        sc.IdentifyViolation(LeaseViolationType.Occupancy, "Violation", Today, Staff);
        var id = sc.LeaseViolations[0].Id;
        sc.IssueViolationNotice(id, Today, Today.AddDays(30), Staff);

        sc.EscalateViolation(id, "No cure attempted", Today.AddDays(31), Staff);

        sc.LeaseViolations[0].Status.Should().Be(LeaseViolationStatus.Escalated);
        sc.LeaseViolations[0].ResolutionNotes.Should().Be("No cure attempted");
    }

    [Fact]
    public void CureViolation_WhenIdentified_Throws()
    {
        var sc = OpenCase();
        sc.IdentifyViolation(LeaseViolationType.Occupancy, "Violation", Today, Staff);
        var id = sc.LeaseViolations[0].Id;

        var act = () => sc.CureViolation(id, Today, Staff);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    // --- Close ---

    [Fact]
    public void Close_ActiveCase_TransitionsToClosed()
    {
        var sc = OpenCase();
        sc.Close("Ground lease terminated", Today, Staff);

        sc.Status.Should().Be(StewardshipCaseStatus.Closed);
        sc.ClosedDate.Should().Be(Today);
        sc.UncommittedEvents.Should().Contain(e => e is StewardshipCaseClosed);
    }

    [Fact]
    public void Close_AlreadyClosed_Throws()
    {
        var sc = OpenCase();
        sc.Close("Reason", Today, Staff);

        var act = () => sc.Close("Reason again", Today, Staff);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void AnyMutation_AfterClosed_Throws()
    {
        var sc = OpenCase();
        sc.Close("Terminated", Today, Staff);

        ((Action)(() => sc.InitiateOccupancyVerification(2026, null, Staff)))
            .Should().Throw<BusinessRuleViolationException>();
        ((Action)(() => sc.SchedulePropertyInspection(Today, "Bob", Staff)))
            .Should().Throw<BusinessRuleViolationException>();
        ((Action)(() => sc.IdentifyViolation(LeaseViolationType.Occupancy, "X", Today, Staff)))
            .Should().Throw<BusinessRuleViolationException>();
    }
}
