using CommonGround.PropertyRegistry.Domain.DevelopmentProjects;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace CommonGround.PropertyRegistry.Tests.Domain.DevelopmentProjects;

public class DevelopmentProjectTests
{
    public class TestBed
    {
        public UserId StaffUser { get; } = UserId.Generate();
        public LandParcelId ParcelId { get; } = LandParcelId.Generate();
        public HomeId HomeId { get; } = HomeId.Generate();
    }

    public class TheInitiateNewConstructionMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenValidInputs_WhenInitiated_ThenIsInitiatedNewConstructionWithoutHomes()
        {
            var sut = DevelopmentProject.InitiateNewConstruction(_testBed.ParcelId, "Infill", "Two duplexes", _testBed.StaffUser);

            sut.ProjectType.Should().Be(ProjectType.NewConstruction);
            sut.ProjectStatus.Should().Be(ProjectStatus.Initiated);
            sut.HomeIds.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void GivenBlankName_WhenInitiated_ThenThrowsBusinessRuleViolation(string name)
        {
            var act = () => DevelopmentProject.InitiateNewConstruction(_testBed.ParcelId, name: name, "Two duplexes", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("property_registry.development_project.initiate.name_required");
        }

        [Fact]
        public void GivenBlankDescription_WhenInitiated_ThenThrowsBusinessRuleViolation()
        {
            var act = () => DevelopmentProject.InitiateNewConstruction(_testBed.ParcelId, "Infill", description: " ", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("property_registry.development_project.initiate.description_required");
        }
    }

    public class TheInitiateRehabilitationMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenValidInputs_WhenInitiated_ThenIsInitiatedRehabilitationWithHomes()
        {
            var sut = DevelopmentProject.InitiateRehabilitation(
                _testBed.ParcelId, "Roof Rehab", "Replace roofs", [_testBed.HomeId], _testBed.StaffUser);

            sut.ProjectType.Should().Be(ProjectType.Rehabilitation);
            sut.HomeIds.Should().Equal(_testBed.HomeId);
        }

        [Fact]
        public void GivenBlankName_WhenInitiated_ThenThrowsBusinessRuleViolation()
        {
            var act = () => DevelopmentProject.InitiateRehabilitation(
                _testBed.ParcelId, name: "", "Replace roofs", [_testBed.HomeId], _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("property_registry.development_project.initiate.name_required");
        }

        [Fact]
        public void GivenNoHomes_WhenInitiated_ThenThrowsBusinessRuleViolation()
        {
            var act = () => DevelopmentProject.InitiateRehabilitation(
                _testBed.ParcelId, "Roof Rehab", "Replace roofs", homeIds: [], _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("property_registry.development_project.initiate_rehabilitation.home_ids_required");
        }
    }
}
