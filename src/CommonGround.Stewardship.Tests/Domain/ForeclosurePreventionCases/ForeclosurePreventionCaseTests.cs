using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;
using CommonGround.Testing;
using FluentAssertions;

namespace CommonGround.Stewardship.Tests.Domain.ForeclosurePreventionCases;

public class ForeclosurePreventionCaseTests
{
    private static readonly UserId Staff = UserId.Generate();
    private static readonly string GroundLeaseIdValue = "groundlease-01JRGV0000000000000000000A";
    private static readonly DateOnly Today = new(2026, 4, 24);

    private static ForeclosurePreventionCase OpenCase(
        ForeclosurePreventionTrigger trigger = ForeclosurePreventionTrigger.LeaseFeeArrears) =>
        EventReplay.Replay<ForeclosurePreventionCase>(ForeclosurePreventionCase.Open(ForeclosurePreventionCaseId.Generate(), GroundLeaseIdValue, trigger, Today, Staff));

    [Fact]
    public void Open_WithLeaseFeeArrearsTrigger_CreatesOpenedCase()
    {
        var fpc = OpenCase(ForeclosurePreventionTrigger.LeaseFeeArrears);

        fpc.GroundLeaseId.Should().Be(GroundLeaseIdValue);
        fpc.Trigger.Should().Be(ForeclosurePreventionTrigger.LeaseFeeArrears);
        fpc.Status.Should().Be(ForeclosurePreventionCaseStatus.Opened);
        fpc.Outcome.Should().BeNull();
    }

    [Fact]
    public void Open_WithServicerDelinquencyTrigger_CreatesOpenedCase()
    {
        var fpc = OpenCase(ForeclosurePreventionTrigger.ServicerDelinquency);

        fpc.Trigger.Should().Be(ForeclosurePreventionTrigger.ServicerDelinquency);
        fpc.Status.Should().Be(ForeclosurePreventionCaseStatus.Opened);
    }

    [Fact]
    public void RecordServicerDelinquency_OpenedCase_RecordsFactWithoutAdvancing()
    {
        var fpc = OpenCase();
        var (decided, events) = fpc.Decide(a => a.RecordServicerDelinquency("First National Bank", 3, Today, Staff));
        fpc = decided;

        fpc.Status.Should().Be(ForeclosurePreventionCaseStatus.Opened);
        events.Should().ContainSingle().Which.Should().BeOfType<ServicerDelinquencyRecorded>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RecordServicerDelinquency_NonPositiveMonths_Throws(int months)
    {
        var fpc = OpenCase();

        var act = () => fpc.RecordServicerDelinquency("First National Bank", months, Today, Staff);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void MakeCounselingReferral_FromOpened_AdvancesToActive()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.MakeCounselingReferral("Housing Counselors Inc", Today, Staff)).Aggregate;

        fpc.Status.Should().Be(ForeclosurePreventionCaseStatus.Active);
        fpc.CounselingReferrals.Should().ContainSingle();
        fpc.CounselingReferrals[0].AgencyName.Should().Be("Housing Counselors Inc");
    }

    [Fact]
    public void MakeCounselingReferral_FromActive_StaysActive()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.MakeCounselingReferral("Agency One", Today, Staff)).Aggregate;
        fpc = fpc.Decide(a => a.MakeCounselingReferral("Agency Two", Today, Staff)).Aggregate;

        fpc.Status.Should().Be(ForeclosurePreventionCaseStatus.Active);
        fpc.CounselingReferrals.Should().HaveCount(2);
    }

    [Fact]
    public void RecordCounselingOutcome_WhenReferralExists_RecordsOutcome()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.MakeCounselingReferral("Housing Counselors Inc", Today, Staff)).Aggregate;
        var referralId = fpc.CounselingReferrals[0].Id;

        fpc = fpc.Decide(a => a.RecordCounselingOutcome(referralId, "Counseling completed successfully", Today.AddDays(30), Staff)).Aggregate;

        fpc.CounselingReferrals[0].Outcome.Should().Be("Counseling completed successfully");
        fpc.CounselingReferrals[0].OutcomeDate.Should().Be(Today.AddDays(30));
    }

    [Fact]
    public void RecordCounselingOutcome_WhenReferralNotFound_Throws()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.MakeCounselingReferral("Agency", Today, Staff)).Aggregate;

        var act = () => fpc.RecordCounselingOutcome(Guid.NewGuid(), "Outcome", Today, Staff);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void RequestForbearance_FromOpened_AdvancesToActive()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.RequestForbearance("First National Bank", Today, Staff)).Aggregate;

        fpc.Status.Should().Be(ForeclosurePreventionCaseStatus.Active);
        fpc.ForbearanceRequests.Should().ContainSingle();
        fpc.ForbearanceRequests[0].LenderName.Should().Be("First National Bank");
    }

    [Fact]
    public void RecordForbearanceOutcome_Granted_RecordsOutcome()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.RequestForbearance("Bank", Today, Staff)).Aggregate;
        var requestId = fpc.ForbearanceRequests[0].Id;

        fpc = fpc.Decide(a => a.RecordForbearanceOutcome(requestId, ForbearanceOutcome.Granted, Today.AddDays(14), "6-month agreement", Staff)).Aggregate;

        fpc.ForbearanceRequests[0].Outcome.Should().Be(ForbearanceOutcome.Granted);
        fpc.ForbearanceRequests[0].Notes.Should().Be("6-month agreement");
    }

    [Fact]
    public void RecordForbearanceOutcome_Denied_RecordsOutcome()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.RequestForbearance("Bank", Today, Staff)).Aggregate;
        var requestId = fpc.ForbearanceRequests[0].Id;

        fpc = fpc.Decide(a => a.RecordForbearanceOutcome(requestId, ForbearanceOutcome.Denied, Today.AddDays(7), null, Staff)).Aggregate;

        fpc.ForbearanceRequests[0].Outcome.Should().Be(ForbearanceOutcome.Denied);
    }

    [Fact]
    public void ActivateCurePeriod_FromOpened_AdvancesToActive()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.ActivateCurePeriod("lra-001", Today, 90, Staff)).Aggregate;

        fpc.Status.Should().Be(ForeclosurePreventionCaseStatus.Active);
        fpc.CurePeriodActivations.Should().ContainSingle();
        fpc.CurePeriodActivations[0].LraId.Should().Be("lra-001");
        fpc.CurePeriodActivations[0].CurePeriodDays.Should().Be(90);
        fpc.CurePeriodActivations[0].CureDeadline.Should().Be(Today.AddDays(90));
    }

    [Fact]
    public void Resolve_WhenActive_TransitionsToResolved()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.MakeCounselingReferral("Agency", Today, Staff)).Aggregate;

        var (decided, events) = fpc.Decide(a => a.Resolve(ForeclosurePreventionOutcome.InterventionSuccessful, Today.AddDays(60), Staff));
        fpc = decided;

        fpc.Status.Should().Be(ForeclosurePreventionCaseStatus.Resolved);
        fpc.Outcome.Should().Be(ForeclosurePreventionOutcome.InterventionSuccessful);
        fpc.ResolvedDate.Should().Be(Today.AddDays(60));
        events.Should().ContainSingle().Which.Should().BeOfType<ForeclosurePreventionCaseResolved>();
    }

    [Fact]
    public void Resolve_WhenOpened_Throws()
    {
        var fpc = OpenCase();

        var act = () => fpc.Resolve(ForeclosurePreventionOutcome.InterventionSuccessful, Today, Staff);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Resolve_WhenAlreadyResolved_Throws()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.MakeCounselingReferral("Agency", Today, Staff)).Aggregate;
        fpc = fpc.Decide(a => a.Resolve(ForeclosurePreventionOutcome.InterventionSuccessful, Today, Staff)).Aggregate;

        var act = () => fpc.Resolve(ForeclosurePreventionOutcome.HomeownerSold, Today, Staff);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void AnyIntervention_AfterResolved_Throws()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.MakeCounselingReferral("Agency", Today, Staff)).Aggregate;
        fpc = fpc.Decide(a => a.Resolve(ForeclosurePreventionOutcome.InterventionSuccessful, Today, Staff)).Aggregate;

        ((Action)(() => fpc.MakeCounselingReferral("Agency2", Today, Staff)))
            .Should().Throw<BusinessRuleViolationException>();
        ((Action)(() => fpc.RequestForbearance("Bank", Today, Staff)))
            .Should().Throw<BusinessRuleViolationException>();
        ((Action)(() => fpc.ActivateCurePeriod("lra-002", Today, 90, Staff)))
            .Should().Throw<BusinessRuleViolationException>();
        ((Action)(() => fpc.RecordServicerDelinquency("Bank", 3, Today, Staff)))
            .Should().Throw<BusinessRuleViolationException>();
    }
}
