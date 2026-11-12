using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Domain.HomeSelections;

public class SelectionCandidateTests
{
    public class TheCreateMethod
    {
        [Fact]
        public void GivenNegativeIncome_WhenCreated_ThenThrowsArgumentOutOfRange()
        {
            var act = () => SelectionCandidate.Create(
                HomeownerApplicationId.Generate(), HouseholdId.Generate(), annualIncome: Money.Create(-1m), 2, false, new DateOnly(2026, 6, 1));

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void GivenEmptyHousehold_WhenCreated_ThenThrowsArgumentOutOfRange()
        {
            var act = () => SelectionCandidate.Create(
                HomeownerApplicationId.Generate(), HouseholdId.Generate(), Money.Create(55000m), householdSize: 0, false, new DateOnly(2026, 6, 1));

            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
