using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases;

public class ResalePolicyTermsTests
{
    [Fact]
    public void AppraisalBased_ValidShare_Creates()
    {
        var terms = ResalePolicyTerms.AppraisalBased(0.25m);

        terms.FormulaKind.Should().Be(FormulaKind.AppraisalBased);
        terms.AppreciationShare.Should().Be(0.25m);
        terms.IndexType.Should().BeNull();
        terms.BaselineIndexValue.Should().BeNull();
        terms.AnnualPercentage.Should().BeNull();
        terms.ImprovementsCap.Should().BeNull();
    }

    [Fact]
    public void AppraisalBased_WithImprovementsCap_Creates()
    {
        var terms = ResalePolicyTerms.AppraisalBased(0.25m, improvementsCap: 12_000m);

        terms.ImprovementsCap.Should().Be(12_000m);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void AppraisalBased_InvalidShare_Throws(decimal share)
    {
        var act = () => ResalePolicyTerms.AppraisalBased(share);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void IndexBased_ValidInputs_Creates()
    {
        var terms = ResalePolicyTerms.IndexBased("CPI", 100m);

        terms.FormulaKind.Should().Be(FormulaKind.IndexBased);
        terms.IndexType.Should().Be("CPI");
        terms.BaselineIndexValue.Should().Be(100m);
    }

    [Fact]
    public void IndexBased_EmptyType_Throws()
    {
        var act = () => ResalePolicyTerms.IndexBased("", 100m);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void IndexBased_ZeroBaseline_Throws()
    {
        var act = () => ResalePolicyTerms.IndexBased("CPI", 0m);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void FixedPercentage_ValidRate_Creates()
    {
        var terms = ResalePolicyTerms.FixedPercentage(0.03m);

        terms.FormulaKind.Should().Be(FormulaKind.FixedPercentage);
        terms.AnnualPercentage.Should().Be(0.03m);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void FixedPercentage_InvalidRate_Throws(decimal rate)
    {
        var act = () => ResalePolicyTerms.FixedPercentage(rate);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void ImprovementsCap_ZeroOrNegative_Throws()
    {
        var act = () => ResalePolicyTerms.AppraisalBased(0.25m, improvementsCap: 0m);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Equality_SameTerms_AreEqual()
    {
        var a = ResalePolicyTerms.AppraisalBased(0.25m, 10_000m);
        var b = ResalePolicyTerms.AppraisalBased(0.25m, 10_000m);
        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentKinds_AreNotEqual()
    {
        var a = ResalePolicyTerms.AppraisalBased(0.25m);
        var b = ResalePolicyTerms.FixedPercentage(0.25m);
        a.Should().NotBe(b);
    }
}
