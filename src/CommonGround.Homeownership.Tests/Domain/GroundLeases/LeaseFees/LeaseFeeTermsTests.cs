using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases.LeaseFees;

public class LeaseFeeTermsTests
{
    public class TheCreateMethod
    {
        [Theory]
        [InlineData(-1, 0, "homeownership.lease_fee_terms.create.land_use_fee_negative")]
        [InlineData(0, -1, "homeownership.lease_fee_terms.create.repair_reserve_fee_negative")]
        public void GivenNegativeFee_WhenCreated_ThenThrowsBusinessRuleViolation(int landUseFee, int repairReserveFee, string expectedCode)
        {
            var act = () => LeaseFeeTerms.Create(Money.Create(landUseFee), Money.Create(repairReserveFee));

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Theory]
        [InlineData(-0.01, 30, "homeownership.lease_fee_terms.create.late_interest_rate_out_of_range")]
        [InlineData(100.01, 30, "homeownership.lease_fee_terms.create.late_interest_rate_out_of_range")]
        [InlineData(10, -1, "homeownership.lease_fee_terms.create.interest_waiver_days_negative")]
        public void GivenInvalidInterestTerms_WhenCreated_ThenThrowsBusinessRuleViolation(double rate, int waiverDays, string expectedCode)
        {
            var act = () => LeaseFeeTerms.Create(Money.Create(45m), Money.Create(15m), (decimal)rate, waiverDays);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Fact]
        public void GivenNoInterestTerms_WhenCreated_ThenChargesNoInterestAndWaivesWithinThirtyDays()
        {
            var sut = LeaseFeeTerms.Create(Money.Create(45m), Money.Create(15m));

            sut.Should().BeEquivalentTo(new { ChargesInterest = false, InterestWaiverDays = 30 });
        }

        [Fact]
        public void GivenBothFees_WhenCreated_ThenChargesTheirSumEachMonth()
        {
            var sut = LeaseFeeTerms.Create(Money.Create(45m), Money.Create(15m));

            sut.MonthlyFee.Amount.Should().Be(60m);
        }
    }
}
