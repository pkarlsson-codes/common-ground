using CommonGround.SharedKernel.Domain;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases;

public class ResalePriceCalculatorTests
{
    private static readonly ResaleCalculationInput StandardInput = new(
        OriginalPurchasePrice: Money.Create(200_000m),
        CurrentAppraisalValue: Money.Create(300_000m),
        OriginalAppraisalValue: Money.Create(200_000m),
        OwnershipYears: 5,
        ApprovedImprovements: Money.Create(15_000m),
        CurrentIndexValue: null);

    // --- Appraisal-based parity ---

    [Fact]
    public void AppraisalBased_CalculatesCorrectly()
    {
        var terms = ResalePolicyTerms.AppraisalBased(0.25m);
        var input = StandardInput with { ApprovedImprovements = null };

        ResalePriceCalculator.Calculate(terms, input).Should().Be(225_000m);
    }

    [Fact]
    public void AppraisalBased_ZeroAppreciation_ReturnsPurchasePrice()
    {
        var terms = ResalePolicyTerms.AppraisalBased(0.25m);
        var input = StandardInput with { CurrentAppraisalValue = Money.Create(200_000m), ApprovedImprovements = null };

        ResalePriceCalculator.Calculate(terms, input).Should().Be(200_000m);
    }

    // --- Index-based parity ---

    [Fact]
    public void IndexBased_CalculatesCorrectly()
    {
        var terms = ResalePolicyTerms.IndexBased("CPI", 100m);
        var input = StandardInput with { ApprovedImprovements = null, CurrentIndexValue = 150m };

        ResalePriceCalculator.Calculate(terms, input).Should().Be(300_000m);
    }

    [Fact]
    public void IndexBased_NoChange_ReturnsPurchasePrice()
    {
        var terms = ResalePolicyTerms.IndexBased("CPI", 100m);
        var input = StandardInput with { ApprovedImprovements = null, CurrentIndexValue = 100m };

        ResalePriceCalculator.Calculate(terms, input).Should().Be(200_000m);
    }

    [Fact]
    public void IndexBased_WithoutCurrentIndexValue_Throws()
    {
        var terms = ResalePolicyTerms.IndexBased("CPI", 100m);

        var act = () => ResalePriceCalculator.Calculate(terms, StandardInput);

        act.Should().Throw<BusinessRuleViolationException>()
            .Which.Code.Should().Be("homeownership.resale_policy_terms.index_based.current_index_value_required");
    }

    // --- Fixed-percentage parity ---

    [Fact]
    public void FixedPercentage_CalculatesCorrectly()
    {
        var terms = ResalePolicyTerms.FixedPercentage(0.03m);
        var input = StandardInput with { ApprovedImprovements = null };

        ResalePriceCalculator.Calculate(terms, input).Should().BeApproximately(231_854.81m, 0.01m);
    }

    [Fact]
    public void FixedPercentage_ZeroYears_ReturnsPurchasePrice()
    {
        var terms = ResalePolicyTerms.FixedPercentage(0.03m);
        var input = StandardInput with { OwnershipYears = 0, ApprovedImprovements = null };

        ResalePriceCalculator.Calculate(terms, input).Should().Be(200_000m);
    }

    // --- Improvements cap parity ---

    [Fact]
    public void ImprovementsCap_CapsAtMaximum()
    {
        var terms = ResalePolicyTerms.AppraisalBased(0.25m, improvementsCap: 10_000m);

        ResalePriceCalculator.Calculate(terms, StandardInput).Should().Be(235_000m);
    }

    [Fact]
    public void ImprovementsCap_BelowCap_AddsAll()
    {
        var terms = ResalePolicyTerms.AppraisalBased(0.25m, improvementsCap: 20_000m);

        ResalePriceCalculator.Calculate(terms, StandardInput).Should().Be(240_000m);
    }

    [Fact]
    public void ImprovementsCap_NoImprovements_ReturnsBasePrice()
    {
        var terms = ResalePolicyTerms.AppraisalBased(0.25m, improvementsCap: 10_000m);
        var input = StandardInput with { ApprovedImprovements = null };

        ResalePriceCalculator.Calculate(terms, input).Should().Be(225_000m);
    }
}
