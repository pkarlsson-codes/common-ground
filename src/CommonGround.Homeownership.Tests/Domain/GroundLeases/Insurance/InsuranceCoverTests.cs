using CommonGround.Homeownership.Domain.GroundLeases.Insurance;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases.Insurance;

public class InsuranceCoverTests
{
    public class TestBed
    {
        public static readonly DateOnly LeaseStart = new(2025, 1, 1);

        /// <summary>Replacement value $280,000, liability $300,000, from the lease start.</summary>
        public InsuranceRequirementsHistory Requirements { get; } = InsuranceRequirementsHistory.Start(
            LeaseStart, InsuranceRequirements.Create(Money.Create(280_000m), Money.Create(300_000m)));

        public static InsuranceCoverage Coverage(
            DateOnly policyStart, DateOnly policyEnd, decimal dwelling = 280_000m, DateOnly? receivedOn = null) =>
            InsuranceCoverage.Create(
                policyStart, policyEnd, Money.Create(dwelling), Money.Create(300_000m), true, receivedOn ?? policyStart);
    }

    public class TheOfMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenNoCertificates_WhenWorkedOut_ThenTheFirstIsDueOnTheLeaseStart()
        {
            var sut = InsuranceCover.Of(TestBed.LeaseStart, [], _testBed.Requirements);

            sut.CoveredThrough.Should().BeNull();
            sut.DueOn.Should().Be(TestBed.LeaseStart);
            sut.Check.Should().BeNull();
        }

        [Fact]
        public void GivenCertificatesOutOfOrder_WhenWorkedOut_ThenTheLatestPolicyEndIsCurrent()
        {
            var later = TestBed.Coverage(new DateOnly(2026, 1, 1), new DateOnly(2027, 1, 1));
            var earlier = TestBed.Coverage(new DateOnly(2025, 1, 1), new DateOnly(2026, 1, 1));

            var sut = InsuranceCover.Of(TestBed.LeaseStart, [later, earlier], _testBed.Requirements);

            sut.Current.Should().Be(later);
            sut.DueOn.Should().Be(new DateOnly(2027, 1, 1));
        }

        [Fact]
        public void GivenTwoCertificatesEndingTheSameDay_WhenWorkedOut_ThenTheLatestReceivedIsCurrent()
        {
            var first = TestBed.Coverage(TestBed.LeaseStart, new DateOnly(2026, 1, 1), dwelling: 250_000m, receivedOn: new DateOnly(2025, 1, 1));
            var amended = TestBed.Coverage(TestBed.LeaseStart, new DateOnly(2026, 1, 1), receivedOn: new DateOnly(2025, 2, 1));

            var sut = InsuranceCover.Of(TestBed.LeaseStart, [amended, first], _testBed.Requirements);

            sut.Current.Should().Be(amended);
            sut.IsShort.Should().BeFalse();
        }

        [Fact]
        public void GivenAnAmendedCertificateReceivedTheSameDay_WhenWorkedOut_ThenTheLaterRecordedIsCurrent()
        {
            var first = TestBed.Coverage(TestBed.LeaseStart, new DateOnly(2026, 1, 1), dwelling: 250_000m);
            var amended = TestBed.Coverage(TestBed.LeaseStart, new DateOnly(2026, 1, 1));

            var sut = InsuranceCover.Of(TestBed.LeaseStart, [first, amended], _testBed.Requirements);

            sut.Current.Should().Be(amended);
        }

        [Fact]
        public void GivenRequirementsRaisedAfterThePolicyStart_WhenWorkedOut_ThenTheCurrentCertificateIsCheckedAgainstTheOldOnes()
        {
            var requirements = _testBed.Requirements.With(InsuranceRequirementsChange.Create(
                new DateOnly(2025, 6, 1), InsuranceRequirements.Create(Money.Create(320_000m), Money.Create(300_000m))));
            var coverage = TestBed.Coverage(TestBed.LeaseStart, new DateOnly(2026, 1, 1));

            var sut = InsuranceCover.Of(TestBed.LeaseStart, [coverage], requirements);

            sut.Check!.MeetsLease.Should().BeTrue();
        }

        [Fact]
        public void GivenRequirementsRaisedOnThePolicyStart_WhenWorkedOut_ThenTheCurrentCertificateIsShort()
        {
            var requirements = _testBed.Requirements.With(InsuranceRequirementsChange.Create(
                new DateOnly(2026, 1, 1), InsuranceRequirements.Create(Money.Create(320_000m), Money.Create(300_000m))));
            var coverage = TestBed.Coverage(new DateOnly(2026, 1, 1), new DateOnly(2027, 1, 1));

            var sut = InsuranceCover.Of(TestBed.LeaseStart, [coverage], requirements);

            sut.Check!.Shortfalls.Should().Equal(InsuranceShortfall.DwellingBelowReplacementValue);
        }
    }

    public class TheIsCertificateDueOnMethod
    {
        private readonly TestBed _testBed = new();

        [Theory]
        [InlineData("2025-12-01", false)]
        [InlineData("2025-12-02", true)]
        [InlineData("2026-02-01", true)]
        public void GivenCoverThroughJanuary1_WhenAskedOnADay_ThenDueFrom30DaysBefore(string today, bool due)
        {
            var coverage = TestBed.Coverage(TestBed.LeaseStart, new DateOnly(2026, 1, 1));
            var sut = InsuranceCover.Of(TestBed.LeaseStart, [coverage], _testBed.Requirements);

            sut.IsCertificateDueOn(DateOnly.Parse(today, System.Globalization.CultureInfo.InvariantCulture)).Should().Be(due);
        }
    }
}
