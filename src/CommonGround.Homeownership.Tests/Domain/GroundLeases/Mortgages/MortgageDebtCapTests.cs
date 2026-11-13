using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases.Mortgages;

public class MortgageDebtCapTests
{
    public class TheCreateMethod
    {
        [Theory]
        [InlineData(0)]
        [InlineData(201)]
        public void GivenPercentOutsideOneTo200_WhenCreated_ThenThrowsBusinessRuleViolation(int percent)
        {
            var act = () => MortgageDebtCap.Create(percent);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.mortgage_debt_cap.create.percent_out_of_range");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(200)]
        public void GivenPercentAtTheLimits_WhenCreated_ThenKeepsIt(int percent)
        {
            MortgageDebtCap.Create(percent).Percent.Should().Be(percent);
        }
    }
}
