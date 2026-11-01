using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.PropertyRegistry.Domain.LandParcels.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using FluentAssertions;
using Xunit;

namespace CommonGround.PropertyRegistry.Tests.Domain.LandParcels;

public class LandParcelTests
{
    public class TestBed
    {
        public UserId StaffUser { get; } = UserId.Generate();

        public LandParcelId ParcelId { get; } = LandParcelId.Generate();

        public LandParcelRegistered Registered() =>
            LandParcel.Register(
                ParcelId,
                "APN-001-234-567",
                "123 Main St, Springfield",
                0.25m,
                new DateOnly(2024, 3, 15),
                AcquisitionMethod.Purchase,
                "Resale limited to 25% of appreciation",
                StaffUser);

        public LandParcel ActiveParcel() => EventReplay.Replay<LandParcel>(Registered());

        public LandParcel DecommissionedParcel() =>
            EventReplay.Replay<LandParcel>(Registered(), new ParcelDecommissioned(ParcelId.Value, StaffUser.Value));
    }

    public class TheRegisterMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenValidInputs_WhenRegistered_ThenStartsAnActiveParcel()
        {
            var registered = _testBed.Registered();

            EventReplay.Replay<LandParcel>(registered).Should().BeEquivalentTo(new
            {
                Id = _testBed.ParcelId,
                LegalParcelNumber = "APN-001-234-567",
                AcquisitionMethod = AcquisitionMethod.Purchase,
                IsDecommissioned = false,
            });
        }

        [Theory]
        [InlineData("", "123 Main St", 0.25)]
        [InlineData("APN-001", "", 0.25)]
        [InlineData("APN-001", "123 Main St", -1)]
        public void GivenInvalidInputs_WhenRegistered_ThenThrowsBusinessRuleViolation(string parcelNumber, string address, decimal areaAcres)
        {
            var act = () => LandParcel.Register(
                _testBed.ParcelId, parcelNumber, address, areaAcres, new DateOnly(2024, 3, 15), AcquisitionMethod.Purchase, null, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>();
        }
    }

    public class TheListForPlacementMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenDecommissionedParcel_WhenListed_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.DecommissionedParcel();

            var act = () => sut.ListForPlacement(_testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>();
        }
    }

    public class TheWithdrawForRedevelopmentMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenDecommissionedParcel_WhenWithdrawn_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.DecommissionedParcel();

            var act = () => sut.WithdrawForRedevelopment(_testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>();
        }
    }

    public class TheDecommissionMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenActiveParcel_WhenDecommissioned_ThenIsDecommissioned()
        {
            var sut = _testBed.ActiveParcel();

            var decommissioned = sut.Decommission(_testBed.StaffUser);

            sut.Evolve(decommissioned!)!.IsDecommissioned.Should().BeTrue();
        }

        [Fact]
        public void GivenDecommissionedParcel_WhenDecommissioned_ThenDecidesNothing()
        {
            var sut = _testBed.DecommissionedParcel();

            var decommissioned = sut.Decommission(_testBed.StaffUser);

            decommissioned.Should().BeNull();
        }
    }

    public class TheRecommissionMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenDecommissionedParcel_WhenRecommissioned_ThenIsActive()
        {
            var sut = _testBed.DecommissionedParcel();

            var recommissioned = sut.Recommission(_testBed.StaffUser);

            sut.Evolve(recommissioned!)!.IsDecommissioned.Should().BeFalse();
        }

        [Fact]
        public void GivenActiveParcel_WhenRecommissioned_ThenDecidesNothing()
        {
            var sut = _testBed.ActiveParcel();

            var recommissioned = sut.Recommission(_testBed.StaffUser);

            recommissioned.Should().BeNull();
        }
    }
}
