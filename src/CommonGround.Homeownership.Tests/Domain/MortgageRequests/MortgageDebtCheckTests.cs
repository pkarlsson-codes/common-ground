using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.Homeownership.Domain.MortgageRequests;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.MortgageRequests;

public class MortgageDebtCheckTests
{
    public class TheCalculateMethod
    {
        [Fact]
        public void GivenDebtEqualToTheCap_WhenCalculated_ThenItIsNotOverTheCap()
        {
            var check = MortgageDebtCheck.Calculate(Money.Create(220_000m), MortgageDebtCap.Create(90), Money.Create(198_000m));

            check.Should().BeEquivalentTo(new { CapAmount = Money.Create(198_000m), DebtPercent = 90.0m, IsOverCap = false });
        }

        [Fact]
        public void GivenOddFigures_WhenCalculated_ThenRoundsTheCapToTheCentAndThePercentToOnePlace()
        {
            var check = MortgageDebtCheck.Calculate(Money.Create(123_456.78m), MortgageDebtCap.Create(85), Money.Create(100_000.01m));

            check.Should().BeEquivalentTo(new { CapAmount = Money.Create(104_938.26m), DebtPercent = 81.0m, IsOverCap = false });
        }
    }
}
