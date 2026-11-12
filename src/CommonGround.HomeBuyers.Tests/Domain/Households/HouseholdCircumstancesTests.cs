using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Domain.Households;

public class HouseholdCircumstancesTests
{
    public class TheCreateMethod
    {
        [Fact]
        public void GivenNegativeAnnualIncome_WhenCreated_ThenThrowsAnnualIncomeNegative()
        {
            var act = () => HouseholdCircumstances.Create(false, Money.Create(-0.01m));

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.household.circumstances.annual_income_negative");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(65000)]
        public void GivenNonNegativeAnnualIncome_WhenCreated_ThenSucceeds(decimal annualIncome)
        {
            var sut = HouseholdCircumstances.Create(true, Money.Create(annualIncome));

            sut.AnnualIncome.Amount.Should().Be(annualIncome);
        }
    }
}
