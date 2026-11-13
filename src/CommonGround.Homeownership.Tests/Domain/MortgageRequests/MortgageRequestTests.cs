using System.Globalization;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.MortgageRequests;
using CommonGround.Homeownership.Domain.MortgageRequests.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.MortgageRequests;

public class MortgageRequestTests
{
    public class TestBed
    {
        public static readonly DateOnly LeaseStart = new(2025, 1, 1);

        /// <summary>A Monday.</summary>
        public static readonly DateOnly Today = new(2026, 6, 1);

        /// <summary>A Friday, 21 business days before <see cref="ExpectedClosing"/>.</summary>
        public static readonly DateOnly Received = new(2026, 5, 15);

        public static readonly DateOnly ExpectedClosing = new(2026, 6, 15);

        public UserId StaffUser { get; } = UserId.Generate();

        public MortgageRequestId RequestId { get; } = MortgageRequestId.Generate();

        /// <summary>
        /// Base price $200,000, original appraisal $250,000, 25% of appreciation, 90% debt cap. An appraisal of
        /// $330,000 gives a purchase option price of $220,000 and a cap of $198,000.
        /// </summary>
        public GroundLease Lease { get; }

        public TestBed()
        {
            Lease = EventReplay.Replay<GroundLease>(GroundLease.Establish(
                GroundLeaseId.Generate(), "land-parcel-1", "home-1", Homeowner.From("Dana Whitfield", "dana@example.com", null),
                LeaseStart, 99, 60, LeaseFeeTerms.Create(Money.Create(45m), Money.Create(15m)), MortgageDebtCap.Create(90),
                200_000m, 250_000m, "Covenant", ResalePolicyTerms.AppraisalBased(0.25m), HomeCommitments.None, StaffUser));
        }

        public static LoanAppraisal Appraisal { get; } = LoanAppraisal.Create(Money.Create(330_000m), new DateOnly(2026, 5, 10), null);

        public static MortgageLoanTerms Terms(decimal totalDebt = 180_000m) => MortgageLoanTerms.Create(
            "Columbia Bank", MortgagePurpose.Refinance, Money.Create(175_000m), Money.Create(totalDebt), 6.25m, MortgageRateType.Fixed, 30);

        public IReadOnlyList<IDomainEvent> Receipt(
            decimal totalDebt = 180_000m, LoanAppraisal? appraisal = null, MortgageRequestId? pendingOnLease = null) =>
            MortgageRequest.Receive(
                RequestId, Lease, pendingOnLease, Received, Terms(totalDebt), ExpectedClosing, Money.Create(3_000m), appraisal, null, Today, StaffUser);

        public MortgageRequest Pending(decimal totalDebt = 180_000m, LoanAppraisal? appraisal = null) =>
            EventReplay.Replay<MortgageRequest>([.. Receipt(totalDebt, appraisal)]);

        public MortgageRequest Appraised(decimal totalDebt = 180_000m) => Pending(totalDebt, Appraisal);

        public MortgageRequest Approved() => Appraised().Decide(r => r.Approve(Today, null, Today, StaffUser)).Aggregate;
    }

    public class TheReceiveMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenNoAppraisal_WhenReceived_ThenStartsAPendingRequestWithNoDebtCheck()
        {
            var events = _testBed.Receipt();

            events.Should().ContainSingle().Which.Should().BeOfType<MortgageRequestReceived>()
                .Which.Should().BeEquivalentTo(new { HomeownerFullName = "Dana Whitfield", HomeId = "home-1", Lender = "Columbia Bank" });
            var sut = EventReplay.Replay<MortgageRequest>([.. events]);
            sut.Should().BeEquivalentTo(new { Status = MortgageRequestStatus.Pending, ReceivedOn = TestBed.Received, DebtCheck = (MortgageDebtCheck?)null });
        }

        [Fact]
        public void GivenAppraisal_WhenReceived_ThenAlsoRecordsTheAppraisalWithItsDebtCheck()
        {
            var events = _testBed.Receipt(appraisal: TestBed.Appraisal);

            events.Should().HaveCount(2);
            events[1].Should().BeOfType<MortgageRequestAppraisalRecorded>().Which.Should().BeEquivalentTo(new
            {
                PurchaseOptionPrice = Money.Create(220_000m),
                MaxMortgageDebtPercent = 90,
                DebtCapAmount = Money.Create(198_000m),
                DebtPercent = 81.8m,
                ClosingCosts = Money.Create(3_000m),
            });
        }

        [Fact]
        public void GivenPendingRequestOnTheLease_WhenReceived_ThenThrowsBusinessRuleViolation()
        {
            var act = () => _testBed.Receipt(pendingOnLease: MortgageRequestId.Generate());

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.mortgage_request.receive.request_pending");
        }

        [Fact]
        public void GivenEndedLease_WhenReceived_ThenThrowsBusinessRuleViolation()
        {
            var ended = _testBed.Lease.Evolve(new GroundLeaseTerminated(_testBed.Lease.Id.Value, "Sold", TestBed.Received, _testBed.StaffUser.Value))!;

            var act = () => MortgageRequest.Receive(
                _testBed.RequestId, ended, null, TestBed.Received, TestBed.Terms(), TestBed.ExpectedClosing, null, null, null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.mortgage_request.receive.lease_not_active");
        }

        [Theory]
        [InlineData("2026-06-02", "2026-06-15", "homeownership.mortgage_request.receive.received_on_in_future")]
        [InlineData("2024-12-31", "2026-06-15", "homeownership.mortgage_request.receive.received_on_before_lease_start")]
        [InlineData("2026-05-15", "2026-05-14", "homeownership.mortgage_request.receive.expected_closing_before_received")]
        public void GivenInvalidDates_WhenReceived_ThenThrowsBusinessRuleViolation(string receivedOn, string expectedClosingOn, string expectedCode)
        {
            var act = () => MortgageRequest.Receive(
                _testBed.RequestId, _testBed.Lease, null, DateOnly.Parse(receivedOn, CultureInfo.InvariantCulture), TestBed.Terms(),
                DateOnly.Parse(expectedClosingOn, CultureInfo.InvariantCulture), null, null, null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Fact]
        public void GivenNegativeClosingCosts_WhenReceived_ThenThrowsBusinessRuleViolation()
        {
            var act = () => MortgageRequest.Receive(
                _testBed.RequestId, _testBed.Lease, null, TestBed.Received, TestBed.Terms(), TestBed.ExpectedClosing,
                Money.Create(-1m), null, null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.mortgage_request.closing_costs_negative");
        }
    }

    public class TheRecordAppraisalMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenAppraisedRequest_WhenAppraisedAgain_ThenTheLaterAppraisalReplacesTheDebtCheck()
        {
            var later = LoanAppraisal.Create(Money.Create(290_000m), new DateOnly(2026, 5, 20), null);

            var (sut, _) = _testBed.Appraised().Decide(r => r.RecordAppraisal(_testBed.Lease, later, null, TestBed.Today, _testBed.StaffUser));

            sut.DebtCheck!.PurchaseOptionPrice.Should().Be(Money.Create(210_000m));
            sut.ClosingCosts.Should().BeNull();
        }

        [Fact]
        public void GivenAppraisalDateInFuture_WhenAppraised_ThenThrowsBusinessRuleViolation()
        {
            var future = LoanAppraisal.Create(Money.Create(330_000m), TestBed.Today.AddDays(1), null);

            var act = () => _testBed.Pending().RecordAppraisal(_testBed.Lease, future, null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.mortgage_request.record_appraisal.appraisal_date_in_future");
        }

        [Fact]
        public void GivenApprovedRequest_WhenAppraised_ThenThrowsBusinessRuleViolation()
        {
            var act = () => _testBed.Approved().RecordAppraisal(_testBed.Lease, TestBed.Appraisal, null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.mortgage_request.record_appraisal.not_pending");
        }
    }

    public class TheApproveMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenDebtUnderTheCap_WhenApprovedWithoutReason_ThenKeepsTheDebtCheck()
        {
            var (sut, events) = _testBed.Appraised().Decide(r => r.Approve(TestBed.Today, null, TestBed.Today, _testBed.StaffUser));

            events.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                PurchaseOptionPrice = Money.Create(220_000m),
                DebtCapAmount = Money.Create(198_000m),
                TotalDebtAfterClosing = Money.Create(180_000m),
                DebtPercent = 81.8m,
                OverCap = false,
                Reason = (string?)null,
            });
            sut.Should().BeEquivalentTo(new { Status = MortgageRequestStatus.Approved, ApprovedOn = (DateOnly?)TestBed.Today });
        }

        [Fact]
        public void GivenDebtOverTheCap_WhenApprovedWithoutReason_ThenThrowsBusinessRuleViolation()
        {
            var act = () => _testBed.Appraised(totalDebt: 200_000m).Approve(TestBed.Today, " ", TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.mortgage_request.approve.over_cap_reason_required");
        }

        [Fact]
        public void GivenDebtOverTheCap_WhenApprovedWithReason_ThenRecordsItOverTheCap()
        {
            var approved = _testBed.Appraised(totalDebt: 200_000m)
                .Approve(TestBed.Today, " Lowers the first loan's payment ", TestBed.Today, _testBed.StaffUser);

            approved.Should().BeEquivalentTo(new { OverCap = true, DebtPercent = 90.9m, Reason = "Lowers the first loan's payment" });
        }

        [Fact]
        public void GivenNoAppraisal_WhenApproved_ThenThrowsBusinessRuleViolation()
        {
            var act = () => _testBed.Pending().Approve(TestBed.Today, null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.mortgage_request.approve.appraisal_required");
        }

        [Theory]
        [InlineData("2026-05-14", "homeownership.mortgage_request.approve.approved_on_before_received")]
        [InlineData("2026-06-02", "homeownership.mortgage_request.approve.approved_on_in_future")]
        public void GivenInvalidDate_WhenApproved_ThenThrowsBusinessRuleViolation(string approvedOn, string expectedCode)
        {
            var act = () => _testBed.Appraised().Approve(DateOnly.Parse(approvedOn, CultureInfo.InvariantCulture), null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Fact]
        public void GivenApprovedRequest_WhenApprovedAgain_ThenThrowsBusinessRuleViolation()
        {
            var act = () => _testBed.Approved().Approve(TestBed.Today, null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.mortgage_request.approve.not_pending");
        }
    }

    public class TheDenyMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenPendingRequest_WhenDenied_ThenItIsDeniedWithTheReason()
        {
            var (sut, events) = _testBed.Pending().Decide(r => r.Deny(TestBed.Today, " Cash out above the cap ", TestBed.Today, _testBed.StaffUser));

            events.Should().ContainSingle().Which.Should().BeEquivalentTo(new { Reason = "Cash out above the cap" });
            sut.Status.Should().Be(MortgageRequestStatus.Denied);
        }

        [Fact]
        public void GivenNoReason_WhenDenied_ThenThrowsBusinessRuleViolation()
        {
            var act = () => _testBed.Pending().Deny(TestBed.Today, "", TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.mortgage_request.deny.reason_required");
        }
    }

    public class TheRecordLoanClosedMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenApprovedRequest_WhenClosed_ThenNamesTheNewMortgageAndTheOnesPaidOff()
        {
            var newId = PermittedMortgageId.Generate();
            var paidOff = PermittedMortgageId.Generate();

            var (sut, events) = _testBed.Approved().Decide(r => r.RecordLoanClosed(TestBed.Today, newId, [paidOff, paidOff], TestBed.Today, _testBed.StaffUser));

            events.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                PermittedMortgageId = newId.Value,
                PaidOffMortgageIds = new[] { paidOff.Value },
            });
            sut.Status.Should().Be(MortgageRequestStatus.Closed);
        }

        [Fact]
        public void GivenPendingRequest_WhenClosed_ThenThrowsBusinessRuleViolation()
        {
            var act = () => _testBed.Appraised().RecordLoanClosed(TestBed.Today, PermittedMortgageId.Generate(), [], TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.mortgage_request.record_loan_closed.not_approved");
        }

        [Fact]
        public void GivenClosingBeforeTheApproval_WhenClosed_ThenThrowsBusinessRuleViolation()
        {
            var act = () => _testBed.Approved().RecordLoanClosed(TestBed.Today.AddDays(-1), PermittedMortgageId.Generate(), [], TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.mortgage_request.record_loan_closed.closed_on_before_approved");
        }
    }

    public class TheWithdrawMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenApprovedRequest_WhenWithdrawn_ThenItIsWithdrawn()
        {
            var (sut, _) = _testBed.Approved().Decide(r => r.Withdraw(TestBed.Today, "Rates went up", TestBed.Today, _testBed.StaffUser));

            sut.Status.Should().Be(MortgageRequestStatus.Withdrawn);
        }

        [Fact]
        public void GivenDeniedRequest_WhenWithdrawn_ThenThrowsBusinessRuleViolation()
        {
            var denied = _testBed.Pending().Decide(r => r.Deny(TestBed.Today, "No", TestBed.Today, _testBed.StaffUser)).Aggregate;

            var act = () => denied.Withdraw(TestBed.Today, "Rates went up", TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.mortgage_request.withdraw.not_open");
        }
    }

    public class TheBusinessDaysBetweenMethod
    {
        [Theory]
        [InlineData("2026-05-15", "2026-05-15", 0)]
        [InlineData("2026-05-15", "2026-05-17", 0)]
        [InlineData("2026-05-15", "2026-05-22", 5)]
        [InlineData("2026-05-15", "2026-06-15", 21)]
        public void GivenTwoDays_WhenCounted_ThenCountsWeekdaysAfterTheFirstUpToTheSecond(string from, string to, int expected)
        {
            MortgageRequest.BusinessDaysBetween(DateOnly.Parse(from, CultureInfo.InvariantCulture), DateOnly.Parse(to, CultureInfo.InvariantCulture))
                .Should().Be(expected);
        }
    }
}
