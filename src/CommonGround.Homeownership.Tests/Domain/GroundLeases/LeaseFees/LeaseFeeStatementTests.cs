using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases.LeaseFees;

public class LeaseFeeStatementTests
{
    public class TestBed
    {
        public LeaseFeeTerms Terms { get; } = LeaseFeeTerms.Create(45m, 15m);

        /// <summary>Works out a statement with every payment received on <paramref name="asOf"/>.</summary>
        public LeaseFeeStatement Calculate(
            DateOnly leaseStart, DateOnly asOf, DateOnly? endedOn = null, params decimal[] payments) =>
            LeaseFeeStatement.Calculate(Terms, leaseStart, endedOn, payments.Select(a => new ReceivedPayment(asOf, a)), asOf);

        public LeaseFeeStatement CalculateWithDatedPayments(
            DateOnly leaseStart, DateOnly asOf, params (string Received, decimal Amount)[] payments) =>
            LeaseFeeStatement.Calculate(
                Terms, leaseStart, null, payments.Select(p => new ReceivedPayment(DateOnly.Parse(p.Received, System.Globalization.CultureInfo.InvariantCulture), p.Amount)), asOf);
    }

    public class TheCalculateMethod
    {
        private readonly TestBed _testBed = new();

        [Theory]
        [InlineData(1, 3)]
        [InlineData(2, 2)]
        public void GivenLeaseStart_WhenCalculated_ThenChargesFromTheFirstFirstOnOrAfterIt(int startDay, int expectedCharges)
        {
            var sut = _testBed.Calculate(new DateOnly(2026, 1, startDay), asOf: new DateOnly(2026, 3, 15));

            sut.ChargeCount.Should().Be(expectedCharges);
        }

        [Fact]
        public void GivenLeaseStartedThisMonthAfterThe1st_WhenCalculated_ThenNothingIsDueYet()
        {
            var sut = _testBed.Calculate(new DateOnly(2026, 3, 5), asOf: new DateOnly(2026, 3, 20));

            sut.Should().BeEquivalentTo(new { ChargeCount = 0, Balance = 0m, OldestUnpaidDueDate = (DateOnly?)null, DaysOverdue = 0 });
        }

        [Fact]
        public void GivenEndedLease_WhenCalculated_ThenStopsChargingAfterItEnded()
        {
            var sut = _testBed.Calculate(new DateOnly(2026, 1, 1), asOf: new DateOnly(2026, 12, 1), endedOn: new DateOnly(2026, 4, 20));

            sut.ChargeCount.Should().Be(4);
        }

        [Fact]
        public void GivenPartialPayments_WhenCalculated_ThenTheOldestChargeNotFullyPaidIsOverdue()
        {
            var sut = _testBed.Calculate(new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 11), null, 60m, 30m);

            sut.Should().BeEquivalentTo(new
            {
                TotalCharged = 180m,
                Balance = 90m,
                OldestUnpaidDueDate = (DateOnly?)new DateOnly(2026, 2, 1),
                DaysOverdue = 38,
            });
        }

        [Fact]
        public void GivenOverpayment_WhenCalculated_ThenShowsACreditAndNothingOverdue()
        {
            var sut = _testBed.Calculate(new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 10), null, 200m);

            sut.Should().BeEquivalentTo(new { Balance = -80m, OldestUnpaidDueDate = (DateOnly?)null, DaysOverdue = 0 });
        }

        [Fact]
        public void GivenChargeDueToday_WhenCalculated_ThenItIsOwedButNotYetOverdue()
        {
            var sut = _testBed.Calculate(new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 1), null, 60m);

            sut.Should().BeEquivalentTo(new { Balance = 60m, OldestUnpaidDueDate = (DateOnly?)new DateOnly(2026, 2, 1), DaysOverdue = 0 });
        }

        [Fact]
        public void GivenNoFee_WhenCalculated_ThenNothingIsEverCharged()
        {
            var sut = LeaseFeeStatement.Calculate(LeaseFeeTerms.None, new DateOnly(2020, 1, 1), null, [], new DateOnly(2026, 1, 1));

            sut.Should().BeEquivalentTo(new { ChargeCount = 0, Balance = 0m, OldestUnpaidDueDate = (DateOnly?)null });
        }
    }

    public class TheArrearsSinceProperty
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenLeasePaidUpThenAMonthMissed_WhenCalculated_ThenArrearsBeginAtTheMissedMonth()
        {
            var sut = _testBed.CalculateWithDatedPayments(
                new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 20), ("2026-01-03", 60m), ("2026-02-03", 60m));

            sut.ArrearsSince.Should().Be(new DateOnly(2026, 3, 1));
        }

        [Fact]
        public void GivenPartialPaymentsThatNeverCatchUp_WhenCalculated_ThenArrearsBeginBeforeTheOldestUnpaidMonth()
        {
            var sut = _testBed.CalculateWithDatedPayments(new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 20), ("2026-02-05", 60m));

            sut.Should().BeEquivalentTo(new
            {
                ArrearsSince = (DateOnly?)new DateOnly(2026, 1, 1),
                OldestUnpaidDueDate = (DateOnly?)new DateOnly(2026, 2, 1),
            });
        }

        [Fact]
        public void GivenPaymentOnTheDueDate_WhenCalculated_ThenItPaysThatMonth()
        {
            var sut = _testBed.CalculateWithDatedPayments(
                new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 20), ("2026-01-01", 60m), ("2026-02-01", 60m));

            sut.Should().BeEquivalentTo(new { Balance = 0m, ArrearsSince = (DateOnly?)null });
        }

        [Fact]
        public void GivenPaymentReceivedAfterAsOf_WhenCalculated_ThenItIsLeftOut()
        {
            var sut = _testBed.CalculateWithDatedPayments(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 20), ("2026-01-25", 60m));

            sut.Should().BeEquivalentTo(new { Balance = 60m, ArrearsSince = (DateOnly?)new DateOnly(2026, 1, 1) });
        }
    }
}
