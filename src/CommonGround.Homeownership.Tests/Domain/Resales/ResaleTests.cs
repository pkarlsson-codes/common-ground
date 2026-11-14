using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
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
            new DateOnly(2020, 1, 1), 99, 60, LeaseFeeTerms.None, MortgageDebtCap.Create(90), 200_000m, 250_000m, "Covenants", ResalePolicyTerms.AppraisalBased(0.25m), HomeCommitments.None,
            UserId.Generate()));

        public IntentToSellRecorded Recorded() =>
            Resale.RecordIntentToSell(ResaleId, Lease, NoticeDate, NoticeChannel.Phone, null, NoticeDate, StaffUser);

        public Resale OpenResale() => EventReplay.Replay<Resale>(Recorded());

        public GroundLease LeaseWithoutFigures() => EventReplay.Replay<GroundLease>(new GroundLeaseCreatedFromPurchase(
            GroundLeaseId.Generate().Value, PurchaseWorkflowId.Generate().Value, GroundLeaseTemplateId.Generate().Value, "land-parcel-1234", "home-5678",
            new DateOnly(2020, 1, 1), 99, 60, Money.Create(0m), Money.Create(0m), 0m, 30, 90, 200_000m, null, "Covenants", FormulaKind.AppraisalBased, 0.25m, null, null, null, null,
            "user-1", "Dana Whitfield", "dana@example.com", null, null, null, null, null));

        public Resale Appraised(decimal appraisedValue) => OpenResale().Decide(r => r.RecordAppraisal(
            Lease, appraisedValue, NoticeDate, "Hollis", null, null, NoticeDate, StaffUser)).Aggregate;

        public Resale Exercised() => Appraised(270_000).Decide(r => r.ExerciseOption(NoticeDate, null, NoticeDate, StaffUser)).Aggregate;
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

    public class TheRecordAppraisalMethod
    {
        private readonly TestBed _testBed = new();

        [Theory]
        [InlineData(270_000, 205_000, 205_000)]
        [InlineData(190_000, 185_000, 185_000)]
        [InlineData(180_000, 182_500, 180_000)]
        public void GivenAppraisal_WhenRecorded_ThenOptionPriceIsTheLesserOfFormulaAndAppraisedValue(
            int appraisedValue, decimal formulaPrice, decimal optionPrice)
        {
            var sut = _testBed.OpenResale();

            var recorded = sut.RecordAppraisal(
                _testBed.Lease, appraisedValue, TestBed.NoticeDate, "  Hollis  ", null, null, TestBed.NoticeDate, _testBed.StaffUser);

            recorded.Should().BeEquivalentTo(new
            {
                AppraisedValue = (decimal)appraisedValue,
                AppraiserName = "Hollis",
                FormulaPrice = formulaPrice,
                PurchaseOptionPrice = optionPrice,
            });
        }

        [Fact]
        public void GivenEarlierAppraisal_WhenRecordedAgain_ThenTheNewOneReplacesIt()
        {
            var sut = _testBed.Appraised(270_000);

            var (replaced, _) = sut.Decide(r => r.RecordAppraisal(
                _testBed.Lease, 190_000, TestBed.NoticeDate, "Hollis", null, null, TestBed.NoticeDate, _testBed.StaffUser));

            replaced.Appraisal.Should().Be(new ResaleAppraisal(190_000, TestBed.NoticeDate, 185_000, 185_000));
        }

        [Fact]
        public void GivenOptionExercised_WhenRecorded_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Exercised();

            var act = () => sut.RecordAppraisal(
                _testBed.Lease, 270_000, TestBed.NoticeDate, "Hollis", null, null, TestBed.NoticeDate, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.resale.record_appraisal.option_exercised");
        }

        [Theory]
        [InlineData(0, "Hollis", 0, 0, "homeownership.resale.record_appraisal.appraised_value_non_positive")]
        [InlineData(270_000, " ", 0, 0, "homeownership.resale.record_appraisal.appraiser_required")]
        [InlineData(270_000, "Hollis", -1, 0, "homeownership.resale.record_appraisal.appraisal_date_before_notice")]
        [InlineData(270_000, "Hollis", 2, 1, "homeownership.resale.record_appraisal.appraisal_date_in_future")]
        public void GivenInvalidInputs_WhenRecorded_ThenThrowsBusinessRuleViolation(
            int appraisedValue, string appraiser, int dateDaysAfterNotice, int todayDaysAfterNotice, string expectedCode)
        {
            var sut = _testBed.OpenResale();

            var act = () => sut.RecordAppraisal(
                _testBed.Lease, appraisedValue, TestBed.NoticeDate.AddDays(dateDaysAfterNotice), appraiser, null, null,
                TestBed.NoticeDate.AddDays(todayDaysAfterNotice), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Fact]
        public void GivenLeaseWithoutOriginalAppraisedValue_WhenRecorded_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.OpenResale();
            var lease = _testBed.LeaseWithoutFigures();

            var act = () => sut.RecordAppraisal(
                lease, 270_000, TestBed.NoticeDate, "Hollis", null, null, TestBed.NoticeDate, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.resale.record_appraisal.lease_figures_missing");
        }
    }

    public class TheExerciseOptionMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenAppraisal_WhenExercised_ThenRecordsTheDecision()
        {
            var sut = _testBed.Appraised(270_000);

            var (exercised, _) = sut.Decide(r => r.ExerciseOption(
                TestBed.NoticeDate.AddDays(3), "  Board approved ", TestBed.NoticeDate.AddDays(3), _testBed.StaffUser));

            exercised.OptionExercisedOn.Should().Be(TestBed.NoticeDate.AddDays(3));
        }

        [Fact]
        public void GivenNoAppraisal_WhenExercised_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.OpenResale();

            var act = () => sut.ExerciseOption(TestBed.NoticeDate, null, TestBed.NoticeDate, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.resale.exercise_option.appraisal_required");
        }

        [Fact]
        public void GivenAlreadyExercised_WhenExercised_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Exercised();

            var act = () => sut.ExerciseOption(TestBed.NoticeDate, null, TestBed.NoticeDate, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.resale.exercise_option.already_exercised");
        }

        [Fact]
        public void GivenDateAfterTheOptionWindowEnds_WhenExercised_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Appraised(270_000);
            var afterWindow = sut.OptionWindowEndsOn.AddDays(1);

            var act = () => sut.ExerciseOption(afterWindow, null, afterWindow, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.resale.exercise_option.option_window_ended");
        }

        [Theory]
        [InlineData(-1, 0, "homeownership.resale.exercise_option.exercised_date_before_appraisal")]
        [InlineData(2, 1, "homeownership.resale.exercise_option.exercised_date_in_future")]
        public void GivenInvalidDates_WhenExercised_ThenThrowsBusinessRuleViolation(
            int exercisedDaysAfterAppraisal, int todayDaysAfterAppraisal, string expectedCode)
        {
            var sut = _testBed.Appraised(270_000);

            var act = () => sut.ExerciseOption(
                TestBed.NoticeDate.AddDays(exercisedDaysAfterAppraisal), null, TestBed.NoticeDate.AddDays(todayDaysAfterAppraisal), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Fact]
        public void GivenOptionExercised_WhenWithdrawn_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Exercised();

            var act = () => sut.Withdraw(TestBed.NoticeDate, "Staying put", TestBed.NoticeDate, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.resale.withdraw.option_exercised");
        }
    }

    public class TheCloseBuybackMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenExercisedOption_WhenClosedOffered_ThenClosesAndOffersTheHome()
        {
            var sut = _testBed.Exercised();

            var (closed, closing) = sut.Decide(r => r.CloseBuyback(
                TestBed.NoticeDate, 200_000, " Done ", true, "ignored", TestBed.NoticeDate, _testBed.StaffUser));

            closed.Status.Should().Be(ResaleStatus.Closed);
            closing.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                GroundLeaseId = _testBed.Lease.Id.Value,
                HomeId = "home-5678",
                PricePaid = 200_000m,
                Notes = "Done",
                OfferedForSale = true,
                HoldReason = (string?)null,
            });
        }

        [Fact]
        public void GivenExercisedOption_WhenClosedHeld_ThenKeepsTheReason()
        {
            var sut = _testBed.Exercised();

            var (_, closing) = sut.Decide(r => r.CloseBuyback(
                TestBed.NoticeDate, 205_000, null, false, " Roof work ", TestBed.NoticeDate, _testBed.StaffUser));

            closing.Should().ContainSingle().Which.Should().BeEquivalentTo(new { OfferedForSale = false, HoldReason = "Roof work" });
        }

        [Fact]
        public void GivenOptionNotExercised_WhenClosed_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Appraised(270_000);

            var act = () => sut.CloseBuyback(TestBed.NoticeDate, 100_000, null, true, null, TestBed.NoticeDate, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.resale.close_buyback.option_not_exercised");
        }

        [Theory]
        [InlineData(-1, 200_000, true, "x", 0, "homeownership.resale.close_buyback.closing_date_before_exercised")]
        [InlineData(2, 200_000, true, "x", 1, "homeownership.resale.close_buyback.closing_date_in_future")]
        [InlineData(0, 0, true, "x", 0, "homeownership.resale.close_buyback.price_paid_non_positive")]
        [InlineData(0, 205_001, true, "x", 0, "homeownership.resale.close_buyback.price_paid_above_option_price")]
        [InlineData(0, 200_000, false, " ", 0, "homeownership.resale.close_buyback.hold_reason_required")]
        public void GivenInvalidInputs_WhenClosed_ThenThrowsBusinessRuleViolation(
            int closingDaysAfterExercised, int pricePaid, bool offerForSale, string holdReason, int todayDaysAfterExercised, string expectedCode)
        {
            var sut = _testBed.Exercised();

            var act = () => sut.CloseBuyback(
                TestBed.NoticeDate.AddDays(closingDaysAfterExercised), pricePaid, null, offerForSale, holdReason,
                TestBed.NoticeDate.AddDays(todayDaysAfterExercised), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }
    }
}
