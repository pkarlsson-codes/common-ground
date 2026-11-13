using CommonGround.SharedKernel.Domain;
using FluentAssertions;

namespace CommonGround.SharedKernel.Tests.Domain;

public class IncomeLimitTableTests
{
    public class TestBed
    {
        public DateOnly EffectiveFrom { get; } = new(2026, 4, 1);

        public IncomeLimitLevel ThirtyPercent { get; } = Level(30, 23_250, 26_600, 29_900, 33_200, 35_900, 38_550, 41_200, 43_850);

        public IncomeLimitLevel FiftyPercent { get; } = Level(50, 38_750, 44_300, 49_850, 55_350, 59_800, 64_250, 68_650, 73_100);

        public IncomeLimitLevel EightyPercent { get; } = Level(80, 62_050, 70_900, 79_750, 88_600, 95_700, 102_800, 109_900, 116_950);

        public IncomeLimitTable Table() => IncomeLimitTable.Create(EffectiveFrom, [EightyPercent, ThirtyPercent, FiftyPercent]);

        private static IncomeLimitLevel Level(int percent, params decimal[] amounts) =>
            IncomeLimitLevel.Create(percent, [.. amounts.Select(Money.Create)]);
    }

    public class TheCreateMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenLevelsOutOfOrder_WhenCreated_ThenHoldsThemLowestPercentFirst()
        {
            var sut = _testBed.Table();

            sut.Levels.Select(l => l.PercentOfAmi).Should().Equal(30, 50, 80);
        }

        [Fact]
        public void GivenNoLevels_WhenCreated_ThenThrows()
        {
            var act = () => IncomeLimitTable.Create(_testBed.EffectiveFrom, []);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GivenTwoLevelsWithTheSamePercent_WhenCreated_ThenThrows()
        {
            var act = () => IncomeLimitTable.Create(_testBed.EffectiveFrom, [_testBed.FiftyPercent, _testBed.FiftyPercent]);

            act.Should().Throw<ArgumentException>();
        }
    }

    public class TheFromEntriesMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenTheEntriesOfATable_WhenRebuilt_ThenEqualsTheTable()
        {
            var table = _testBed.Table();

            var sut = IncomeLimitTable.FromEntries(table.EffectiveFrom, table.ToEntries());

            sut.Should().Be(table);
        }
    }

    public class TheBandForMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenIncomeAtTheLowestLevelsLimit_WhenBanded_ThenIsUpToTheLowestLevel()
        {
            var sut = _testBed.Table();

            var band = sut.BandFor(Money.Create(29_900m), 3);

            band.Should().Be(IncomeBand.Create(null, 30));
        }

        [Fact]
        public void GivenIncomeACentOverOneLevelsLimit_WhenBanded_ThenIsOverThatLevelUpToTheNext()
        {
            var sut = _testBed.Table();

            var band = sut.BandFor(Money.Create(29_900.01m), 3);

            band.Should().Be(IncomeBand.Create(30, 50));
        }

        [Fact]
        public void GivenIncomeOverTheHighestLevelsLimit_WhenBanded_ThenIsOverTheHighestLevel()
        {
            var sut = _testBed.Table();

            var band = sut.BandFor(Money.Create(79_750.01m), 3);

            band.Should().Be(IncomeBand.Create(80, null));
        }

        [Fact]
        public void GivenAHouseholdOfNine_WhenBanded_ThenUsesTheWorkedOutLimitForNine()
        {
            var sut = _testBed.Table();

            // 30% for 9 people: 43,850 + 8% of 33,200 = 46,506, rounded to 46,500.
            var band = sut.BandFor(Money.Create(46_500m), 9);

            band.Should().Be(IncomeBand.Create(null, 30));
        }
    }
}
