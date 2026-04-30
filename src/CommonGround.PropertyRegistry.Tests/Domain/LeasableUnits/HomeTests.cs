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

    private static Home RegisterHome() =>
        Home.Register(
            ParcelId,
            HomeType.SingleFamily,
            3, 2m, 1200m, 1985, 280000m,
            StaffUser);

    [Fact]
    public void Register_ValidInputs_CreatesHomeWithAvailableStatus()
    {
        var unit = RegisterHome();

        unit.LandParcelId.Should().Be(ParcelId);
        unit.UnitType.Should().Be(HomeType.SingleFamily);
        unit.Bedrooms.Should().Be(3);
        unit.Bathrooms.Should().Be(2m);
        unit.SquareFootage.Should().Be(1200m);
        unit.YearBuilt.Should().Be(1985);
        unit.Status.Should().Be(HomeStatus.Available);
        unit.RegisteredBy.Should().Be(StaffUser);
    }

}
