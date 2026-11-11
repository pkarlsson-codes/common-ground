using System.Text.Json;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.SharedKernel.Tests.Domain;

public class MoneyTests
{
    public class TheCreateMethod
    {
        [Theory]
        [InlineData("60")]
        [InlineData("60.5")]
        [InlineData("60.25")]
        [InlineData("-5.10")]
        public void GivenWholeCents_WhenCreated_ThenKeepsTheAmount(string amount)
        {
            Money.Create(decimal.Parse(amount)).Amount.Should().Be(decimal.Parse(amount));
        }

        [Fact]
        public void GivenAFractionOfACent_WhenCreated_ThenThrowsFractionOfCent()
        {
            var act = () => Money.Create(10.005m);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("shared.money.fraction_of_cent");
        }
    }

    public class TheRoundedFromMethod
    {
        [Theory]
        [InlineData("10.004", "10.00")]
        [InlineData("10.005", "10.01")]
        [InlineData("-10.005", "-10.01")]
        public void GivenAFinerAmount_WhenRounded_ThenRoundsHalfACentAwayFromZero(string amount, string expected)
        {
            Money.RoundedFrom(decimal.Parse(amount)).Should().Be(Money.Create(decimal.Parse(expected)));
        }
    }

    public class TheShareMethod
    {
        [Fact]
        public void GivenTwoThirdsOfThreeHundred_WhenShared_ThenIsExactlyTwoHundred()
        {
            Money.Create(300m).Share(2m / 3m).Should().Be(Money.Create(200m));
        }

        [Fact]
        public void GivenTwoThirdsOfOneDollar_WhenShared_ThenRoundsToTheCent()
        {
            Money.Create(1m).Share(2m / 3m).Should().Be(Money.Create(0.67m));
        }
    }

    public class TheOperators
    {
        [Fact]
        public void GivenTwoAmounts_WhenAddedAndSubtracted_ThenStayInCents()
        {
            var fee = Money.Create(45m);
            var reserve = Money.Create(15m);

            (fee + reserve).Should().Be(Money.Create(60m));
            (reserve - fee).Should().Be(Money.Create(-30m));
            (-fee).Should().Be(Money.Create(-45m));
        }

        [Fact]
        public void GivenAMonthlyFee_WhenMultipliedByMonths_ThenIsTheTotal()
        {
            (Money.Create(60m) * 3).Should().Be(Money.Create(180m));
        }

        [Fact]
        public void GivenTwoAmounts_WhenDivided_ThenIsTheRatio()
        {
            (Money.Create(150m) / Money.Create(60m)).Should().Be(2.5m);
        }

        [Fact]
        public void GivenTwoAmounts_WhenCompared_ThenComparesTheirAmounts()
        {
            var less = Money.Create(1m);
            var more = Money.Create(2m);

            (less < more).Should().BeTrue();
            (more >= less).Should().BeTrue();
            Money.Max(less, more).Should().Be(more);
            Money.Min(less, more).Should().Be(less);
        }

        [Fact]
        public void GivenAmounts_WhenSorted_ThenAreInOrderOfAmount()
        {
            var sorted = new[] { Money.Create(3m), Money.Create(-1m), Money.Create(2m) }.OrderByDescending(m => m).ToList();

            sorted.Should().Equal(Money.Create(3m), Money.Create(2m), Money.Create(-1m));
        }

        [Fact]
        public void GivenTheSameAmountAtDifferentScales_WhenCompared_ThenIsEqual()
        {
            (Money.Create(60m) == Money.Create(60.00m)).Should().BeTrue();
        }
    }

    public class TheSumMethod
    {
        [Fact]
        public void GivenNoAmounts_WhenSummed_ThenIsZero()
        {
            Array.Empty<Money>().Sum().Should().Be(Money.Zero);
        }

        [Fact]
        public void GivenAmounts_WhenSummed_ThenIsTheirTotal()
        {
            new[] { Money.Create(0.10m), Money.Create(0.20m) }.Sum(m => m).Should().Be(Money.Create(0.30m));
        }
    }

    public class TheToStringMethod
    {
        [Fact]
        public void GivenAnAmount_WhenFormatted_ThenIsUsDollars()
        {
            Money.Create(1234.5m).ToString().Should().Be("$1,234.50");
            Money.Create(-5m).ToString().Should().Be("-$5.00");
        }
    }

    public class TheJsonConverter
    {
        private sealed record Holder(Money Amount);

        [Fact]
        public void GivenMoney_WhenSerialized_ThenIsAPlainNumber()
        {
            JsonSerializer.Serialize(new Holder(Money.Create(60.25m))).Should().Be("""{"Amount":60.25}""");
        }

        [Fact]
        public void GivenANumber_WhenDeserialized_ThenIsMoney()
        {
            JsonSerializer.Deserialize<Holder>("""{"Amount":60.25}""")!.Amount.Should().Be(Money.Create(60.25m));
        }
    }
}
