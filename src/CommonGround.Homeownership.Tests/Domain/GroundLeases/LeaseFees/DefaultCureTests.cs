using CommonGround.SharedKernel.Domain;
using System.Globalization;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases.LeaseFees;

public class DefaultCureTests
{
    public class TestBed
    {
        /// <summary>A notice of default for $300 sent March 3; the 30-day deadline is April 2, extended to May 2.</summary>
        public static readonly DateOnly SentOn = new(2026, 3, 3);

        public static readonly Money AmountOwed = Money.Create(300m);

        public static DateOnly Day(string iso) => DateOnly.Parse(iso, CultureInfo.InvariantCulture);

        public static ReceivedPayment Paid(string on, decimal amount) => new(Day(on), Money.Create(amount));

        public static DefaultCure Cure(string asOf, params ReceivedPayment[] payments) =>
            DefaultCure.Calculate(SentOn, AmountOwed, DefaultCurePeriod.Default, payments, Day(asOf));
    }

    public class TheCalculateMethod
    {
        [Fact]
        public void GivenNothingPaid_WhenWithinTheCureDays_ThenCuringUntilThirtyDaysAfterSending()
        {
            var sut = TestBed.Cure("2026-03-20");

            sut.Should().BeEquivalentTo(new
            {
                Status = DefaultCureStatus.Curing,
                CureBy = TestBed.Day("2026-04-02"),
                Extended = false,
                StillToPay = Money.Create(300m),
            });
        }

        [Fact]
        public void GivenAmountOwedPaidByTheDeadline_WhenAsked_ThenCured()
        {
            var sut = TestBed.Cure("2026-06-01", TestBed.Paid("2026-03-10", 100m), TestBed.Paid("2026-04-02", 200m));

            sut.Status.Should().Be(DefaultCureStatus.Cured);
            sut.PaidInFullOn.Should().Be(TestBed.Day("2026-04-02"));
        }

        [Fact]
        public void GivenTwoThirdsPaidWithinTheCureDays_WhenAsked_ThenTheDeadlineIsExtendedThirtyDays()
        {
            var sut = TestBed.Cure("2026-04-20", TestBed.Paid("2026-03-25", 200m));

            sut.Should().BeEquivalentTo(new
            {
                Status = DefaultCureStatus.Curing,
                OriginalDeadline = TestBed.Day("2026-04-02"),
                CureBy = TestBed.Day("2026-05-02"),
                Extended = true,
                StillToPay = Money.Create(100m),
            });
        }

        [Fact]
        public void GivenTwoThirdsPaidAfterTheCureDays_WhenPastTheDeadline_ThenUncuredWithoutExtension()
        {
            var sut = TestBed.Cure("2026-04-20", TestBed.Paid("2026-04-03", 200m));

            sut.Status.Should().Be(DefaultCureStatus.Uncured);
            sut.Extended.Should().BeFalse();
        }

        [Fact]
        public void GivenLessThanTwoThirdsPaid_WhenPastTheDeadline_ThenUncured()
        {
            var sut = TestBed.Cure("2026-04-03", TestBed.Paid("2026-03-25", 199m));

            sut.Status.Should().Be(DefaultCureStatus.Uncured);
            sut.CureBy.Should().Be(TestBed.Day("2026-04-02"));
        }

        [Fact]
        public void GivenAmountOwedPaidAfterTheDeadline_WhenAsked_ThenPaidAfterDeadline()
        {
            var sut = TestBed.Cure("2026-06-01", TestBed.Paid("2026-04-10", 300m));

            sut.Status.Should().Be(DefaultCureStatus.PaidAfterDeadline);
        }

        [Fact]
        public void GivenPaymentBeforeTheNoticeWasSent_WhenAsked_ThenItDoesNotCount()
        {
            var sut = TestBed.Cure("2026-03-20", TestBed.Paid("2026-03-02", 300m));

            sut.Status.Should().Be(DefaultCureStatus.Curing);
            sut.PaidSinceNotice.Amount.Should().Be(0m);
        }

        [Fact]
        public void GivenNoExtensionDays_WhenTwoThirdsPaidInTime_ThenTheDeadlineIsNotExtended()
        {
            var sut = DefaultCure.Calculate(
                TestBed.SentOn, TestBed.AmountOwed, DefaultCurePeriod.Create(30, 0),
                [TestBed.Paid("2026-03-25", 250m)], TestBed.Day("2026-04-03"));

            sut.Status.Should().Be(DefaultCureStatus.Uncured);
        }
    }

    public class TheCurePeriodCreateMethod
    {
        [Theory]
        [InlineData(0, 30, "homeownership.default_cure_period.create.cure_days_too_few")]
        [InlineData(30, -1, "homeownership.default_cure_period.create.extension_days_negative")]
        public void GivenInvalidDays_WhenCreated_ThenThrowsBusinessRuleViolation(int cureDays, int extensionDays, string expectedCode)
        {
            var act = () => DefaultCurePeriod.Create(cureDays, extensionDays);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }
    }
}
