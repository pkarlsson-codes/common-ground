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

        public DevelopmentProject InitiateNewConstruction(string name = "Infill", string? description = "Two duplexes") =>
            DevelopmentProject.InitiateNewConstruction(ParcelId, name, description, StaffUser);

        public DevelopmentProject InitiateRehabilitation(
            string name = "Roof Rehab", string? description = "Replace roofs", IReadOnlyCollection<HomeId>? homeIds = null) =>
            DevelopmentProject.InitiateRehabilitation(ParcelId, name, description, homeIds ?? [HomeId], StaffUser);
    }

    public class TheInitiateNewConstructionMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenValidInputs_WhenInitiated_ThenIsInitiatedNewConstructionWithoutHomes()
        {
            var sut = _testBed.InitiateNewConstruction();

            sut.ProjectType.Should().Be(ProjectType.NewConstruction);
            sut.ProjectStatus.Should().Be(ProjectStatus.Initiated);
            sut.HomeIds.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void GivenBlankName_WhenInitiated_ThenThrowsBusinessRuleViolation(string name)
        {
            var act = () => _testBed.InitiateNewConstruction(name: name);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("property_registry.development_project.initiate.name_required");
        }

        [Fact]
        public void GivenBlankDescription_WhenInitiated_ThenThrowsBusinessRuleViolation()
        {
            var act = () => _testBed.InitiateNewConstruction(description: " ");

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
            var sut = _testBed.InitiateRehabilitation();

            sut.ProjectType.Should().Be(ProjectType.Rehabilitation);
            sut.HomeIds.Should().Equal(_testBed.HomeId);
        }

        [Fact]
        public void GivenBlankName_WhenInitiated_ThenThrowsBusinessRuleViolation()
        {
            var act = () => _testBed.InitiateRehabilitation(name: "");

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("property_registry.development_project.initiate.name_required");
        }

        [Fact]
        public void GivenNoHomes_WhenInitiated_ThenThrowsBusinessRuleViolation()
        {
            var act = () => _testBed.InitiateRehabilitation(homeIds: []);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("property_registry.development_project.initiate_rehabilitation.home_ids_required");
        }
    }
}
