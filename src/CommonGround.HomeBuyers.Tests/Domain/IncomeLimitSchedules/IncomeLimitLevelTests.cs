using CommonGround.HomeBuyers.Domain.IncomeLimitSchedules;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Domain.IncomeLimitSchedules;

public class IncomeLimitLevelTests
{
    public class TestBed
    {
        public static List<Money> Limits(params decimal[] amounts) => [.. amounts.Select(Money.Create)];

        public List<Money> EightyPercentLimits { get; } =
            Limits(62_050, 70_900, 79_750, 88_600, 95_700, 102_800, 109_900, 116_950);

        public IncomeLimitLevel EightyPercent() => IncomeLimitLevel.Create(80, EightyPercentLimits);
    }

    public class TheCreateMethod
    {
        private readonly TestBed _testBed = new();

        [Theory]
        [InlineData(0)]
        [InlineData(201)]
        public void GivenPercentOutsideOneTo200_WhenCreated_ThenThrowsPercentOutOfRange(int percent)
        {
            var act = () => IncomeLimitLevel.Create(percent, _testBed.EightyPercentLimits);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.income_limit_schedule.level.percent_out_of_range");
        }

        [Fact]
        public void GivenSevenLimits_WhenCreated_ThenThrowsWrongNumberOfLimits()
        {
            var act = () => IncomeLimitLevel.Create(80, _testBed.EightyPercentLimits[..7]);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.income_limit_schedule.level.wrong_number_of_limits");
        }

        [Fact]
        public void GivenAZeroLimit_WhenCreated_ThenThrowsLimitNotPositiveNamingTheHouseholdSize()
        {
            var limits = TestBed.Limits(62_050, 70_900, 0, 88_600, 95_700, 102_800, 109_900, 116_950);

            var act = () => IncomeLimitLevel.Create(80, limits);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Should().BeEquivalentTo(new
                {
                    Code = "home_buyers.income_limit_schedule.level.limit_not_positive",
                    Args = new Dictionary<string, object?> { ["percentOfAmi"] = 80, ["householdSize"] = 3 },
                });
        }
    }

    public class TheLimitForMethod
    {
        private readonly TestBed _testBed = new();

        [Theory]
        [InlineData(1, 62_050)]
        [InlineData(4, 88_600)]
        [InlineData(8, 116_950)]
        public void GivenAPrintedSize_WhenLimitAsked_ThenReturnsThePublishedAmount(int size, decimal expected)
        {
            var sut = _testBed.EightyPercent();

            sut.LimitFor(size).Should().Be(Money.Create(expected));
        }

        [Theory]
        [InlineData(9, 124_050)]   // 116,950 + 7,088 = 124,038
        [InlineData(10, 131_150)]  // 116,950 + 14,176 = 131,126
        public void GivenMoreThanEightPeople_WhenLimitAsked_ThenAddsEightPercentOfTheFourPersonLimitPerExtraPersonRoundedToFifty(
            int size, decimal expected)
        {
            var sut = _testBed.EightyPercent();

            sut.LimitFor(size).Should().Be(Money.Create(expected));
        }

        [Theory]
        [InlineData(120_024, 128_000)]
        [InlineData(120_025, 128_000)]
        [InlineData(120_026, 128_050)]
        public void GivenNinePeopleWithAnAmountNearHalfwayBetweenFifties_WhenLimitAsked_ThenRoundsHalfwayDown(
            decimal eightPersonLimit, decimal expected)
        {
            // The 4-person limit of 100,000 adds 8,000 for the ninth person.
            var sut = IncomeLimitLevel.Create(80, TestBed.Limits(70_000, 80_000, 90_000, 100_000, 105_000, 110_000, 115_000, eightPersonLimit));

            sut.LimitFor(9).Should().Be(Money.Create(expected));
        }
    }
}
