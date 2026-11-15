using System.Globalization;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.HeirTransfers;
using CommonGround.Homeownership.Domain.HeirTransfers.Events;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.HeirTransfers;

public class HeirTransferTests
{
    public class TestBed
    {
        public static readonly DateOnly LeaseStart = new(2020, 1, 1);
        public static readonly DateOnly Today = new(2026, 6, 1);
        public static readonly DateOnly DiedOn = new(2026, 3, 10);
        public static readonly DateOnly NoticeOn = new(2026, 4, 1);

        public UserId StaffUser { get; } = UserId.Generate();

        public HeirTransferId TransferId { get; } = HeirTransferId.Generate();

        public GroundLease Lease { get; }

        /// <summary>The 80% of AMI level of a schedule: 1 person $50,000 up to 8 people $92,000.</summary>
        public HeirIncomeLimit Limit { get; } = new(
            80,
            IncomeLimitTable.Create(
                new DateOnly(2026, 1, 1),
                [IncomeLimitLevel.Create(80, [.. new[] { 50_000m, 57_000m, 64_000m, 71_000m, 76_000m, 82_000m, 87_000m, 92_000m }.Select(Money.Create)])]));

        public TestBed()
        {
            Lease = EventReplay.Replay<GroundLease>(GroundLease.Establish(
                GroundLeaseId.Generate(), "land-parcel-1", "home-1", Homeowner.From("Dana Whitfield", "dana@example.com", "415-555-0100"),
                LeaseStart, 99, 60, LeaseFeeTerms.Create(Money.Create(45m), Money.Create(15m)), MortgageDebtCap.Create(90),
                Money.Create(200_000m), Money.Create(250_000m), "Covenant", ResalePolicyTerms.AppraisalBased(0.25m), HomeCommitments.None, StaffUser));
        }

        public HeirTransferOpened Opening(DateOnly? noticeOn = null) =>
            HeirTransfer.Open(TransferId, Lease, null, DiedOn, noticeOn ?? NoticeOn, " Pat Whitfield ", " 415-555-0101 ", null, Today, StaffUser);

        public HeirTransfer Open() => EventReplay.Replay<HeirTransfer>(Opening());

        public HeirTransfer WithHeir(HeirRelationship? relationship = null) =>
            Open().Decide(t => RecordSpouse(t, relationship ?? HeirRelationship.Spouse)).Aggregate;

        public HeirRecorded RecordSpouse(HeirTransfer transfer, HeirRelationship relationship) =>
            transfer.RecordHeir("Sam Whitfield", "sam@example.com", "415-555-0102", relationship, null, null, null, null, null, StaffUser);

        public HeirTransferCompleted Complete(HeirTransfer transfer) =>
            transfer.Complete(new DateOnly(2026, 5, 20), "document-1", "document-2", " Signed in person ", Today, StaffUser);
    }

    public class TheOpenMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenValidNotice_WhenOpened_ThenStartsAnOpenTransferNamingTheDeceasedHomeowner()
        {
            var opened = _testBed.Opening();

            opened.Should().BeEquivalentTo(new
            {
                GroundLeaseId = _testBed.Lease.Id.Value,
                HomeId = "home-1",
                DeceasedHomeownerFullName = "Dana Whitfield",
                NotifiedBy = "Pat Whitfield",
                NotifiedByContact = "415-555-0101",
            });
            EventReplay.Replay<HeirTransfer>(opened).Should().BeEquivalentTo(new
            {
                Status = HeirTransferStatus.Open,
                DiedOn = TestBed.DiedOn,
                NoticeReceivedOn = TestBed.NoticeOn,
                NoticeWasLate = false,
            });
        }

        [Theory]
        [InlineData("2026-06-08", false)]
        [InlineData("2026-06-09", true)]
        public void GivenNoticeNearNinetyDays_WhenOpened_ThenIsLateOnlyAfterTheNinetiethDay(string noticeOn, bool late)
        {
            var today = new DateOnly(2026, 7, 1);
            var opened = HeirTransfer.Open(
                _testBed.TransferId, _testBed.Lease, null, new DateOnly(2026, 3, 10), DateOnly.Parse(noticeOn, CultureInfo.InvariantCulture),
                "Pat", null, null, today, _testBed.StaffUser);

            EventReplay.Replay<HeirTransfer>(opened).NoticeWasLate.Should().Be(late);
        }

        [Fact]
        public void GivenEndedLease_WhenOpened_ThenThrowsBusinessRuleViolation()
        {
            var ended = _testBed.Lease.Evolve(new GroundLeaseTerminated(_testBed.Lease.Id.Value, "Sold", TestBed.DiedOn, _testBed.StaffUser.Value))!;

            var act = () => HeirTransfer.Open(_testBed.TransferId, ended, null, TestBed.DiedOn, TestBed.NoticeOn, "Pat", null, null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("homeownership.heir_transfer.open.lease_not_active");
        }

        [Fact]
        public void GivenLeaseWithAnOpenTransfer_WhenOpened_ThenThrowsBusinessRuleViolation()
        {
            var act = () => HeirTransfer.Open(
                _testBed.TransferId, _testBed.Lease, HeirTransferId.Generate(), TestBed.DiedOn, TestBed.NoticeOn, "Pat", null, null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("homeownership.heir_transfer.open.already_open");
        }

        [Theory]
        [InlineData("2026-06-02", "2026-06-02", "Pat", "homeownership.heir_transfer.open.died_on_in_future")]
        [InlineData("2026-03-10", "2026-03-09", "Pat", "homeownership.heir_transfer.open.notice_before_death")]
        [InlineData("2026-03-10", "2026-06-02", "Pat", "homeownership.heir_transfer.open.notice_in_future")]
        [InlineData("2026-03-10", "2026-04-01", " ", "homeownership.heir_transfer.open.notified_by_required")]
        public void GivenInvalidInput_WhenOpened_ThenThrowsBusinessRuleViolation(string diedOn, string noticeOn, string notifiedBy, string expectedCode)
        {
            var act = () => HeirTransfer.Open(
                _testBed.TransferId, _testBed.Lease, null, DateOnly.Parse(diedOn, CultureInfo.InvariantCulture),
                DateOnly.Parse(noticeOn, CultureInfo.InvariantCulture), notifiedBy, null, null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }
    }

    public class TheRecordHeirMethod
    {
        private readonly TestBed _testBed = new();

        [Theory]
        [MemberData(nameof(FamilyRelationships))]
        public void GivenSpouseOrChild_WhenRecorded_ThenNeedsNothingMore(string relationshipName)
        {
            var relationship = HeirRelationship.FromDisplayName(relationshipName);

            var (sut, events) = _testBed.Open().Decide(t => _testBed.RecordSpouse(t, relationship));

            events.Should().ContainSingle().Which.Should().BeEquivalentTo(new { FullName = "Sam Whitfield", Email = "sam@example.com", Relationship = relationship });
            sut.Should().BeEquivalentTo(new { Status = HeirTransferStatus.HeirRecorded, Relationship = relationship });
        }

        public static TheoryData<string> FamilyRelationships() => new() { "Spouse", "Child" };

        [Fact]
        public void GivenHouseholdMemberAYearBeforeTheDeath_WhenRecorded_ThenRecordsTheMoveInDate()
        {
            var movedIn = TestBed.DiedOn.AddYears(-1);

            var recorded = _testBed.Open().RecordHeir(
                "Sam Whitfield", "sam@example.com", null, HeirRelationship.HouseholdMember, movedIn, null, null, null, null, _testBed.StaffUser);

            recorded.MovedInOn.Should().Be(movedIn);
        }

        [Theory]
        [InlineData("2025-03-11")]
        [InlineData("2026-03-11")]
        public void GivenHouseholdMemberUnderAYear_WhenRecorded_ThenThrowsBusinessRuleViolation(string movedIn)
        {
            var act = () => _testBed.Open().RecordHeir(
                "Sam", "sam@example.com", null, HeirRelationship.HouseholdMember, DateOnly.Parse(movedIn, CultureInfo.InvariantCulture),
                null, null, null, null, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("homeownership.heir_transfer.record_heir.household_member_under_one_year");
        }

        [Fact]
        public void GivenHouseholdMemberWithoutMoveInDate_WhenRecorded_ThenThrowsBusinessRuleViolation()
        {
            var act = () => _testBed.Open().RecordHeir(
                "Sam", "sam@example.com", null, HeirRelationship.HouseholdMember, null, null, null, null, null, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("homeownership.heir_transfer.record_heir.moved_in_on_required");
        }

        [Fact]
        public void GivenOtherHeirUnderTheLimit_WhenRecorded_ThenKeepsTheCheckAgainstTheLimit()
        {
            var recorded = _testBed.Open().RecordHeir(
                "Nia Okoye", "nia@example.com", null, HeirRelationship.Other, null, Money.Create(64_000m), 3, [" doc-1 ", "doc-1", "doc-2"],
                _testBed.Limit, _testBed.StaffUser);

            recorded.Should().BeEquivalentTo(new
            {
                VerifiedAnnualIncome = Money.Create(64_000m),
                VerifiedHouseholdSize = 3,
                IncomeDocumentIds = new[] { "doc-1", "doc-2" },
                IncomeLimit = Money.Create(64_000m),
                IncomeLimitPercentOfAmi = 80,
                IncomeLimitsEffectiveFrom = new DateOnly(2026, 1, 1),
            });
        }

        [Fact]
        public void GivenOtherHeirOverTheLimit_WhenRecorded_ThenThrowsBusinessRuleViolation()
        {
            var act = () => _testBed.Open().RecordHeir(
                "Nia", "nia@example.com", null, HeirRelationship.Other, null, Money.Create(64_000.01m), 3, ["doc-1"], _testBed.Limit, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("homeownership.heir_transfer.record_heir.over_income_limit");
        }

        [Fact]
        public void GivenOtherHeirAndHomeWithoutALimit_WhenRecorded_ThenRecordsThemUnchecked()
        {
            var recorded = _testBed.Open().RecordHeir(
                "Nia", "nia@example.com", null, HeirRelationship.Other, null, Money.Create(500_000m), 1, ["doc-1"], null, _testBed.StaffUser);

            recorded.Should().BeEquivalentTo(new { IncomeLimit = (Money?)null, IncomeLimitPercentOfAmi = (int?)null });
        }

        [Fact]
        public void GivenOtherHeirWithoutIncomeDocuments_WhenRecorded_ThenThrowsBusinessRuleViolation()
        {
            var act = () => _testBed.Open().RecordHeir(
                "Nia", "nia@example.com", null, HeirRelationship.Other, null, Money.Create(40_000m), 2, [], _testBed.Limit, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("homeownership.heir_transfer.record_heir.income_documents_required");
        }

        [Theory]
        [InlineData(null, 2, "homeownership.heir_transfer.record_heir.income_required")]
        [InlineData(40_000, null, "homeownership.heir_transfer.record_heir.household_size_required")]
        [InlineData(-1, 2, "homeownership.heir_transfer.record_heir.income_negative")]
        [InlineData(40_000, 0, "homeownership.heir_transfer.record_heir.household_size_below_one")]
        public void GivenIncompleteOtherHeir_WhenRecorded_ThenThrowsBusinessRuleViolation(int? income, int? size, string expectedCode)
        {
            var act = () => _testBed.Open().RecordHeir(
                "Nia", "nia@example.com", null, HeirRelationship.Other, null, income is { } i ? Money.Create(i) : null, size, ["doc-1"], _testBed.Limit, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Fact]
        public void GivenRecordedHeir_WhenRecordedAgain_ThenReplacesTheHeir()
        {
            var transfer = _testBed.WithHeir();

            var (sut, _) = transfer.Decide(t => t.RecordHeir(
                "Lee Whitfield", "lee@example.com", null, HeirRelationship.Child, null, null, null, null, null, _testBed.StaffUser));

            sut.Heir!.FullName.Value.Should().Be("Lee Whitfield");
            sut.Relationship.Should().Be(HeirRelationship.Child);
        }

        [Fact]
        public void GivenClosedTransfer_WhenRecorded_ThenThrowsBusinessRuleViolation()
        {
            var closed = _testBed.Open().Decide(t => t.Close(HeirTransferCloseReason.HeirDeclined, null, _testBed.StaffUser)).Aggregate;

            var act = () => _testBed.RecordSpouse(closed, HeirRelationship.Spouse);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("homeownership.heir_transfer.record_heir.not_open");
        }
    }

    public class TheCompleteMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenHeirRecorded_WhenCompleted_ThenKeepsTheLettersAndEndsTheTransfer()
        {
            var (sut, events) = _testBed.WithHeir().Decide(_testBed.Complete);

            events.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                TransferredOn = new DateOnly(2026, 5, 20),
                LetterOfAgreementDocumentId = "document-1",
                AttorneyAcknowledgementDocumentId = "document-2",
                Notes = "Signed in person",
            });
            sut.Status.Should().Be(HeirTransferStatus.Completed);
        }

        [Fact]
        public void GivenNoHeirRecorded_WhenCompleted_ThenThrowsBusinessRuleViolation()
        {
            var act = () => _testBed.Complete(_testBed.Open());

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("homeownership.heir_transfer.complete.no_heir_recorded");
        }

        [Theory]
        [InlineData(null, "document-2")]
        [InlineData("document-1", " ")]
        public void GivenAMissingLetter_WhenCompleted_ThenThrowsBusinessRuleViolation(string? letter, string? acknowledgement)
        {
            var act = () => _testBed.WithHeir().Complete(new DateOnly(2026, 5, 20), letter, acknowledgement, null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("homeownership.heir_transfer.complete.letters_required");
        }

        [Theory]
        [InlineData("2026-03-09", "homeownership.heir_transfer.complete.transferred_on_before_death")]
        [InlineData("2026-06-02", "homeownership.heir_transfer.complete.transferred_on_in_future")]
        public void GivenInvalidDate_WhenCompleted_ThenThrowsBusinessRuleViolation(string transferredOn, string expectedCode)
        {
            var act = () => _testBed.WithHeir().Complete(
                DateOnly.Parse(transferredOn, CultureInfo.InvariantCulture), "document-1", "document-2", null, TestBed.Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }
    }

    public class TheCloseMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenOpenTransfer_WhenClosed_ThenKeepsTheReason()
        {
            var (sut, events) = _testBed.Open().Decide(t => t.Close(HeirTransferCloseReason.HeirNotQualified, " Over the limit ", _testBed.StaffUser));

            events.Should().ContainSingle().Which.Should().BeEquivalentTo(new { Reason = HeirTransferCloseReason.HeirNotQualified, Notes = "Over the limit" });
            sut.Status.Should().Be(HeirTransferStatus.Closed);
        }

        [Fact]
        public void GivenCompletedTransfer_WhenClosed_ThenThrowsBusinessRuleViolation()
        {
            var completed = _testBed.WithHeir().Decide(_testBed.Complete).Aggregate;

            var act = () => completed.Close(HeirTransferCloseReason.Other, null, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("homeownership.heir_transfer.close.not_open");
        }
    }

    public class ThePassToHeirMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenHeirRecorded_WhenPassed_ThenTheHeirIsHomeownerAndTheDeceasedIsInTheHistory()
        {
            var transfer = _testBed.WithHeir();

            var (sut, events) = _testBed.Lease.Decide(l => l.PassToHeir(transfer, new DateOnly(2026, 5, 20), _testBed.StaffUser));

            events.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                HeirTransferId = transfer.Id.Value,
                PreviousFullName = "Dana Whitfield",
                HomeownerFullName = "Sam Whitfield",
                Relationship = "Spouse",
            });
            sut.CurrentHomeowner!.FullName.Value.Should().Be("Sam Whitfield");
            sut.PreviousHomeowners.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                Homeowner = Homeowner.From("Dana Whitfield", "dana@example.com", "415-555-0100"),
                TransferredOn = new DateOnly(2026, 5, 20),
                HeirTransferId = transfer.Id,
            });
            sut.BasePrice.Should().Be(_testBed.Lease.BasePrice);
        }

        [Fact]
        public void GivenOpenResale_WhenPassed_ThenThrowsBusinessRuleViolation()
        {
            var transfer = _testBed.WithHeir();
            var selling = _testBed.Lease.Evolve(new ResaleStarted(_testBed.Lease.Id.Value, ResaleId.Generate().Value))!;

            var act = () => selling.PassToHeir(transfer, new DateOnly(2026, 5, 20), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("homeownership.heir_transfer.complete.open_resale");
        }

        [Fact]
        public void GivenEndedLease_WhenPassed_ThenThrowsBusinessRuleViolation()
        {
            var transfer = _testBed.WithHeir();
            var ended = _testBed.Lease.Evolve(new GroundLeaseTerminated(_testBed.Lease.Id.Value, "Sold", TestBed.DiedOn, _testBed.StaffUser.Value))!;

            var act = () => ended.PassToHeir(transfer, new DateOnly(2026, 5, 20), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("homeownership.heir_transfer.complete.lease_not_active");
        }
    }
}
