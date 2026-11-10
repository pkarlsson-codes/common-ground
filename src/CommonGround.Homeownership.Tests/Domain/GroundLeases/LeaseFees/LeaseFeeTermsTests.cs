using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
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
            var act = () => LeaseFeeTerms.Create(landUseFee, repairReserveFee);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Fact]
        public void GivenBothFees_WhenCreated_ThenChargesTheirSumEachMonth()
        {
            var sut = LeaseFeeTerms.Create(45m, 15m);

            sut.MonthlyFee.Should().Be(60m);
        }
    }
}
