using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.Homeownership.Domain.Resales.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.Resales;

public class ResaleTests
{
    public class TestBed
    {
        public static readonly DateOnly NoticeDate = new(2026, 3, 10);

        public UserId StaffUser { get; } = UserId.Generate();

        public ResaleId ResaleId { get; } = ResaleId.Generate();

        public GroundLease Lease { get; } = EventReplay.Replay<GroundLease>(GroundLease.Establish(
            GroundLeaseId.Generate(), "land-parcel-1234", "home-5678", Homeowner.From("Dana Whitfield", "dana@example.com"),
            new DateOnly(2020, 1, 1), 99, 60, "Covenants", ResalePolicyTerms.AppraisalBased(0.25m), HomeCommitments.None,
            UserId.Generate()));

        public IntentToSellRecorded Recorded() =>
            Resale.RecordIntentToSell(ResaleId, Lease, NoticeDate, NoticeChannel.Phone, null, NoticeDate, StaffUser);

        public Resale OpenResale() => EventReplay.Replay<Resale>(Recorded());
    }

    public class TheRecordIntentToSellMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenNoticeToday_WhenRecorded_ThenTheOptionWindowEndsTheLeasesWindowLater()
        {
            var recorded = Resale.RecordIntentToSell(
                _testBed.ResaleId, _testBed.Lease, TestBed.NoticeDate, NoticeChannel.Email, "  Moving for work  ", TestBed.NoticeDate, _testBed.StaffUser);

            recorded.Should().BeEquivalentTo(new
            {
                GroundLeaseId = _testBed.Lease.Id.Value,
                HomeId = "home-5678",
                HomeownerFullName = "Dana Whitfield",
                Notes = "Moving for work",
                OptionWindowEndsOn = new DateOnly(2026, 5, 9),
            });
        }

        [Fact]
        public void GivenBlankNotes_WhenRecorded_ThenRecordsNoNotes()
        {
            var recorded = Resale.RecordIntentToSell(
                _testBed.ResaleId, _testBed.Lease, TestBed.NoticeDate, NoticeChannel.Letter, "   ", TestBed.NoticeDate, _testBed.StaffUser);

            recorded.Notes.Should().BeNull();
        }

        [Fact]
        public void GivenNoticeDateAfterToday_WhenRecorded_ThenThrowsBusinessRuleViolation()
        {
            var act = () => Resale.RecordIntentToSell(
                _testBed.ResaleId, _testBed.Lease, TestBed.NoticeDate.AddDays(1), NoticeChannel.Phone, null, TestBed.NoticeDate, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.resale.record_intent_to_sell.notice_date_in_future");
        }

        [Fact]
        public void GivenValidNotice_WhenRecorded_ThenOpensTheResale()
        {
            var recorded = _testBed.Recorded();

            EventReplay.Replay<Resale>(recorded).Status.Should().Be(ResaleStatus.Open);
        }
    }

    public class TheWithdrawMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenOpenResale_WhenWithdrawn_ThenIsWithdrawn()
        {
            var sut = _testBed.OpenResale();

            var (withdrawn, _) = sut.Decide(r => r.Withdraw(TestBed.NoticeDate.AddDays(5), "Staying put", TestBed.NoticeDate.AddDays(5), _testBed.StaffUser));

            withdrawn.Status.Should().Be(ResaleStatus.Withdrawn);
        }

        [Fact]
        public void GivenWithdrawnResale_WhenWithdrawn_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.OpenResale().Decide(r => r.Withdraw(TestBed.NoticeDate, "Staying put", TestBed.NoticeDate, _testBed.StaffUser)).Aggregate;

            var act = () => sut.Withdraw(TestBed.NoticeDate, "Again", TestBed.NoticeDate, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.resale.withdraw.not_open");
        }

        [Theory]
        [InlineData(0, " ", 0, "homeownership.resale.withdraw.reason_required")]
        [InlineData(-1, "Staying put", 0, "homeownership.resale.withdraw.withdrawn_date_before_notice")]
        [InlineData(3, "Staying put", 2, "homeownership.resale.withdraw.withdrawn_date_in_future")]
        public void GivenInvalidInputs_WhenWithdrawn_ThenThrowsBusinessRuleViolation(
            int withdrawnDaysAfterNotice, string reason, int todayDaysAfterNotice, string expectedCode)
        {
            var sut = _testBed.OpenResale();

            var act = () => sut.Withdraw(
                TestBed.NoticeDate.AddDays(withdrawnDaysAfterNotice), reason, TestBed.NoticeDate.AddDays(todayDaysAfterNotice), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }
    }
}
