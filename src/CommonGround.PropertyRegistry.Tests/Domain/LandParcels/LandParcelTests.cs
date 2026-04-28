using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace CommonGround.PropertyRegistry.Tests.Domain.LandParcels;

public class LandParcelTests
{
    private static readonly UserId StaffUser = UserId.Generate();

    [Fact]
    public void Register_ValidInputs_CreatesParcelWithAvailableStatus()
    {
        var parcel = LandParcel.Register(
            "APN-001-234-567",
            "123 Main St, Springfield",
            0.25m,
            new DateOnly(2024, 3, 15),
            AcquisitionMethod.Purchase,
            "Resale limited to 25% of appreciation",
            StaffUser);

        parcel.LegalParcelNumber.Should().Be("APN-001-234-567");
        parcel.Address.Should().Be("123 Main St, Springfield");
        parcel.AreaAcres.Should().Be(0.25m);
        parcel.Status.Should().Be(ParcelStatus.Available);
        parcel.AcquisitionMethod.Should().Be(AcquisitionMethod.Purchase);
        parcel.RegisteredBy.Should().Be(StaffUser);
    }

    [Fact]
    public void Register_EmptyLegalParcelNumber_ThrowsBusinessRuleViolation()
    {
        var act = () => LandParcel.Register(
            "",
            "123 Main St",
            0.25m,
            new DateOnly(2024, 3, 15),
            AcquisitionMethod.Purchase,
            null,
            StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Register_NegativeArea_ThrowsBusinessRuleViolation()
    {
        var act = () => LandParcel.Register(
            "APN-001",
            "123 Main St",
            -1m,
            new DateOnly(2024, 3, 15),
            AcquisitionMethod.Purchase,
            null,
            StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void StartPredevelopment_FromAvailable_UpdatesStatus()
    {
        var parcel = LandParcel.Register("APN-001", "123 Main St", 0.25m,
            new DateOnly(2024, 3, 15), AcquisitionMethod.Purchase, null, StaffUser);

        parcel.StartPredevelopment(StaffUser);

        parcel.Status.Should().Be(ParcelStatus.Predevelopment);
    }

    [Fact]
    public void StartPredevelopment_SameStatus_IsIdempotent()
    {
        var parcel = LandParcel.Register("APN-001", "123 Main St", 0.25m,
            new DateOnly(2024, 3, 15), AcquisitionMethod.Purchase, null, StaffUser);

        parcel.StartPredevelopment(StaffUser);
        parcel.StartPredevelopment(StaffUser);

        parcel.Status.Should().Be(ParcelStatus.Predevelopment);
        parcel.UncommittedEvents.Should().HaveCount(2);
    }

    [Fact]
    public void ExplicitLifecycle_HappyPath_TransitionsToTransitioning()
    {
        var parcel = LandParcel.Register("APN-001", "123 Main St", 0.25m,
            new DateOnly(2024, 3, 15), AcquisitionMethod.Purchase, null, StaffUser);

        parcel.StartPredevelopment(StaffUser);
        parcel.StartConstruction(StaffUser);
        parcel.MarkReadyForOccupancy(StaffUser);
        parcel.MarkOccupied(StaffUser);
        parcel.EnterTransitioning(StaffUser);

        parcel.Status.Should().Be(ParcelStatus.Transitioning);
    }

    [Fact]
    public void MarkReadyForOccupancy_FromTransitioning_UpdatesStatus()
    {
        var parcel = LandParcel.Register("APN-001", "123 Main St", 0.25m,
            new DateOnly(2024, 3, 15), AcquisitionMethod.Purchase, null, StaffUser);

        parcel.StartPredevelopment(StaffUser);
        parcel.StartConstruction(StaffUser);
        parcel.MarkReadyForOccupancy(StaffUser);
        parcel.MarkOccupied(StaffUser);
        parcel.EnterTransitioning(StaffUser);

        parcel.MarkReadyForOccupancy(StaffUser);

        parcel.Status.Should().Be(ParcelStatus.ReadyForOccupancy);
    }

    [Fact]
    public void StartPredevelopment_FromTransitioning_UpdatesStatus()
    {
        var parcel = LandParcel.Register("APN-001", "123 Main St", 0.25m,
            new DateOnly(2024, 3, 15), AcquisitionMethod.Purchase, null, StaffUser);

        parcel.StartPredevelopment(StaffUser);
        parcel.StartConstruction(StaffUser);
        parcel.MarkReadyForOccupancy(StaffUser);
        parcel.MarkOccupied(StaffUser);
        parcel.EnterTransitioning(StaffUser);

        parcel.StartPredevelopment(StaffUser);

        parcel.Status.Should().Be(ParcelStatus.Predevelopment);
    }

    [Fact]
    public void StartConstruction_FromAvailable_ThrowsBusinessRuleViolation()
    {
        var parcel = LandParcel.Register("APN-001", "123 Main St", 0.25m,
            new DateOnly(2024, 3, 15), AcquisitionMethod.Purchase, null, StaffUser);

        var act = () => parcel.StartConstruction(StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void ReturnToAvailable_FromPredevelopment_ThrowsBusinessRuleViolation()
    {
        var parcel = LandParcel.Register("APN-001", "123 Main St", 0.25m,
            new DateOnly(2024, 3, 15), AcquisitionMethod.Purchase, null, StaffUser);

        parcel.StartPredevelopment(StaffUser);

        var act = () => parcel.ReturnToAvailable(StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Decommission_FromOccupied_ThrowsBusinessRuleViolation()
    {
        var parcel = LandParcel.Register("APN-001", "123 Main St", 0.25m,
            new DateOnly(2024, 3, 15), AcquisitionMethod.Purchase, null, StaffUser);

        parcel.StartPredevelopment(StaffUser);
        parcel.StartConstruction(StaffUser);
        parcel.MarkReadyForOccupancy(StaffUser);
        parcel.MarkOccupied(StaffUser);

        var act = () => parcel.Decommission(StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void DecommissionAndRecommission_ResultsInAvailable()
    {
        var parcel = LandParcel.Register("APN-001", "123 Main St", 0.25m,
            new DateOnly(2024, 3, 15), AcquisitionMethod.Purchase, null, StaffUser);

        parcel.StartPredevelopment(StaffUser);
        parcel.Decommission(StaffUser);
        parcel.Recommission(StaffUser);

        parcel.Status.Should().Be(ParcelStatus.Available);
    }

    [Fact]
    public void Recommission_FromAvailable_IsIdempotent()
    {
        var parcel = LandParcel.Register("APN-001", "123 Main St", 0.25m,
            new DateOnly(2024, 3, 15), AcquisitionMethod.Purchase, null, StaffUser);

        parcel.Recommission(StaffUser);

        parcel.Status.Should().Be(ParcelStatus.Available);
        parcel.UncommittedEvents.Should().HaveCount(1);
    }
}