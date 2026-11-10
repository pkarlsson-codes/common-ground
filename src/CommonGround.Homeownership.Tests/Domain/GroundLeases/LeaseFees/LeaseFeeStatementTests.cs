using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases.LeaseFees;

public class LeaseFeeStatementTests
{
    public class TestBed
    {
        public LeaseFeeTerms Terms { get; } = LeaseFeeTerms.Create(45m, 15m);

        public LeaseFeeStatement Calculate(
            DateOnly leaseStart, DateOnly asOf, DateOnly? endedOn = null, params decimal[] payments) =>
            LeaseFeeStatement.Calculate(Terms, leaseStart, endedOn, payments, asOf);
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
}
