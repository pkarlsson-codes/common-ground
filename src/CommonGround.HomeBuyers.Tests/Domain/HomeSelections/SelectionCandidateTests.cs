using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
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
                HomeownerApplicationId.Generate(), HouseholdId.Generate(), annualIncome: -1m, 2, false, new DateOnly(2026, 6, 1));

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void GivenEmptyHousehold_WhenCreated_ThenThrowsArgumentOutOfRange()
        {
            var act = () => SelectionCandidate.Create(
                HomeownerApplicationId.Generate(), HouseholdId.Generate(), 55000m, householdSize: 0, false, new DateOnly(2026, 6, 1));

            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
