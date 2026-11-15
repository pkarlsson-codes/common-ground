using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases.ResaleFormulas;

public class ResalePriceStatementPriceTests
{
    public class TestBed
    {
        public static readonly DateOnly LeaseStart = new(2020, 3, 1);

        public static readonly DateOnly Today = new(2026, 6, 1);

        public static ResalePriceStatementFigures NoFigures { get; } = new(null, null, null, null);

        public static ResalePriceStatementPrice Price(
            ResalePolicyTerms terms,
            ResalePriceStatementFigures figures,
            DateOnly? statementDate = null,
            decimal? originalAppraisedValue = 260_000m) =>
            ResalePriceStatementPrice.Calculate(
                terms, Money.Create(180_000m), Money.CreateOrNull(originalAppraisedValue), LeaseStart, statementDate ?? Today, Today, figures);
    }

    public class TheCalculateMethod
    {
        [Fact]
        public void GivenIndexBasedLease_WhenCalculated_ThenScalesThePurchasePriceByTheIndexAndKeepsOnlyTheIndexValue()
        {
            var price = TestBed.Price(
                ResalePolicyTerms.IndexBased("CPI-U", 290.0m),
                new ResalePriceStatementFigures(Money.Create(320_000m), "County", 312.5m, null));

            price.Should().BeEquivalentTo(new
            {
                Price = Money.Create(193_965.52m),
                IndexValue = 312.5m,
                MarketValue = (Money?)null,
                MarketValueSource = (string?)null,
            });
        }

        [Fact]
        public void GivenAppraisalBasedLease_WhenCalculated_ThenAddsTheShareOfTheRiseAndKeepsTheMarketValueAndSource()
        {
            var price = TestBed.Price(
                ResalePolicyTerms.AppraisalBased(0.25m),
                new ResalePriceStatementFigures(Money.Create(320_000m), "  County assessment 2026 ", null, null));

            price.Should().BeEquivalentTo(new
            {
                Price = Money.Create(195_000m),
                MarketValue = Money.Create(320_000m),
                MarketValueSource = "County assessment 2026",
                IndexValue = (decimal?)null,
            });
        }

        [Theory]
        [InlineData("2026-03-01", 6, "202709.24")]
        [InlineData("2026-02-28", 5, "198734.54")]
        public void GivenFixedRateLease_WhenCalculated_ThenCompoundsForTheFullYearsOwned(string statementDate, int years, string expected)
        {
            var price = TestBed.Price(ResalePolicyTerms.FixedPercentage(0.02m), TestBed.NoFigures, DateOnly.Parse(statementDate));

            price.YearsOwned.Should().Be(years);
            price.Price.Should().Be(Money.Create(decimal.Parse(expected)));
        }

        [Theory]
        [InlineData(8_000, 5_000)]
        [InlineData(4_000, 4_000)]
        public void GivenImprovementsCap_WhenCalculated_ThenCountsTheCreditUpToTheCap(int credit, int counted)
        {
            var price = TestBed.Price(ResalePolicyTerms.FixedPercentage(0m, 5_000m), new ResalePriceStatementFigures(null, null, null, Money.CreateOrNull(credit)));

            price.ImprovementsCredit.Should().Be(Money.Create(counted));
            price.Price.Should().Be(Money.Create(180_000m + counted));
        }

        [Fact]
        public void GivenNoImprovementsCap_WhenCalculated_ThenIgnoresTheCredit()
        {
            var price = TestBed.Price(ResalePolicyTerms.FixedPercentage(0m), new ResalePriceStatementFigures(null, null, null, Money.Create(4_000m)));

            price.ImprovementsCredit.Should().BeNull();
            price.Price.Should().Be(Money.Create(180_000m));
        }

        [Theory]
        [InlineData("2026-06-02", "statement_date_in_future")]
        [InlineData("2020-02-29", "statement_date_before_lease_start")]
        public void GivenStatementDateOutOfRange_WhenCalculated_ThenThrowsBusinessRuleViolation(string statementDate, string code)
        {
            var act = () => TestBed.Price(ResalePolicyTerms.FixedPercentage(0.02m), TestBed.NoFigures, DateOnly.Parse(statementDate));

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.record_resale_price_statement." + code);
        }

        [Theory]
        [InlineData(null, "County", "market_value_required")]
        [InlineData(0, "County", "market_value_non_positive")]
        [InlineData(320_000, " ", "market_value_source_required")]
        public void GivenAppraisalBasedLeaseWithoutAMarketValue_WhenCalculated_ThenThrowsBusinessRuleViolation(
            int? marketValue, string source, string code)
        {
            var act = () => TestBed.Price(
                ResalePolicyTerms.AppraisalBased(0.25m), new ResalePriceStatementFigures(Money.CreateOrNull(marketValue), source, null, null));

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.record_resale_price_statement." + code);
        }

        [Fact]
        public void GivenAppraisalBasedLeaseWithNoOriginalAppraisedValue_WhenCalculated_ThenThrowsBusinessRuleViolation()
        {
            var act = () => TestBed.Price(
                ResalePolicyTerms.AppraisalBased(0.25m),
                new ResalePriceStatementFigures(Money.Create(320_000m), "County", null, null),
                originalAppraisedValue: null);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.record_resale_price_statement.original_appraised_value_missing");
        }

        [Theory]
        [InlineData(null, "index_value_required")]
        [InlineData(0, "index_value_non_positive")]
        public void GivenIndexBasedLeaseWithoutAnIndexValue_WhenCalculated_ThenThrowsBusinessRuleViolation(int? indexValue, string code)
        {
            var act = () => TestBed.Price(
                ResalePolicyTerms.IndexBased("CPI-U", 290.0m), new ResalePriceStatementFigures(null, null, indexValue, null));

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.record_resale_price_statement." + code);
        }

        [Fact]
        public void GivenNegativeImprovementsCredit_WhenCalculated_ThenThrowsBusinessRuleViolation()
        {
            var act = () => TestBed.Price(
                ResalePolicyTerms.FixedPercentage(0m, 5_000m), new ResalePriceStatementFigures(null, null, null, Money.Create(-1m)));

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.record_resale_price_statement.improvements_credit_negative");
        }
    }

    public class TheNextDueOnMethod
    {
        [Fact]
        public void GivenNoStatementSent_WhenAsked_ThenIsAYearAfterTheLeaseStart() =>
            ResalePriceStatementPrice.NextDueOn(new DateOnly(2020, 3, 1), null).Should().Be(new DateOnly(2021, 3, 1));

        [Fact]
        public void GivenAStatementSent_WhenAsked_ThenIsAYearAfterIt() =>
            ResalePriceStatementPrice.NextDueOn(new DateOnly(2020, 3, 1), new DateOnly(2025, 11, 10)).Should().Be(new DateOnly(2026, 11, 10));
    }
}
