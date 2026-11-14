using System.Globalization;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.ImprovementRequests;
using CommonGround.Homeownership.Domain.ImprovementRequests.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.ImprovementRequests;

public class ImprovementRequestTests
{
    public class TestBed
    {
        public static readonly DateOnly LeaseStart = new(2025, 1, 1);
        public static readonly DateOnly Today = new(2026, 6, 1);
        public static readonly DateOnly Received = new(2026, 5, 15);

        public UserId StaffUser { get; } = UserId.Generate();

        public ImprovementRequestId RequestId { get; } = ImprovementRequestId.Generate();

        public GroundLease Lease { get; }

        public TestBed()
        {
            Lease = EventReplay.Replay<GroundLease>(GroundLease.Establish(
                GroundLeaseId.Generate(), "land-parcel-1", "home-1", Homeowner.From("Dana Whitfield", "dana@example.com", null),
                LeaseStart, 99, 60, LeaseFeeTerms.Create(Money.Create(45m), Money.Create(15m)), MortgageDebtCap.Create(90),
                200_000m, 250_000m, "Covenant", ResalePolicyTerms.AppraisalBased(0.25m), HomeCommitments.None, StaffUser));
        }

        public ImprovementRequestReceived Receipt(decimal estimatedCost = 20_000m) =>
            ImprovementRequest.Receive(RequestId, Lease, Received, " Replace the roof ", Money.Create(estimatedCost), Today, StaffUser);

        public ImprovementRequest Waiting() => EventReplay.Replay<ImprovementRequest>(Receipt());

        public ImprovementRequest Approved(decimal credit = 15_000m) =>
            Waiting().Decide(r => r.Approve(Today, Money.Create(credit), null, Money.Zero, null, Today, StaffUser)).Aggregate;
    }

    public class TheReceiveMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenValidRequest_WhenReceived_ThenStartsAWaitingRequestWithTheLeasesHome()
        {
            var received = _testBed.Receipt();

            received.Should().BeEquivalentTo(new
            {
                GroundLeaseId = _testBed.Lease.Id.Value,
                HomeId = "home-1",
                HomeownerFullName = "Dana Whitfield",
                Description = "Replace the roof",
                EstimatedCost = Money.Create(20_000m),
            });
            EventReplay.Replay<ImprovementRequest>(received).Should().BeEquivalentTo(new
            {
                Status = ImprovementRequestStatus.Waiting,
                ReceivedOn = TestBed.Received,
            });
        }

        [Fact]
        public void GivenEndedLease_WhenReceived_ThenThrowsBusinessRuleViolation()
        {
            var ended = _testBed.Lease.Evolve(new GroundLeaseTerminated(_testBed.Lease.Id.Value, "Sold", TestBed.Received, _testBed.StaffUser.Value))!;

            var act = () => ImprovementRequest.Receive(_testBed.RequestId, ended, TestBed.Received, "Roof", Money.Create(1m), TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.improvement_request.receive.lease_not_active");
        }

        [Theory]
        [InlineData("2026-06-02", "Roof", 100, "homeownership.improvement_request.receive.received_on_in_future")]
        [InlineData("2024-12-31", "Roof", 100, "homeownership.improvement_request.receive.received_on_before_lease_start")]
        [InlineData("2026-05-15", " ", 100, "homeownership.improvement_request.receive.description_required")]
        [InlineData("2026-05-15", "Roof", 0, "homeownership.improvement_request.receive.estimated_cost_not_positive")]
        public void GivenInvalidInput_WhenReceived_ThenThrowsBusinessRuleViolation(string receivedOn, string description, int cost, string expectedCode)
        {
            var act = () => ImprovementRequest.Receive(
                _testBed.RequestId, _testBed.Lease, DateOnly.Parse(receivedOn, CultureInfo.InvariantCulture), description,
                Money.Create(cost), TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }
    }

    public class TheApproveMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenWaitingRequest_WhenApproved_ThenKeepsTheCapAndTheCreditAlreadyOnTheLease()
        {
            var (sut, events) = _testBed.Waiting().Decide(r => r.Approve(
                TestBed.Today, Money.Create(15_000m), Money.Create(20_000m), Money.Create(8_000m), "Roof only", TestBed.Today, _testBed.StaffUser));

            events.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                PreApprovedCredit = Money.Create(15_000m),
                LeaseImprovementsCap = Money.Create(20_000m),
                CreditOnLeaseBefore = Money.Create(8_000m),
                Notes = "Roof only",
            });
            sut.Should().BeEquivalentTo(new { Status = ImprovementRequestStatus.Approved, PreApprovedCredit = Money.Create(15_000m) });
        }

        [Fact]
        public void GivenCreditPastTheCap_WhenApproved_ThenStillApproves()
        {
            var approved = _testBed.Waiting().Decide(r => r.Approve(
                TestBed.Today, Money.Create(15_000m), Money.Create(20_000m), Money.Create(18_000m), null, TestBed.Today, _testBed.StaffUser)).Aggregate;

            approved.Status.Should().Be(ImprovementRequestStatus.Approved);
        }

        [Fact]
        public void GivenZeroCredit_WhenApproved_ThenApprovesTheWorkCountingNothing()
        {
            _testBed.Approved(credit: 0m).PreApprovedCredit.Should().Be(Money.Zero);
        }

        [Theory]
        [InlineData(20_001, "2026-06-01", "homeownership.improvement_request.approve.credit_out_of_range")]
        [InlineData(-1, "2026-06-01", "homeownership.improvement_request.approve.credit_out_of_range")]
        [InlineData(100, "2026-05-14", "homeownership.improvement_request.approve.approved_on_before_received")]
        [InlineData(100, "2026-06-02", "homeownership.improvement_request.approve.approved_on_in_future")]
        public void GivenInvalidInput_WhenApproved_ThenThrowsBusinessRuleViolation(int credit, string approvedOn, string expectedCode)
        {
            var sut = _testBed.Waiting();

            var act = () => sut.Approve(
                DateOnly.Parse(approvedOn, CultureInfo.InvariantCulture), Money.Create(credit), null, Money.Zero, null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Fact]
        public void GivenApprovedRequest_WhenApprovedAgain_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Approved();

            var act = () => sut.Approve(TestBed.Today, Money.Zero, null, Money.Zero, null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.improvement_request.approve.not_waiting");
        }
    }

    public class TheDenyMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenWaitingRequest_WhenDenied_ThenItIsDeniedWithTheReason()
        {
            var (sut, events) = _testBed.Waiting().Decide(r => r.Deny(TestBed.Today, " Not allowed by the covenant ", TestBed.Today, _testBed.StaffUser));

            events.Should().ContainSingle().Which.Should().BeOfType<ImprovementRequestDenied>()
                .Which.Reason.Should().Be("Not allowed by the covenant");
            sut.Status.Should().Be(ImprovementRequestStatus.Denied);
        }

        [Fact]
        public void GivenNoReason_WhenDenied_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Waiting();

            var act = () => sut.Deny(TestBed.Today, " ", TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.improvement_request.deny.reason_required");
        }

        [Fact]
        public void GivenApprovedRequest_WhenDenied_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Approved();

            var act = () => sut.Deny(TestBed.Today, "No", TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.improvement_request.deny.not_waiting");
        }
    }

    public class TheRecordCompletedMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenApprovedRequest_WhenCompleted_ThenTheConfirmedCreditIsRecorded()
        {
            var (sut, events) = _testBed.Approved().Decide(r => r.RecordCompleted(
                TestBed.Today, Money.Create(18_000m), ["doc-1", "doc-1", "doc-2"], Money.Create(15_000m), null, TestBed.Today, _testBed.StaffUser));

            events.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                ActualCost = Money.Create(18_000m),
                ReceiptDocumentIds = new[] { "doc-1", "doc-2" },
                ConfirmedCredit = Money.Create(15_000m),
            });
            sut.Status.Should().Be(ImprovementRequestStatus.Completed);
        }

        [Fact]
        public void GivenWaitingRequest_WhenCompleted_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Waiting();

            var act = () => sut.RecordCompleted(TestBed.Today, Money.Create(1m), ["doc-1"], Money.Zero, null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.improvement_request.record_completed.not_approved");
        }

        [Fact]
        public void GivenNoReceipts_WhenCompleted_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Approved();

            var act = () => sut.RecordCompleted(TestBed.Today, Money.Create(1m), [], Money.Zero, null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.improvement_request.record_completed.receipts_required");
        }

        [Theory]
        [InlineData(0, 0, "2026-06-01", "homeownership.improvement_request.record_completed.actual_cost_not_positive")]
        [InlineData(100, 15_001, "2026-06-01", "homeownership.improvement_request.record_completed.credit_out_of_range")]
        [InlineData(100, -1, "2026-06-01", "homeownership.improvement_request.record_completed.credit_out_of_range")]
        [InlineData(100, 0, "2026-05-31", "homeownership.improvement_request.record_completed.completed_on_before_approved")]
        [InlineData(100, 0, "2026-06-02", "homeownership.improvement_request.record_completed.completed_on_in_future")]
        public void GivenInvalidInput_WhenCompleted_ThenThrowsBusinessRuleViolation(int actualCost, int credit, string completedOn, string expectedCode)
        {
            var sut = _testBed.Approved();

            var act = () => sut.RecordCompleted(
                DateOnly.Parse(completedOn, CultureInfo.InvariantCulture), Money.Create(actualCost), ["doc-1"], Money.Create(credit), null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }
    }

    public class TheWithdrawMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenWaitingRequest_WhenWithdrawn_ThenItIsWithdrawn()
        {
            var (sut, _) = _testBed.Waiting().Decide(r => r.Withdraw(TestBed.Today, "Changed plans", TestBed.Today, _testBed.StaffUser));

            sut.Status.Should().Be(ImprovementRequestStatus.Withdrawn);
        }

        [Fact]
        public void GivenApprovedRequest_WhenWithdrawn_ThenItIsWithdrawn()
        {
            var (sut, _) = _testBed.Approved().Decide(r => r.Withdraw(TestBed.Today, "Changed plans", TestBed.Today, _testBed.StaffUser));

            sut.Status.Should().Be(ImprovementRequestStatus.Withdrawn);
        }

        [Fact]
        public void GivenCompletedRequest_WhenWithdrawn_ThenThrowsBusinessRuleViolation()
        {
            var completed = _testBed.Approved().Decide(r => r.RecordCompleted(
                TestBed.Today, Money.Create(1m), ["doc-1"], Money.Zero, null, TestBed.Today, _testBed.StaffUser)).Aggregate;

            var act = () => completed.Withdraw(TestBed.Today, "Changed plans", TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.improvement_request.withdraw.not_open");
        }

        [Fact]
        public void GivenNoReason_WhenWithdrawn_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Waiting();

            var act = () => sut.Withdraw(TestBed.Today, "", TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.improvement_request.withdraw.reason_required");
        }
    }
}
