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
                HomeownerApplicationId.Generate(), HouseholdId.Generate(), annualIncome: Money.Create(-1m), 2, false, new DateOnly(2026, 6, 1), incomeLimit: null);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void GivenEmptyHousehold_WhenCreated_ThenThrowsArgumentOutOfRange()
        {
            var act = () => SelectionCandidate.Create(
                HomeownerApplicationId.Generate(), HouseholdId.Generate(), Money.Create(55000m), householdSize: 0, false, new DateOnly(2026, 6, 1), incomeLimit: null);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }

    public class TheIsOverIncomeLimitProperty
    {
        [Theory]
        [InlineData(70000, 70001, false)]
        [InlineData(70000, 70000, false)]
        [InlineData(70000, 69999, true)]
        public void GivenIncomeLimit_WhenRead_ThenIsTrueOnlyWhenIncomeIsAboveIt(decimal annualIncome, decimal incomeLimit, bool expected)
        {
            var sut = Candidate(annualIncome, Money.Create(incomeLimit));

            sut.IsOverIncomeLimit.Should().Be(expected);
        }

        [Fact]
        public void GivenNoIncomeLimit_WhenRead_ThenIsFalse()
        {
            var sut = Candidate(1_000_000m, incomeLimit: null);

            sut.IsOverIncomeLimit.Should().BeFalse();
        }

        private static SelectionCandidate Candidate(decimal annualIncome, Money? incomeLimit) =>
            SelectionCandidate.Create(
                HomeownerApplicationId.Generate(), HouseholdId.Generate(), Money.Create(annualIncome), 2, false, new DateOnly(2026, 6, 1), incomeLimit);
    }
}
