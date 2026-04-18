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
    public void ChangeStatus_DifferentStatus_UpdatesStatus()
    {
        var parcel = LandParcel.Register("APN-001", "123 Main St", 0.25m,
            new DateOnly(2024, 3, 15), AcquisitionMethod.Purchase, null, StaffUser);

        parcel.ChangeStatus(ParcelStatus.UnderDevelopment, StaffUser);

        parcel.Status.Should().Be(ParcelStatus.UnderDevelopment);
    }

    [Fact]
    public void ChangeStatus_SameStatus_IsIdempotent()
    {
        var parcel = LandParcel.Register("APN-001", "123 Main St", 0.25m,
            new DateOnly(2024, 3, 15), AcquisitionMethod.Purchase, null, StaffUser);

        parcel.ChangeStatus(ParcelStatus.Available, StaffUser);

        parcel.Status.Should().Be(ParcelStatus.Available);
        parcel.UncommittedEvents.Should().HaveCount(1);
    }
}