using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace CommonGround.PropertyRegistry.Tests.Domain.LandParcels;

public class LandParcelTests
{
    private static readonly UserId StaffUser = UserId.Generate();

    private static LandParcel RegisterParcel() =>
        LandParcel.Register(
            "APN-001-234-567",
            "123 Main St, Springfield",
            0.25m,
            new DateOnly(2024, 3, 15),
            AcquisitionMethod.Purchase,
            "Resale limited to 25% of appreciation",
            StaffUser);

    [Fact]
    public void Register_ValidInputs_CreatesParcel()
    {
        var parcel = RegisterParcel();

        parcel.LegalParcelNumber.Should().Be("APN-001-234-567");
        parcel.Address.Should().Be("123 Main St, Springfield");
        parcel.AreaAcres.Should().Be(0.25m);
        parcel.IsDecommissioned.Should().BeFalse();
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
    public void ListForPlacement_WhenActive_Succeeds()
    {
        var parcel = RegisterParcel();

        var act = () => parcel.ListForPlacement(StaffUser);

        act.Should().NotThrow();
    }

    [Fact]
    public void ListForPlacement_WhenDecommissioned_ThrowsBusinessRuleViolation()
    {
        var parcel = RegisterParcel();
        parcel.Decommission(StaffUser);

        var act = () => parcel.ListForPlacement(StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void WithdrawForRedevelopment_WhenActive_Succeeds()
    {
        var parcel = RegisterParcel();

        var act = () => parcel.WithdrawForRedevelopment(StaffUser);

        act.Should().NotThrow();
    }

    [Fact]
    public void WithdrawForRedevelopment_WhenDecommissioned_ThrowsBusinessRuleViolation()
    {
        var parcel = RegisterParcel();
        parcel.Decommission(StaffUser);

        var act = () => parcel.WithdrawForRedevelopment(StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Decommission_WhenActive_SetsIsDecommissioned()
    {
        var parcel = RegisterParcel();

        parcel.Decommission(StaffUser);

        parcel.IsDecommissioned.Should().BeTrue();
    }

    [Fact]
    public void Decommission_WhenAlreadyDecommissioned_IsIdempotent()
    {
        var parcel = RegisterParcel();
        parcel.Decommission(StaffUser);

        parcel.Decommission(StaffUser);

        parcel.IsDecommissioned.Should().BeTrue();
        parcel.UncommittedEvents.Should().HaveCount(2); // Register + one Decommission
    }

    [Fact]
    public void Recommission_WhenDecommissioned_ClearsIsDecommissioned()
    {
        var parcel = RegisterParcel();
        parcel.Decommission(StaffUser);

        parcel.Recommission(StaffUser);

        parcel.IsDecommissioned.Should().BeFalse();
    }

    [Fact]
    public void Recommission_WhenNotDecommissioned_IsIdempotent()
    {
        var parcel = RegisterParcel();

        parcel.Recommission(StaffUser);

        parcel.IsDecommissioned.Should().BeFalse();
        parcel.UncommittedEvents.Should().HaveCount(1); // only Register event
    }
}
