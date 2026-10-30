using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Domain.HomeSelections;

public class SelectionCandidateTests
{
    public class TestBed
    {
        public SelectionCandidate Create(decimal annualIncome = 55000m, int householdSize = 2) =>
            SelectionCandidate.Create(
                HomeownerApplicationId.Generate(),
                HouseholdId.Generate(),
                annualIncome,
                householdSize,
                false,
                new DateOnly(2026, 6, 1));
    }

    public class TheCreateMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenNegativeIncome_WhenCreated_ThenThrowsArgumentOutOfRange()
        {
            var act = () => _testBed.Create(annualIncome: -1m);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void GivenEmptyHousehold_WhenCreated_ThenThrowsArgumentOutOfRange()
        {
            var act = () => _testBed.Create(householdSize: 0);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
