using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases.LeaseFees;

public class LeaseFeeStatementTests
{
    public class TestBed
    {
        public LeaseFeeTerms Terms { get; } = LeaseFeeTerms.Create(Money.Create(45m), Money.Create(15m));

        /// <summary>Works out a statement with every payment received on <paramref name="asOf"/>.</summary>
        public LeaseFeeStatement Calculate(
            DateOnly leaseStart, DateOnly asOf, DateOnly? endedOn = null, params decimal[] payments) =>
            LeaseFeeStatement.Calculate(Terms, leaseStart, endedOn, payments.Select(a => new ReceivedPayment(asOf, Money.Create(a))), asOf);

        public LeaseFeeStatement CalculateWithDatedPayments(
            DateOnly leaseStart, DateOnly asOf, params (string Received, decimal Amount)[] payments) =>
            LeaseFeeStatement.Calculate(Terms, leaseStart, null, Dated(payments), asOf);

        /// <summary>$60 a month at 36.5% a year: 6 cents a day of interest on an unpaid month.</summary>
        public LeaseFeeTerms TermsWithInterest { get; } = LeaseFeeTerms.Create(Money.Create(45m), Money.Create(15m), 36.5m);

        /// <summary>A statement for a lease that started on 2026-01-01 and charges interest.</summary>
        public LeaseFeeStatement CalculateWithInterest(
            string asOf,
            (string Received, decimal Amount)[]? payments = null,
            string? endedOn = null,
            params (string WaivedOn, decimal Amount)[] waivers) =>
            LeaseFeeStatement.Calculate(
                TermsWithInterest,
                new DateOnly(2026, 1, 1),
                endedOn is null ? null : Day(endedOn),
                Dated(payments ?? []),
                Day(asOf),
                waivers.Select(w => new WaivedInterest(Day(w.WaivedOn), Money.Create(w.Amount))));

        private static DateOnly Day(string day) => DateOnly.Parse(day, System.Globalization.CultureInfo.InvariantCulture);

        private static IEnumerable<ReceivedPayment> Dated((string Received, decimal Amount)[] payments) =>
            payments.Select(p => new ReceivedPayment(Day(p.Received), Money.Create(p.Amount)));
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

            sut.Should().BeEquivalentTo(new { ChargeCount = 0, Balance = Money.Create(0m), OldestUnpaidDueDate = (DateOnly?)null, DaysOverdue = 0 });
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
                TotalCharged = Money.Create(180m),
                Balance = Money.Create(90m),
                OldestUnpaidDueDate = (DateOnly?)new DateOnly(2026, 2, 1),
                DaysOverdue = 38,
            });
        }

        [Fact]
        public void GivenOverpayment_WhenCalculated_ThenShowsACreditAndNothingOverdue()
        {
            var sut = _testBed.Calculate(new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 10), null, 200m);

            sut.Should().BeEquivalentTo(new { Balance = Money.Create(-80m), OldestUnpaidDueDate = (DateOnly?)null, DaysOverdue = 0 });
        }

        [Fact]
        public void GivenChargeDueToday_WhenCalculated_ThenItIsOwedButNotYetOverdue()
        {
            var sut = _testBed.Calculate(new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 1), null, 60m);

            sut.Should().BeEquivalentTo(new { Balance = Money.Create(60m), OldestUnpaidDueDate = (DateOnly?)new DateOnly(2026, 2, 1), DaysOverdue = 0 });
        }

        [Fact]
        public void GivenNoFee_WhenCalculated_ThenNothingIsEverCharged()
        {
            var sut = LeaseFeeStatement.Calculate(LeaseFeeTerms.None, new DateOnly(2020, 1, 1), null, [], new DateOnly(2026, 1, 1));

            sut.Should().BeEquivalentTo(new { ChargeCount = 0, Balance = Money.Create(0m), OldestUnpaidDueDate = (DateOnly?)null });
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

            sut.Should().BeEquivalentTo(new { Balance = Money.Create(0m), ArrearsSince = (DateOnly?)null });
        }

        [Fact]
        public void GivenPaymentReceivedAfterAsOf_WhenCalculated_ThenItIsLeftOut()
        {
            var sut = _testBed.CalculateWithDatedPayments(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 20), ("2026-01-25", 60m));

            sut.Should().BeEquivalentTo(new { Balance = Money.Create(60m), ArrearsSince = (DateOnly?)new DateOnly(2026, 1, 1) });
        }
    }

    public class TheInterest
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenFeePaidWithinTheWaiverDays_WhenCalculated_ThenNoInterestIsOwed()
        {
            var sut = _testBed.CalculateWithInterest("2026-01-31", [("2026-01-31", 60m)]);

            sut.Should().BeEquivalentTo(new { InterestCharged = Money.Create(0m), InterestAccruing = Money.Create(0m), Balance = Money.Create(0m) });
        }

        [Fact]
        public void GivenFeeUnpaidWithinTheWaiverDays_WhenCalculated_ThenInterestIsAccruingButNotInTheBalance()
        {
            var sut = _testBed.CalculateWithInterest("2026-01-31");

            sut.Should().BeEquivalentTo(new
            {
                InterestCharged = Money.Create(0m),
                InterestAccruing = Money.Create(1.80m),
                Balance = Money.Create(60m),
                InterestWaivedIfPaidBy = (DateOnly?)new DateOnly(2026, 1, 31),
            });
        }

        [Fact]
        public void GivenFeeUnpaidPastTheWaiverDays_WhenCalculated_ThenInterestFromTheDueDateIsInTheBalance()
        {
            var sut = _testBed.CalculateWithInterest("2026-02-01");

            sut.Should().BeEquivalentTo(new
            {
                InterestCharged = Money.Create(1.86m),
                InterestDue = Money.Create(1.86m),
                Balance = Money.Create(121.86m),
                InterestWaivedIfPaidBy = (DateOnly?)null,
            });
        }

        [Fact]
        public void GivenFeePaidPastTheWaiverDays_WhenCalculated_ThenInterestRunsToTheDayItWasPaid()
        {
            var sut = _testBed.CalculateWithInterest("2026-02-10", [("2026-02-10", 60m)]);

            sut.Should().BeEquivalentTo(new
            {
                InterestCharged = Money.Create(2.40m),
                InterestAccruing = Money.Create(0.54m),
                Balance = Money.Create(62.40m),
                InterestWaivedIfPaidBy = (DateOnly?)new DateOnly(2026, 3, 3),
            });
        }

        [Fact]
        public void GivenPartPaidWithinTheWaiverDays_WhenCalculated_ThenOnlyTheLatePartEarnsInterest()
        {
            var sut = _testBed.CalculateWithInterest("2026-02-20", [("2026-01-20", 30m), ("2026-02-20", 30m)]);

            sut.InterestCharged.Amount.Should().Be(1.50m);
        }

        [Fact]
        public void GivenPaymentAboveTheFees_WhenCalculated_ThenTheRestPaysTheInterest()
        {
            var sut = _testBed.CalculateWithInterest("2026-02-10", [("2026-02-10", 122.40m)]);

            sut.Balance.Amount.Should().Be(0m);
        }

        [Fact]
        public void GivenEndedLease_WhenCalculated_ThenInterestStopsOnTheDayItEnded()
        {
            var sut = _testBed.CalculateWithInterest("2026-03-01", endedOn: "2026-01-20");

            sut.InterestCharged.Amount.Should().Be(1.14m);
        }

        [Fact]
        public void GivenWaivers_WhenCalculated_ThenThoseUpToTheDayLowerTheInterestDue()
        {
            var sut = _testBed.CalculateWithInterest("2026-02-01", null, null, ("2026-02-01", 1m), ("2026-02-02", 0.86m));

            sut.Should().BeEquivalentTo(new { InterestWaived = Money.Create(1m), InterestDue = Money.Create(0.86m), Balance = Money.Create(120.86m) });
        }

        [Fact]
        public void GivenNoInterestRate_WhenFeeIsPaidLate_ThenNoInterestIsOwed()
        {
            var sut = _testBed.CalculateWithDatedPayments(new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 1), ("2026-02-20", 60m));

            sut.Should().BeEquivalentTo(new { InterestCharged = Money.Create(0m), InterestAccruing = Money.Create(0m) });
        }
    }
}
