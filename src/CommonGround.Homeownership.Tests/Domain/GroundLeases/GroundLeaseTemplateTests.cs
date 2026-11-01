using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases;

public class GroundLeaseTemplateTests
{
    public class TestBed
    {
        public UserId StaffUser { get; } = UserId.Generate();

        public GroundLeaseTemplateId TemplateId { get; } = GroundLeaseTemplateId.Generate();

        public GroundLeaseTemplateCreated Defined() =>
            GroundLeaseTemplate.Define(TemplateId, "Template", 50, "Terms", ResalePolicyTerms.FixedPercentage(0.03m), StaffUser);

        public GroundLeaseTemplate ActiveTemplate() => EventReplay.Replay<GroundLeaseTemplate>(Defined());

        public GroundLeaseTemplate InactiveTemplate() =>
            EventReplay.Replay<GroundLeaseTemplate>(Defined(), new GroundLeaseTemplateArchived(TemplateId.Value, StaffUser.Value));
    }

    public class TheDefineMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenValidInputs_WhenDefined_ThenStartsAnActiveTemplate()
        {
            var defined = GroundLeaseTemplate.Define(
                _testBed.TemplateId, "Standard 99-Year", 99, "Standard covenant terms", ResalePolicyTerms.AppraisalBased(0.25m, 10000m), _testBed.StaffUser);

            EventReplay.Replay<GroundLeaseTemplate>(defined).Should().BeEquivalentTo(new
            {
                TemplateName = "Standard 99-Year",
                TermYears = 99,
                CovenantTerms = "Standard covenant terms",
                Status = GroundLeaseTemplateStatus.Active,
                CreatedBy = _testBed.StaffUser,
            });
        }

        [Theory]
        [InlineData("", 99, "Terms")]
        [InlineData("Template", 0, "Terms")]
        [InlineData("Template", 99, "  ")]
        public void GivenInvalidInputs_WhenDefined_ThenThrowsBusinessRuleViolation(string name, int termYears, string covenantTerms)
        {
            var act = () => GroundLeaseTemplate.Define(
                _testBed.TemplateId, name, termYears, covenantTerms, ResalePolicyTerms.AppraisalBased(0.25m), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>();
        }
    }

    public class TheArchiveMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenActiveTemplate_WhenArchived_ThenIsInactive()
        {
            var sut = _testBed.ActiveTemplate();

            var (archived, events) = sut.Decide(t => t.Archive(_testBed.StaffUser));

            archived.Status.Should().Be(GroundLeaseTemplateStatus.Inactive);
            events.Should().ContainSingle().Which.Should().BeOfType<GroundLeaseTemplateArchived>()
                .Which.ArchivedBy.Should().Be(_testBed.StaffUser.Value);
        }

        [Fact]
        public void GivenInactiveTemplate_WhenArchived_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.InactiveTemplate();

            var act = () => sut.Archive(_testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>();
        }
    }

    public class TheRestoreMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenInactiveTemplate_WhenRestored_ThenIsActive()
        {
            var sut = _testBed.InactiveTemplate();

            var (restored, events) = sut.Decide(t => t.Restore(_testBed.StaffUser));

            restored.Status.Should().Be(GroundLeaseTemplateStatus.Active);
            events.Should().ContainSingle().Which.Should().BeOfType<GroundLeaseTemplateRestored>()
                .Which.RestoredBy.Should().Be(_testBed.StaffUser.Value);
        }

        [Fact]
        public void GivenActiveTemplate_WhenRestored_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.ActiveTemplate();

            var act = () => sut.Restore(_testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>();
        }
    }
}
