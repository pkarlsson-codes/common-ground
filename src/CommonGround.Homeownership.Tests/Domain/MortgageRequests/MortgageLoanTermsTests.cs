using CommonGround.Homeownership.Domain.MortgageRequests;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.MortgageRequests;

public class MortgageLoanTermsTests
{
    public class TheCreateMethod
    {
        [Theory]
        [InlineData(" ", 100, 100, 6, 30, "homeownership.mortgage_loan_terms.create.lender_required")]
        [InlineData("Bank", 0, 100, 6, 30, "homeownership.mortgage_loan_terms.create.principal_non_positive")]
        [InlineData("Bank", 100, 99, 6, 30, "homeownership.mortgage_loan_terms.create.total_debt_below_principal")]
        [InlineData("Bank", 100, 100, -0.01, 30, "homeownership.mortgage_loan_terms.create.interest_rate_out_of_range")]
        [InlineData("Bank", 100, 100, 100, 30, "homeownership.mortgage_loan_terms.create.interest_rate_out_of_range")]
        [InlineData("Bank", 100, 100, 6, 0, "homeownership.mortgage_loan_terms.create.term_years_out_of_range")]
        [InlineData("Bank", 100, 100, 6, 51, "homeownership.mortgage_loan_terms.create.term_years_out_of_range")]
        public void GivenInvalidTerms_WhenCreated_ThenThrowsBusinessRuleViolation(
            string lender, int principal, int totalDebt, double rate, int termYears, string expectedCode)
        {
            var act = () => MortgageLoanTerms.Create(
                lender, MortgagePurpose.HomeRepair, Money.Create(principal), Money.Create(totalDebt), (decimal)rate, MortgageRateType.Fixed, termYears);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Fact]
        public void GivenZeroRateRepairLoan_WhenCreated_ThenKeepsTheTrimmedLender()
        {
            var terms = MortgageLoanTerms.Create(
                " City Repair Fund ", MortgagePurpose.HomeRepair, Money.Create(15_000m), Money.Create(160_000m), 0m, MortgageRateType.Fixed, 10);

            terms.Lender.Should().Be("City Repair Fund");
        }
    }
}
