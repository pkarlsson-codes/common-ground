using System.Globalization;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases.LeaseFees;

public class LeaseFeeNoticeScheduleTests
{
    public class TestBed
    {
        public static readonly DateOnly LeaseStart = new(2026, 1, 1);

        public LeaseFeeNoticeSchedule Schedule { get; } = LeaseFeeNoticeSchedule.Default;

        /// <summary>A $60 lease paid through January, so February's charge is the oldest unpaid.</summary>
        public static LeaseFeeStatement BehindSinceFebruary(string asOf) =>
            LeaseFeeStatement.Calculate(
                LeaseFeeTerms.Create(45m, 15m), LeaseStart, null, [new ReceivedPayment(LeaseStart, 60m)], Day(asOf));

        public static DateOnly Day(string iso) => DateOnly.Parse(iso, CultureInfo.InvariantCulture);

        public static LeaseFeeNoticeSent Sent(LeaseFeeNoticeLevel level, string on) => new(level, Day(on));
    }

    public class TheCreateMethod
    {
        [Theory]
        [InlineData(0, 30, "homeownership.lease_fee_notice_schedule.create.first_notice_days_too_few")]
        [InlineData(10, 10, "homeownership.lease_fee_notice_schedule.create.second_notice_not_after_first")]
        public void GivenInvalidDays_WhenCreated_ThenThrowsBusinessRuleViolation(int first, int second, string expectedCode)
        {
            var act = () => LeaseFeeNoticeSchedule.Create(first, second);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }
    }

    public class TheNoticeDueMethod
    {
        private readonly TestBed _testBed = new();

        [Theory]
        [InlineData("2026-02-10", null)]
        [InlineData("2026-02-11", "First")]
        [InlineData("2026-03-03", "Second")]
        public void GivenNoNoticeSent_WhenDaysOverduePassAThreshold_ThenThatNoticeIsDue(string asOf, string? expected)
        {
            var sut = _testBed.Schedule.NoticeDue(TestBed.BehindSinceFebruary(asOf), []);

            sut?.DisplayName.Should().Be(expected);
            if (expected is null)
                sut.Should().BeNull();
        }

        [Fact]
        public void GivenFirstNoticeSentInTheseArrears_WhenStillBeforeTheSecondThreshold_ThenNothingIsDue()
        {
            var sut = _testBed.Schedule.NoticeDue(
                TestBed.BehindSinceFebruary("2026-02-25"), [TestBed.Sent(LeaseFeeNoticeLevel.First, "2026-02-12")]);

            sut.Should().BeNull();
        }

        [Fact]
        public void GivenFirstNoticeSentInTheseArrears_WhenPastTheSecondThreshold_ThenTheSecondIsDue()
        {
            var sut = _testBed.Schedule.NoticeDue(
                TestBed.BehindSinceFebruary("2026-03-05"), [TestBed.Sent(LeaseFeeNoticeLevel.First, "2026-02-12")]);

            sut.Should().Be(LeaseFeeNoticeLevel.Second);
        }

        [Fact]
        public void GivenSecondNoticeSentInTheseArrears_WhenStillBehind_ThenNoThirdNoticeIsDue()
        {
            var sut = _testBed.Schedule.NoticeDue(
                TestBed.BehindSinceFebruary("2026-05-20"), [TestBed.Sent(LeaseFeeNoticeLevel.Second, "2026-03-05")]);

            sut.Should().BeNull();
        }

        [Fact]
        public void GivenNoticeSentBeforeTheseArrearsBegan_WhenPastTheFirstThreshold_ThenTheFirstIsDueAgain()
        {
            var sut = _testBed.Schedule.NoticeDue(
                TestBed.BehindSinceFebruary("2026-02-15"), [TestBed.Sent(LeaseFeeNoticeLevel.Second, "2026-01-20")]);

            sut.Should().Be(LeaseFeeNoticeLevel.First);
        }

        [Fact]
        public void GivenTrustScheduleOfFiveAndFifteenDays_WhenSixDaysOverdue_ThenTheFirstIsDue()
        {
            var sut = LeaseFeeNoticeSchedule.Create(5, 15).NoticeDue(TestBed.BehindSinceFebruary("2026-02-07"), []);

            sut.Should().Be(LeaseFeeNoticeLevel.First);
        }

        [Fact]
        public void GivenNothingOwed_WhenAskedLongAfter_ThenNothingIsDue()
        {
            var paidUp = LeaseFeeStatement.Calculate(
                LeaseFeeTerms.Create(45m, 15m), TestBed.LeaseStart, null, [new ReceivedPayment(TestBed.LeaseStart, 600m)],
                TestBed.Day("2026-04-20"));

            _testBed.Schedule.NoticeDue(paidUp, []).Should().BeNull();
        }
    }
}
