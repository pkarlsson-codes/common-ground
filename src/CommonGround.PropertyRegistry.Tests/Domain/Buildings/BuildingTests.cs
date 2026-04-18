using CommonGround.PropertyRegistry.Domain.Buildings;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace CommonGround.PropertyRegistry.Tests.Domain.Buildings;

public class BuildingTests
{
    private static readonly UserId StaffUser = UserId.Generate();
    private static readonly LandParcelId ParcelId = LandParcelId.Generate();

    [Fact]
    public void Register_ValidInputs_CreatesBuildingWithAvailableStatus()
    {
        var building = Building.Register(
            ParcelId,
            BuildingType.SingleFamily,
            3, 2m, 1200m, 1985, 280000m,
            StaffUser);

        building.LandParcelId.Should().Be(ParcelId);
        building.Type.Should().Be(BuildingType.SingleFamily);
        building.Bedrooms.Should().Be(3);
        building.Bathrooms.Should().Be(2m);
        building.SquareFootage.Should().Be(1200m);
        building.YearBuilt.Should().Be(1985);
        building.AssessedValue.Should().Be(280000m);
        building.Status.Should().Be(BuildingStatus.Available);
    }

    [Fact]
    public void Register_NegativeSquareFootage_ThrowsBusinessRuleViolation()
    {
        var act = () => Building.Register(ParcelId, BuildingType.SingleFamily, 3, 2m, -1m, 1985, 280000m, StaffUser);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Register_NegativeAssessedValue_ThrowsBusinessRuleViolation()
    {
        var act = () => Building.Register(ParcelId, BuildingType.SingleFamily, 3, 2m, 1200m, 1985, -1m, StaffUser);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void ChangeStatus_DifferentStatus_UpdatesStatus()
    {
        var building = Building.Register(ParcelId, BuildingType.SingleFamily, 3, 2m, 1200m, 1985, 280000m, StaffUser);

        building.ChangeStatus(BuildingStatus.UnderRenovation, StaffUser);

        building.Status.Should().Be(BuildingStatus.UnderRenovation);
    }

    [Fact]
    public void RecordAssessment_ValidInputs_UpdatesAssessedValue()
    {
        var building = Building.Register(ParcelId, BuildingType.SingleFamily, 3, 2m, 1200m, 1985, 280000m, StaffUser);
        var assessmentDate = new DateOnly(2025, 1, 15);

        building.RecordAssessment(310000m, assessmentDate, "Good condition", StaffUser);

        building.AssessedValue.Should().Be(310000m);
        building.LastAssessedDate.Should().Be(assessmentDate);
        building.ConditionNotes.Should().Be("Good condition");
    }
}