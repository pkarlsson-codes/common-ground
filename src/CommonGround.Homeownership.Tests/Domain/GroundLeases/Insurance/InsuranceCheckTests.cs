using CommonGround.Homeownership.Domain.GroundLeases.Insurance;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases.Insurance;

public class InsuranceCheckTests
{
    public class TestBed
    {
        public static InsuranceCoverage Coverage(decimal dwelling, decimal liability, bool trustNamed) =>
            InsuranceCoverage.Create(
                new DateOnly(2025, 1, 1), new DateOnly(2026, 1, 1), Money.Create(dwelling), Money.Create(liability), trustNamed, new DateOnly(2025, 1, 1));

        /// <summary>Replacement value $280,000, liability $300,000.</summary>
        public InsuranceRequirements Requirements { get; } =
            InsuranceRequirements.Create(Money.Create(280_000m), Money.Create(300_000m));
    }

    public class TheOfMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenCoverageAtEachRequiredAmount_WhenChecked_ThenItMeetsTheLease()
        {
            var sut = InsuranceCheck.Of(TestBed.Coverage(280_000m, 300_000m, true), _testBed.Requirements);

            sut.MeetsLease.Should().BeTrue();
        }

        [Fact]
        public void GivenCoverageShortOnEverything_WhenChecked_ThenListsEachShortfall()
        {
            var sut = InsuranceCheck.Of(TestBed.Coverage(279_999.99m, 100_000m, false), _testBed.Requirements);

            sut.Shortfalls.Should().Equal(
                InsuranceShortfall.DwellingBelowReplacementValue,
                InsuranceShortfall.LiabilityBelowRequired,
                InsuranceShortfall.TrustNotAdditionalInsured);
        }

        [Fact]
        public void GivenNoAmountsRequired_WhenChecked_ThenOnlyTheAdditionalInsuredIsChecked()
        {
            var sut = InsuranceCheck.Of(TestBed.Coverage(0m, 0m, true), InsuranceRequirements.None);

            sut.MeetsLease.Should().BeTrue();
            sut.ReplacementValueChecked.Should().BeFalse();
            sut.LiabilityChecked.Should().BeFalse();
        }
    }
}
