using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.Homes.Events;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
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

        public HomeRegistered Registered(string? unitDesignation = "Unit B", int? incomeLimitPercentOfAmi = 80) =>
            Home.Register(HomeId, ParcelId, unitDesignation, HomeType.SingleFamily, 3, 2m, true, incomeLimitPercentOfAmi, 1200m, 1985, InitialAssessment(), StaffUser);

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

        [Theory]
        [InlineData(80)]
        [InlineData(null)]
        public void GivenAnIncomeLimitOrNone_WhenRegistered_ThenHomeHasIt(int? percentOfAmi)
        {
            var registered = _testBed.Registered(incomeLimitPercentOfAmi: percentOfAmi);

            EventReplay.Replay<Home>(registered).IncomeLimitPercentOfAmi.Should().Be(percentOfAmi);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(201)]
        public void GivenAnIncomeLimitOutOfRange_WhenRegistered_ThenThrows(int percentOfAmi)
        {
            var act = () => _testBed.Registered(incomeLimitPercentOfAmi: percentOfAmi);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("property_registry.home.income_limit.percent_out_of_range");
        }
    }

    public class TheSetIncomeLimitMethod
    {
        private readonly TestBed _testBed = new();

        [Theory]
        [InlineData(1)]
        [InlineData(200)]
        [InlineData(null)]
        public void GivenALimitInRangeOrNone_WhenSet_ThenHomeHasIt(int? percentOfAmi)
        {
            var sut = _testBed.AvailableHome();

            var set = sut.SetIncomeLimit(percentOfAmi, _testBed.StaffUser);

            EventReplay.Replay<Home>(_testBed.Registered(), set).IncomeLimitPercentOfAmi.Should().Be(percentOfAmi);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(201)]
        public void GivenALimitOutOfRange_WhenSet_ThenThrows(int percentOfAmi)
        {
            var sut = _testBed.AvailableHome();

            var act = () => sut.SetIncomeLimit(percentOfAmi, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("property_registry.home.income_limit.percent_out_of_range");
        }
    }
}
