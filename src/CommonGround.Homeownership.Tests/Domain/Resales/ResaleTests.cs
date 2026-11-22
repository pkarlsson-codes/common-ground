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

        public GroundLease Lease { get; } = EventReplay.Replay<GroundLease>(GroundLease.RegisterExisting(
            GroundLeaseId.Generate(), "land-parcel-1234", "home-5678", Homeowner.From("Dana Whitfield", "dana@example.com"),
            new DateOnly(2020, 1, 1), 99, 60, LeaseFeeTerms.None, MortgageDebtCap.Create(90), Money.Create(200_000m), Money.Create(250_000m), "Covenants", ResalePolicyTerms.AppraisalBased(0.25m), HomeCommitments.None,
            UserId.Generate(), LeaseFeeTracking.FromLeaseStart(new DateOnly(2020, 1, 1))));

        public IntentToSellRecorded Recorded() =>
            Resale.RecordIntentToSell(ResaleId, Lease, NoticeDate, NoticeChannel.Phone, null, NoticeDate, StaffUser);

        public Resale OpenResale() => EventReplay.Replay<Resale>(Recorded());

        public GroundLease LeaseWithoutFigures() => EventReplay.Replay<GroundLease>(new GroundLeaseCreatedFromPurchase(
            GroundLeaseId.Generate().Value, PurchaseWorkflowId.Generate().Value, GroundLeaseTemplateId.Generate().Value, "land-parcel-1234", "home-5678",
            new DateOnly(2020, 1, 1), 99, 60, Money.Create(0m), Money.Create(0m), 0m, 30, 90, Money.Create(200_000m), null, "Covenants", FormulaKind.AppraisalBased, 0.25m, null, null, null, null,
            "user-1", "Dana Whitfield", "dana@example.com", null, null, null, null, null));

        public Resale Appraised(decimal appraisedValue) => OpenResale().Decide(r => r.RecordAppraisal(
            Lease, Money.Create(appraisedValue), NoticeDate, "Hollis", null, null, NoticeDate, StaffUser)).Aggregate;

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
                _testBed.Lease, Money.Create(appraisedValue), TestBed.NoticeDate, "  Hollis  ", null, null, TestBed.NoticeDate, _testBed.StaffUser);

            recorded.Should().BeEquivalentTo(new
            {
                AppraisedValue = Money.Create(appraisedValue),
                AppraiserName = "Hollis",
                FormulaPrice = Money.Create(formulaPrice),
                PurchaseOptionPrice = Money.Create(optionPrice),
            });
        }

        [Fact]
        public void GivenEarlierAppraisal_WhenRecordedAgain_ThenTheNewOneReplacesIt()
        {
            var sut = _testBed.Appraised(270_000);

            var (replaced, _) = sut.Decide(r => r.RecordAppraisal(
                _testBed.Lease, Money.Create(190_000), TestBed.NoticeDate, "Hollis", null, null, TestBed.NoticeDate, _testBed.StaffUser));

            replaced.Appraisal.Should().Be(new ResaleAppraisal(Money.Create(190_000), TestBed.NoticeDate, Money.Create(185_000), Money.Create(185_000)));
        }

        [Fact]
        public void GivenOptionExercised_WhenRecorded_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Exercised();

            var act = () => sut.RecordAppraisal(
                _testBed.Lease, Money.Create(270_000), TestBed.NoticeDate, "Hollis", null, null, TestBed.NoticeDate, _testBed.StaffUser);

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
                _testBed.Lease, Money.Create(appraisedValue), TestBed.NoticeDate.AddDays(dateDaysAfterNotice), appraiser, null, null,
                TestBed.NoticeDate.AddDays(todayDaysAfterNotice), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Fact]
        public void GivenLeaseWithoutOriginalAppraisedValue_WhenRecorded_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.OpenResale();
            var lease = _testBed.LeaseWithoutFigures();

            var act = () => sut.RecordAppraisal(
                lease, Money.Create(270_000), TestBed.NoticeDate, "Hollis", null, null, TestBed.NoticeDate, _testBed.StaffUser);

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
                TestBed.NoticeDate, Money.Create(200_000), " Done ", true, "ignored", TestBed.NoticeDate, _testBed.StaffUser));

            closed.Status.Should().Be(ResaleStatus.Closed);
            closing.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                GroundLeaseId = _testBed.Lease.Id.Value,
                HomeId = "home-5678",
                PricePaid = Money.Create(200_000m),
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
                TestBed.NoticeDate, Money.Create(205_000), null, false, " Roof work ", TestBed.NoticeDate, _testBed.StaffUser));

            closing.Should().ContainSingle().Which.Should().BeEquivalentTo(new { OfferedForSale = false, HoldReason = "Roof work" });
        }

        [Fact]
        public void GivenOptionNotExercised_WhenClosed_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Appraised(270_000);

            var act = () => sut.CloseBuyback(TestBed.NoticeDate, Money.Create(100_000), null, true, null, TestBed.NoticeDate, _testBed.StaffUser);

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
                TestBed.NoticeDate.AddDays(closingDaysAfterExercised), Money.Create(pricePaid), null, offerForSale, holdReason,
                TestBed.NoticeDate.AddDays(todayDaysAfterExercised), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }
    }

    public class ThePayoffStatementMethods
    {
        private static readonly DateOnly Today = new(2026, 3, 10);
        private readonly TestBed _testBed = new();

        private GroundLease FeeLease() => EventReplay.Replay<GroundLease>(GroundLease.RegisterExisting(
            GroundLeaseId.Generate(), "land-parcel-1234", "home-5678", Homeowner.From("Dana Whitfield", "dana@example.com"),
            new DateOnly(2026, 1, 1), 99, 60, LeaseFeeTerms.Create(Money.Create(50m), Money.Create(10m)), MortgageDebtCap.Create(90),
            Money.Create(200_000m), Money.Create(250_000m), "Covenants", ResalePolicyTerms.AppraisalBased(0.25m), HomeCommitments.None, _testBed.StaffUser, LeaseFeeTracking.FromLeaseStart(new DateOnly(2026, 1, 1))));

        private Resale ResaleOn(GroundLease lease) => EventReplay.Replay<Resale>(
            Resale.RecordIntentToSell(_testBed.ResaleId, lease, Today, NoticeChannel.Phone, null, Today, _testBed.StaffUser));

        [Fact]
        public void GivenThreeUnpaidMonths_WhenWorkedOut_ThenOwesThreeMonthlyFees()
        {
            var lease = FeeLease();

            var statement = ResaleOn(lease).PayoffStatementOn(lease, Today, Today);

            statement.Should().BeEquivalentTo(new
            {
                GoodThrough = Today,
                FeesOwed = Money.Create(180m),
                InterestOwed = Money.Zero,
                Credit = Money.Zero,
                TotalOwed = Money.Create(180m),
                OldestUnpaidMonth = (DateOnly?)new DateOnly(2026, 1, 1),
            });
        }

        [Fact]
        public void GivenGoodThroughAfterNextFirst_WhenWorkedOut_ThenCountsThatMonthToo()
        {
            var lease = FeeLease();

            var statement = ResaleOn(lease).PayoffStatementOn(lease, new DateOnly(2026, 4, 2), Today);

            statement.TotalOwed.Should().Be(Money.Create(240m));
        }

        [Fact]
        public void GivenPaymentsAboveFees_WhenWorkedOut_ThenShowsCreditAndOwesNothing()
        {
            var lease = FeeLease();
            lease = lease.Evolve(lease.RecordLeaseFeePayment(
                LeaseFeePaymentId.Generate(), new DateOnly(2026, 3, 1), Money.Create(200m), PaymentMethod.Check, null, Today, _testBed.StaffUser))!;

            var statement = ResaleOn(lease).PayoffStatementOn(lease, Today, Today);

            statement.Should().BeEquivalentTo(new { FeesOwed = Money.Zero, Credit = Money.Create(20m), TotalOwed = Money.Zero, OldestUnpaidMonth = (DateOnly?)null });
        }

        [Fact]
        public void GivenValidGoodThrough_WhenRecorded_ThenRecordsTheTotalOwed()
        {
            var lease = FeeLease();
            var sut = ResaleOn(lease);

            var (recorded, events) = sut.Decide(r => r.RecordPayoffStatement(lease, new DateOnly(2026, 4, 1), Today, _testBed.StaffUser));

            recorded.PayoffStatementCount.Should().Be(1);
            events.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                GroundLeaseId = lease.Id.Value,
                GoodThrough = new DateOnly(2026, 4, 1),
                TotalOwed = Money.Create(240m),
            });
        }

        [Theory]
        [InlineData(-1, "homeownership.resale.payoff_statement.good_through_in_past")]
        [InlineData(61, "homeownership.resale.payoff_statement.good_through_too_far")]
        public void GivenGoodThroughOutsideTheRange_WhenWorkedOut_ThenThrowsBusinessRuleViolation(int daysFromToday, string expectedCode)
        {
            var lease = FeeLease();
            var sut = ResaleOn(lease);

            var act = () => sut.PayoffStatementOn(lease, Today.AddDays(daysFromToday), Today);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Fact]
        public void GivenGoodThroughSixtyDaysAhead_WhenWorkedOut_ThenAllowsIt()
        {
            var lease = FeeLease();

            var act = () => ResaleOn(lease).PayoffStatementOn(lease, Today.AddDays(60), Today);

            act.Should().NotThrow();
        }

        [Fact]
        public void GivenEndedLease_WhenWorkedOut_ThenThrowsLeaseNotActive()
        {
            var lease = FeeLease();
            var sut = ResaleOn(lease);
            lease = lease.Evolve(lease.Terminate("Bought by the trust", Today, _testBed.StaffUser))!;

            var act = () => sut.PayoffStatementOn(lease, Today, Today);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.resale.payoff_statement.lease_not_active");
        }

        [Fact]
        public void GivenWithdrawnResale_WhenWorkedOut_ThenThrowsNotOpen()
        {
            var lease = FeeLease();
            var sut = ResaleOn(lease).Decide(r => r.Withdraw(Today, "Staying", Today, _testBed.StaffUser)).Aggregate;

            var act = () => sut.PayoffStatementOn(lease, Today, Today);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.resale.payoff_statement.not_open");
        }
    }

    public class TheCloseBuybackFeesPaidFromProceeds
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenAmountAboveZero_WhenClosed_ThenRecordsItOnTheEvent()
        {
            var (_, events) = _testBed.Exercised().Decide(r => r.CloseBuyback(
                TestBed.NoticeDate, Money.Create(200_000), null, true, null, TestBed.NoticeDate, _testBed.StaffUser, Money.Create(180m)));

            events.Should().ContainSingle().Which.Should().BeEquivalentTo(new { FeesPaidFromProceeds = Money.Create(180m) });
        }

        [Fact]
        public void GivenZero_WhenClosed_ThenRecordsNone()
        {
            var (_, events) = _testBed.Exercised().Decide(r => r.CloseBuyback(
                TestBed.NoticeDate, Money.Create(200_000), null, true, null, TestBed.NoticeDate, _testBed.StaffUser, Money.Zero));

            events.Should().ContainSingle().Which.Should().BeEquivalentTo(new { FeesPaidFromProceeds = (Money?)null });
        }

        [Fact]
        public void GivenNegativeAmount_WhenClosed_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Exercised();

            var act = () => sut.CloseBuyback(
                TestBed.NoticeDate, Money.Create(200_000), null, true, null, TestBed.NoticeDate, _testBed.StaffUser, Money.Create(-1m));

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.resale.close_buyback.fees_paid_from_proceeds_negative");
        }
    }
}
