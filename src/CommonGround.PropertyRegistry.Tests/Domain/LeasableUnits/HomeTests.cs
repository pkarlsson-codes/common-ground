using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;
using Xunit;

namespace CommonGround.PropertyRegistry.Tests.Domain.Homes;

public class HomeTests
{
    private static readonly UserId StaffUser = UserId.Generate();
    private static readonly LandParcelId ParcelId = LandParcelId.Generate();

    private static readonly UnitAssessment _initialAssessment =
        UnitAssessment.Create(280000m, new DateOnly(2024, 1, 1), null, null, StaffUser, DateTimeOffset.UtcNow, new DateOnly(2026, 1, 1));

    private static Home RegisterHome() =>
        Home.Register(
            ParcelId,
            HomeType.SingleFamily,
            3, 2m, isAccessible: true, 1200m, 1985,
            _initialAssessment,
            StaffUser);

    [Fact]
    public void Register_ValidInputs_CreatesHomeWithAvailableStatus()
    {
        var unit = RegisterHome();

        unit.LandParcelId.Should().Be(ParcelId);
        unit.UnitType.Should().Be(HomeType.SingleFamily);
        unit.Bedrooms.Should().Be(3);
        unit.Bathrooms.Should().Be(2m);
        unit.IsAccessible.Should().BeTrue();
        unit.SquareFootage.Should().Be(1200m);
        unit.YearBuilt.Should().Be(1985);
        unit.Status.Should().Be(HomeStatus.Available);
        unit.RegisteredBy.Should().Be(StaffUser);
        unit.Assessments.Should().HaveCount(1);
        unit.Assessments[0].AssessedValue.Should().Be(280000m);
    }

}
