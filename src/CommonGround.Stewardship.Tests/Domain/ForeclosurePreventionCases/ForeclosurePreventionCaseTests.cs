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
    private static readonly DateOnly Now = Today.AddDays(90);

    private static ForeclosurePreventionCase OpenCase(
        ForeclosurePreventionTrigger trigger = ForeclosurePreventionTrigger.LeaseFeeArrears) =>
        EventReplay.Replay<ForeclosurePreventionCase>(ForeclosurePreventionCase.Open(ForeclosurePreventionCaseId.Generate(), GroundLeaseIdValue, trigger, Today, unresolvedCaseId: null, Staff));

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
        var (decided, events) = fpc.Decide(a => a.RecordServicerDelinquency("First National Bank", 3, Today, Now, Staff));
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

        var act = () => fpc.RecordServicerDelinquency("First National Bank", months, Today, Now, Staff);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void MakeCounselingReferral_FromOpened_AdvancesToActive()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.MakeCounselingReferral("Housing Counselors Inc", Today, Now, Staff)).Aggregate;

        fpc.Status.Should().Be(ForeclosurePreventionCaseStatus.Active);
        fpc.CounselingReferrals.Should().ContainSingle();
        fpc.CounselingReferrals[0].AgencyName.Should().Be("Housing Counselors Inc");
    }

    [Fact]
    public void MakeCounselingReferral_FromActive_StaysActive()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.MakeCounselingReferral("Agency One", Today, Now, Staff)).Aggregate;
        fpc = fpc.Decide(a => a.MakeCounselingReferral("Agency Two", Today, Now, Staff)).Aggregate;

        fpc.Status.Should().Be(ForeclosurePreventionCaseStatus.Active);
        fpc.CounselingReferrals.Should().HaveCount(2);
    }

    [Fact]
    public void RecordCounselingOutcome_WhenReferralExists_RecordsOutcome()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.MakeCounselingReferral("Housing Counselors Inc", Today, Now, Staff)).Aggregate;
        var referralId = fpc.CounselingReferrals[0].Id;

        fpc = fpc.Decide(a => a.RecordCounselingOutcome(referralId, "Counseling completed successfully", Today.AddDays(30), Now, Staff)).Aggregate;

        fpc.CounselingReferrals[0].Outcome.Should().Be("Counseling completed successfully");
        fpc.CounselingReferrals[0].OutcomeDate.Should().Be(Today.AddDays(30));
    }

    [Fact]
    public void RecordCounselingOutcome_WhenReferralNotFound_Throws()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.MakeCounselingReferral("Agency", Today, Now, Staff)).Aggregate;

        var act = () => fpc.RecordCounselingOutcome(Guid.NewGuid(), "Outcome", Today, Now, Staff);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void RequestForbearance_FromOpened_AdvancesToActive()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.RequestForbearance("First National Bank", Today, Now, Staff)).Aggregate;

        fpc.Status.Should().Be(ForeclosurePreventionCaseStatus.Active);
        fpc.ForbearanceRequests.Should().ContainSingle();
        fpc.ForbearanceRequests[0].LenderName.Should().Be("First National Bank");
    }

    [Fact]
    public void RecordForbearanceOutcome_Granted_RecordsOutcome()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.RequestForbearance("Bank", Today, Now, Staff)).Aggregate;
        var requestId = fpc.ForbearanceRequests[0].Id;

        fpc = fpc.Decide(a => a.RecordForbearanceOutcome(requestId, ForbearanceOutcome.Granted, Today.AddDays(14), "6-month agreement", Now, Staff)).Aggregate;

        fpc.ForbearanceRequests[0].Outcome.Should().Be(ForbearanceOutcome.Granted);
        fpc.ForbearanceRequests[0].Notes.Should().Be("6-month agreement");
    }

    [Fact]
    public void RecordForbearanceOutcome_Denied_RecordsOutcome()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.RequestForbearance("Bank", Today, Now, Staff)).Aggregate;
        var requestId = fpc.ForbearanceRequests[0].Id;

        fpc = fpc.Decide(a => a.RecordForbearanceOutcome(requestId, ForbearanceOutcome.Denied, Today.AddDays(7), null, Now, Staff)).Aggregate;

        fpc.ForbearanceRequests[0].Outcome.Should().Be(ForbearanceOutcome.Denied);
    }

    [Fact]
    public void ActivateCurePeriod_FromOpened_AdvancesToActive()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.ActivateCurePeriod("lra-001", Today, 90, Now, Staff)).Aggregate;

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
        fpc = fpc.Decide(a => a.MakeCounselingReferral("Agency", Today, Now, Staff)).Aggregate;

        var (decided, events) = fpc.Decide(a => a.Resolve(ForeclosurePreventionOutcome.InterventionSuccessful, Today.AddDays(60), Now, Staff));
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

        var act = () => fpc.Resolve(ForeclosurePreventionOutcome.InterventionSuccessful, Today, Now, Staff);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Resolve_WhenAlreadyResolved_Throws()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.MakeCounselingReferral("Agency", Today, Now, Staff)).Aggregate;
        fpc = fpc.Decide(a => a.Resolve(ForeclosurePreventionOutcome.InterventionSuccessful, Today, Now, Staff)).Aggregate;

        var act = () => fpc.Resolve(ForeclosurePreventionOutcome.HomeownerSold, Today, Now, Staff);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void AnyIntervention_AfterResolved_Throws()
    {
        var fpc = OpenCase();
        fpc = fpc.Decide(a => a.MakeCounselingReferral("Agency", Today, Now, Staff)).Aggregate;
        fpc = fpc.Decide(a => a.Resolve(ForeclosurePreventionOutcome.InterventionSuccessful, Today, Now, Staff)).Aggregate;

        ((Action)(() => fpc.MakeCounselingReferral("Agency2", Today, Now, Staff)))
            .Should().Throw<BusinessRuleViolationException>();
        ((Action)(() => fpc.RequestForbearance("Bank", Today, Now, Staff)))
            .Should().Throw<BusinessRuleViolationException>();
        ((Action)(() => fpc.ActivateCurePeriod("lra-002", Today, 90, Now, Staff)))
            .Should().Throw<BusinessRuleViolationException>();
        ((Action)(() => fpc.RecordServicerDelinquency("Bank", 3, Today, Now, Staff)))
            .Should().Throw<BusinessRuleViolationException>();
    }

    public class TestBed
    {
        public UserId Staff { get; } = UserId.Generate();
        public string GroundLeaseId { get; } = "groundlease-01JRGV0000000000000000000A";
        public DateOnly Today { get; } = new(2026, 4, 24);
        public ForeclosurePreventionCaseId CaseId { get; } = ForeclosurePreventionCaseId.Generate();

        public LeaseDefaultNotice UnpaidDefault() =>
            LeaseDefaultNotice.Create(new DateOnly(2026, 3, 1), Money.Create(450m), new DateOnly(2026, 3, 31), Money.Create(300m));
    }

    public class TheOpenMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenTheLeaseHasAnUnresolvedCase_WhenOpened_ThenRefusesWithCaseAlreadyOpen()
        {
            var act = () => ForeclosurePreventionCase.Open(
                _testBed.CaseId, _testBed.GroundLeaseId, ForeclosurePreventionTrigger.ServicerDelinquency, _testBed.Today,
                unresolvedCaseId: ForeclosurePreventionCaseId.Generate(), _testBed.Staff);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("stewardship.foreclosure_prevention_case.open.case_already_open");
        }
    }

    public class TheOpenForLeaseDefaultMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenAnUnpaidDefault_WhenOpened_ThenOpensALeaseFeeArrearsCaseAndRecordsTheNoticeFigures()
        {
            var events = ForeclosurePreventionCase.OpenForLeaseDefault(
                _testBed.CaseId, _testBed.GroundLeaseId, _testBed.UnpaidDefault(), _testBed.Today, unresolvedCaseId: null, _testBed.Staff);

            events.Should().SatisfyRespectively(
                first => first.Should().BeEquivalentTo(new { Trigger = ForeclosurePreventionTrigger.LeaseFeeArrears, OpenedDate = _testBed.Today }),
                second => second.Should().BeEquivalentTo(new
                {
                    AggregateId = _testBed.CaseId.Value,
                    NoticeSentOn = new DateOnly(2026, 3, 1),
                    AmountOwed = 450m,
                    CureBy = new DateOnly(2026, 3, 31),
                    StillToPay = 300m,
                    RecordedDate = _testBed.Today,
                    RecordedBy = _testBed.Staff.Value,
                }));
            EventReplay.Replay<ForeclosurePreventionCase>(events).LeaseDefault.Should().Be(_testBed.UnpaidDefault());
        }

        [Fact]
        public void GivenNoUnpaidDefault_WhenOpened_ThenRefusesWithNoUnpaidDefault()
        {
            var act = () => ForeclosurePreventionCase.OpenForLeaseDefault(
                _testBed.CaseId, _testBed.GroundLeaseId, unpaidDefault: null, _testBed.Today, unresolvedCaseId: null, _testBed.Staff);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("stewardship.foreclosure_prevention_case.open_for_lease_default.no_unpaid_default");
        }

        [Fact]
        public void GivenTheLeaseHasAnUnresolvedCase_WhenOpened_ThenRefusesWithCaseAlreadyOpen()
        {
            var act = () => ForeclosurePreventionCase.OpenForLeaseDefault(
                _testBed.CaseId, _testBed.GroundLeaseId, _testBed.UnpaidDefault(), _testBed.Today,
                unresolvedCaseId: ForeclosurePreventionCaseId.Generate(), _testBed.Staff);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("stewardship.foreclosure_prevention_case.open.case_already_open");
        }
    }
    public class TheDateChecks
    {
        private static ForeclosurePreventionCase ActiveCase()
        {
            var fpc = OpenCase();
            return fpc.Decide(a => a.MakeCounselingReferral("Agency", Today, Now, Staff)).Aggregate;
        }

        public static TheoryData<string, Func<ForeclosurePreventionCase, DateOnly, object>> Commands => new()
        {
            { "servicer delinquency", (f, d) => f.RecordServicerDelinquency("Bank", 3, d, Now, Staff) },
            { "counseling referral", (f, d) => f.MakeCounselingReferral("Agency", d, Now, Staff) },
            { "forbearance request", (f, d) => f.RequestForbearance("Bank", d, Now, Staff) },
            { "cure period", (f, d) => f.ActivateCurePeriod("lra-1", d, 90, Now, Staff) },
            { "resolve", (f, d) => f.Resolve(ForeclosurePreventionOutcome.InterventionSuccessful, d, Now, Staff) },
        };

        [Theory]
        [MemberData(nameof(Commands))]
        public void ADateAfterTodayIsRejected(string name, Func<ForeclosurePreventionCase, DateOnly, object> command)
        {
            var fpc = ActiveCase();

            var act = () => command(fpc, Now.AddDays(1));

            act.Should().Throw<BusinessRuleViolationException>(name)
                .Which.Code.Should().Be("stewardship.foreclosure_prevention_case.guard_date.in_future");
        }

        [Theory]
        [MemberData(nameof(Commands))]
        public void ADateBeforeTheCaseOpenedIsRejected(string name, Func<ForeclosurePreventionCase, DateOnly, object> command)
        {
            var fpc = ActiveCase();

            var act = () => command(fpc, Today.AddDays(-1));

            act.Should().Throw<BusinessRuleViolationException>(name)
                .Which.Code.Should().Be("stewardship.foreclosure_prevention_case.guard_date.before_case_opened");
        }

        [Theory]
        [MemberData(nameof(Commands))]
        public void TodayAndTheOpenedDateAreAccepted(string name, Func<ForeclosurePreventionCase, DateOnly, object> command)
        {
            var fpc = ActiveCase();

            command(fpc, Now).Should().NotBeNull(name);
            command(fpc, Today).Should().NotBeNull(name);
        }
    }
}
