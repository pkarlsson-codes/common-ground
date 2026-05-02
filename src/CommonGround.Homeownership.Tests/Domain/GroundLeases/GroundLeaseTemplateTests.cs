using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.GroundLeases.Templates;
using CommonGround.Homeownership.Domain.GroundLeases.Templates.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases;

public class GroundLeaseTemplateTests
{
    private static readonly UserId StaffUser = UserId.Generate();

    [Fact]
    public void Create_ValidInputs_CreatesActiveTemplate()
    {
        var template = GroundLeaseTemplate.Create(
            "Standard 99-Year",
            99,
            "Standard covenant terms",
            ResalePolicyTerms.AppraisalBased(0.25m, 10000m),
            StaffUser);

        template.TemplateName.Should().Be("Standard 99-Year");
        template.TermYears.Should().Be(99);
        template.CovenantTerms.Should().Be("Standard covenant terms");
        template.Status.Should().Be(GroundLeaseTemplateStatus.Active);
        template.CreatedBy.Should().Be(StaffUser);
    }

    [Fact]
    public void Create_EmptyTemplateName_Throws()
    {
        var act = () => GroundLeaseTemplate.Create(
            "",
            99,
            "Terms",
            ResalePolicyTerms.AppraisalBased(0.25m),
            StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_NonPositiveTermYears_Throws()
    {
        var act = () => GroundLeaseTemplate.Create(
            "Template",
            0,
            "Terms",
            ResalePolicyTerms.AppraisalBased(0.25m),
            StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_EmptyCovenantTerms_Throws()
    {
        var act = () => GroundLeaseTemplate.Create(
            "Template",
            99,
            "  ",
            ResalePolicyTerms.AppraisalBased(0.25m),
            StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_EmitsGroundLeaseTemplateCreatedEvent()
    {
        var template = GroundLeaseTemplate.Create(
            "Template",
            50,
            "Terms",
            ResalePolicyTerms.FixedPercentage(0.03m),
            StaffUser);

        template.UncommittedEvents.Should().HaveCount(1);
        template.UncommittedEvents.Single().Should().BeOfType<GroundLeaseTemplateCreated>();
    }

    [Fact]
    public void Deactivate_ActiveTemplate_SetsInactive()
    {
        var template = GroundLeaseTemplate.Create(
            "Template",
            50,
            "Terms",
            ResalePolicyTerms.FixedPercentage(0.03m),
            StaffUser);

        template.Deactivate(StaffUser);

        template.Status.Should().Be(GroundLeaseTemplateStatus.Inactive);
    }

    [Fact]
    public void Deactivate_AlreadyInactive_Throws()
    {
        var template = GroundLeaseTemplate.Create(
            "Template",
            50,
            "Terms",
            ResalePolicyTerms.FixedPercentage(0.03m),
            StaffUser);
        template.Deactivate(StaffUser);

        var act = () => template.Deactivate(StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Activate_InactiveTemplate_SetsActive()
    {
        var template = GroundLeaseTemplate.Create(
            "Template",
            50,
            "Terms",
            ResalePolicyTerms.FixedPercentage(0.03m),
            StaffUser);
        template.Deactivate(StaffUser);

        template.Activate(StaffUser);

        template.Status.Should().Be(GroundLeaseTemplateStatus.Active);
    }

    [Fact]
    public void Activate_AlreadyActive_Throws()
    {
        var template = GroundLeaseTemplate.Create(
            "Template",
            50,
            "Terms",
            ResalePolicyTerms.FixedPercentage(0.03m),
            StaffUser);

        var act = () => template.Activate(StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Deactivate_EmitsArchivedEvent()
    {
        var template = GroundLeaseTemplate.Create(
            "Template",
            50,
            "Terms",
            ResalePolicyTerms.FixedPercentage(0.03m),
            StaffUser);

        template.Deactivate(StaffUser);

        var evt = template.UncommittedEvents.OfType<GroundLeaseTemplateArchived>().Single();
        evt.ArchivedBy.Should().Be(StaffUser.Value);
    }

    [Fact]
    public void Activate_EmitsRestoredEvent()
    {
        var template = GroundLeaseTemplate.Create(
            "Template",
            50,
            "Terms",
            ResalePolicyTerms.FixedPercentage(0.03m),
            StaffUser);
        template.Deactivate(StaffUser);

        template.Activate(StaffUser);

        var evt = template.UncommittedEvents.OfType<GroundLeaseTemplateRestored>().Single();
        evt.RestoredBy.Should().Be(StaffUser.Value);
    }
}
