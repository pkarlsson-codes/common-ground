using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.Homes.Events;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.Testing;
using FluentAssertions;
using Xunit;

namespace CommonGround.PropertyRegistry.Tests.Domain.Homes;

public class HomeTests
{
    public class TestBed
    {
        public UserId StaffUser { get; } = UserId.Generate();
        public HomeId HomeId { get; } = HomeId.Generate();
        public LandParcelId ParcelId { get; } = LandParcelId.Generate();
        public DateOnly Today { get; } = new(2026, 10, 1);

        public UnitAssessment InitialAssessment() =>
            UnitAssessment.Create(280000m, new DateOnly(2024, 1, 1), null, null, StaffUser, DateTimeOffset.UtcNow, Today);

        public HomeRegistered Registered(string? unitDesignation = "Unit B") =>
            Home.Register(HomeId, ParcelId, unitDesignation, HomeType.SingleFamily, 3, 2m, true, 1200m, 1985, InitialAssessment(), StaffUser);

        public Home AvailableHome() => EventReplay.Replay<Home>(Registered());
    }

    public class TheRegisterMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenValidInputs_WhenRegistered_ThenHomeHasItsFactsAndInitialAssessment()
        {
            var registered = _testBed.Registered();

            var sut = EventReplay.Replay<Home>(registered);
            sut.LandParcelId.Should().Be(_testBed.ParcelId);
            sut.UnitDesignation.Should().Be("Unit B");
            sut.UnitType.Should().Be(HomeType.SingleFamily);
            sut.RegisteredBy.Should().Be(_testBed.StaffUser);
            sut.Assessments.Should().ContainSingle().Which.AssessedValue.Should().Be(280000m);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public void GivenNoUnitDesignation_WhenRegistered_ThenHomeHasNone(string? unitDesignation)
        {
            var registered = _testBed.Registered(unitDesignation);

            EventReplay.Replay<Home>(registered).UnitDesignation.Should().BeNull();
        }
    }
}
