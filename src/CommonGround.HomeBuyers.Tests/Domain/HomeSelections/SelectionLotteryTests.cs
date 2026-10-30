using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Domain.HomeSelections;

public class SelectionLotteryTests
{
    public class TestBed
    {
        public IReadOnlyList<HomeownerApplicationId> Pool { get; } =
        [
            HomeownerApplicationId.Create("application-a"),
            HomeownerApplicationId.Create("application-b"),
            HomeownerApplicationId.Create("application-c"),
            HomeownerApplicationId.Create("application-d"),
            HomeownerApplicationId.Create("application-e"),
        ];
    }

    public class TheDrawMethod
    {
        private readonly TestBed _testBed = new();

        // Pinned so a recorded draw can always be re-run: changing the algorithm breaks these.
        [Theory]
        [InlineData(42, "application-d")]
        [InlineData(7, "application-c")]
        [InlineData(-1, "application-a")]
        public void GivenSeed_WhenDrawn_ThenPicksTheSameWinnerEveryTime(int seed, string expectedWinner)
        {
            var winner = SelectionLottery.Draw(_testBed.Pool, seed);

            winner.Value.Should().Be(expectedWinner);
        }

        [Fact]
        public void GivenPoolInAnotherOrder_WhenDrawn_ThenPicksTheSameWinner()
        {
            var reversed = _testBed.Pool.Reverse().ToList();

            var winner = SelectionLottery.Draw(reversed, 42);

            winner.Should().Be(SelectionLottery.Draw(_testBed.Pool, 42));
        }

        [Fact]
        public void GivenEmptyPool_WhenDrawn_ThenThrowsArgumentException()
        {
            var act = () => SelectionLottery.Draw([], 42);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GivenSameApplicationTwice_WhenDrawn_ThenThrowsArgumentException()
        {
            var act = () => SelectionLottery.Draw([_testBed.Pool[0], HomeownerApplicationId.Create("application-a")], 42);

            act.Should().Throw<ArgumentException>();
        }
    }
}
