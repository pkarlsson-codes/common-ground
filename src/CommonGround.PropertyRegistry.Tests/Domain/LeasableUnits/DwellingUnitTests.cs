using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.PropertyRegistry.Domain.LeasableUnits;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace CommonGround.PropertyRegistry.Tests.Domain.LeasableUnits;

public class DwellingUnitTests
{
    private static readonly UserId StaffUser = UserId.Generate();
    private static readonly LandParcelId ParcelId = LandParcelId.Generate();

    private static DwellingUnit RegisterUnit() =>
        DwellingUnit.Register(
            ParcelId,
            DwellingUnitType.SingleFamily,
            3, 2m, 1200m, 1985, 280000m,
            StaffUser);

    [Fact]
    public void Register_ValidInputs_CreatesDwellingUnitWithAvailableStatus()
    {
        var unit = RegisterUnit();

        unit.LandParcelId.Should().Be(ParcelId);
        unit.UnitType.Should().Be(DwellingUnitType.SingleFamily);
        unit.Bedrooms.Should().Be(3);
        unit.Bathrooms.Should().Be(2m);
        unit.SquareFootage.Should().Be(1200m);
        unit.YearBuilt.Should().Be(1985);
        unit.AssessedValue.Should().Be(280000m);
        unit.Status.Should().Be(UnitStatus.Available);
        unit.RegisteredBy.Should().Be(StaffUser);
    }

    [Fact]
    public void ExecuteGroundLease_FromAvailable_SetsPendingOccupancy()
    {
        var unit = RegisterUnit();

        unit.ExecuteGroundLease(StaffUser);

        unit.Status.Should().Be(UnitStatus.PendingOccupancy);
    }

    [Fact]
    public void ConfirmMoveIn_FromPendingOccupancy_SetsOccupied()
    {
        var unit = RegisterUnit();
        unit.ExecuteGroundLease(StaffUser);

        unit.ConfirmMoveIn(StaffUser);

        unit.Status.Should().Be(UnitStatus.Occupied);
    }

    [Fact]
    public void InitiateResale_FromOccupied_SetsResale()
    {
        var unit = RegisterUnit();
        unit.ExecuteGroundLease(StaffUser);
        unit.ConfirmMoveIn(StaffUser);

        unit.InitiateResale(StaffUser);

        unit.Status.Should().Be(UnitStatus.Resale);
    }

    [Fact]
    public void CompleteResaleClosing_FromResale_SetsPendingOccupancy()
    {
        var unit = RegisterUnit();
        unit.ExecuteGroundLease(StaffUser);
        unit.ConfirmMoveIn(StaffUser);
        unit.InitiateResale(StaffUser);

        unit.CompleteResaleClosing(StaffUser);

        unit.Status.Should().Be(UnitStatus.PendingOccupancy);
    }

    [Fact]
    public void CancelOccupancy_FromPendingOccupancy_SetsAvailable()
    {
        var unit = RegisterUnit();
        unit.ExecuteGroundLease(StaffUser);

        unit.CancelOccupancy(StaffUser);

        unit.Status.Should().Be(UnitStatus.Available);
    }

    [Fact]
    public void ReopenResaleListing_FromResale_SetsAvailable()
    {
        var unit = RegisterUnit();
        unit.ExecuteGroundLease(StaffUser);
        unit.ConfirmMoveIn(StaffUser);
        unit.InitiateResale(StaffUser);

        unit.ReopenResaleListing(StaffUser);

        unit.Status.Should().Be(UnitStatus.Available);
    }

    [Fact]
    public void AbandonResaleForRedevelopment_FromResale_SetsAvailable()
    {
        var unit = RegisterUnit();
        unit.ExecuteGroundLease(StaffUser);
        unit.ConfirmMoveIn(StaffUser);
        unit.InitiateResale(StaffUser);

        unit.AbandonResaleForRedevelopment(StaffUser);

        unit.Status.Should().Be(UnitStatus.Available);
    }

    [Fact]
    public void AbandonResaleForRedevelopment_FromPendingOccupancy_SetsAvailable()
    {
        var unit = RegisterUnit();
        unit.ExecuteGroundLease(StaffUser);

        unit.AbandonResaleForRedevelopment(StaffUser);

        unit.Status.Should().Be(UnitStatus.Available);
    }

    [Fact]
    public void ExecuteGroundLease_FromOccupied_ThrowsBusinessRuleViolation()
    {
        var unit = RegisterUnit();
        unit.ExecuteGroundLease(StaffUser);
        unit.ConfirmMoveIn(StaffUser);

        var act = () => unit.ExecuteGroundLease(StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void FullLifecycle_HappyPath()
    {
        // Available → PendingOccupancy → Occupied → Resale → PendingOccupancy → Occupied
        var unit = RegisterUnit();

        unit.Status.Should().Be(UnitStatus.Available);

        unit.ExecuteGroundLease(StaffUser);
        unit.Status.Should().Be(UnitStatus.PendingOccupancy);

        unit.ConfirmMoveIn(StaffUser);
        unit.Status.Should().Be(UnitStatus.Occupied);

        unit.InitiateResale(StaffUser);
        unit.Status.Should().Be(UnitStatus.Resale);

        unit.CompleteResaleClosing(StaffUser);
        unit.Status.Should().Be(UnitStatus.PendingOccupancy);

        unit.ConfirmMoveIn(StaffUser);
        unit.Status.Should().Be(UnitStatus.Occupied);
    }
}
