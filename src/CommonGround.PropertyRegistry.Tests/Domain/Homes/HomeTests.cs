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

        public HomeRegistered Registered(string? unitDesignation = "Unit B") =>
            Home.Register(HomeId, ParcelId, unitDesignation, HomeType.SingleFamily, 3, 2m, true, 1200m, 1985, InitialAssessment(), StaffUser);

        public Home AvailableHome() => EventReplay.Replay<Home>(Registered());

        public Home HomeInSelection() => AvailableHome().Evolve(new HomeSelectionOpened(HomeId.Value, "selection-1", Today))!;

        public Home HomeUnderContract() => HomeInSelection().Evolve(new HomeUnderContract(HomeId.Value, Today.AddDays(7)))!;

        public Home LeasedHome() => HomeUnderContract().Evolve(new HomeLeased(HomeId.Value, Today.AddDays(60)))!;
    }

    public class TheRegisterMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenValidInputs_WhenRegistered_ThenHomeIsAvailableWithItsInitialAssessment()
        {
            var registered = _testBed.Registered();

            var sut = EventReplay.Replay<Home>(registered);
            sut.LandParcelId.Should().Be(_testBed.ParcelId);
            sut.UnitDesignation.Should().Be("Unit B");
            sut.UnitType.Should().Be(HomeType.SingleFamily);
            sut.Status.Should().Be(HomeStatus.Available);
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

    public class TheOpenSelectionMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenAvailableHome_WhenSelectionOpened_ThenHomeIsInSelection()
        {
            var sut = _testBed.AvailableHome();

            var events = sut.OpenSelection("selection-1", _testBed.Today);

            events.Should().ContainSingle().Which.Should().BeOfType<HomeSelectionOpened>()
                .Which.OpenedDate.Should().Be(_testBed.Today);
            sut.Evolve(events)!.Status.Should().Be(HomeStatus.InSelection);
        }

        [Fact]
        public void GivenHomeUnderContract_WhenSelectionOpened_ThenReleasesTheContractAndIsInSelection()
        {
            // The new selection's news arrived before the purchase's falling through.
            var sut = _testBed.HomeUnderContract();

            var events = sut.OpenSelection("selection-2", _testBed.Today.AddDays(30));

            events.Should().SatisfyRespectively(
                e => e.Should().BeOfType<HomeContractReleased>(),
                e => e.Should().BeOfType<HomeSelectionOpened>());
            sut.Evolve(events)!.Status.Should().Be(HomeStatus.InSelection);
        }

        [Fact]
        public void GivenLeasedHome_WhenSelectionOpened_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.LeasedHome();

            var act = () => sut.OpenSelection("selection-2", _testBed.Today);

            act.Should().Throw<BusinessRuleViolationException>();
        }
    }

    public class TheCloseSelectionWithoutSaleMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenHomeInSelection_WhenClosedWithoutSale_ThenHomeIsAvailableAgain()
        {
            var sut = _testBed.HomeInSelection();

            var closed = sut.CloseSelectionWithoutSale("selection-1");

            sut.Evolve(closed)!.Status.Should().Be(HomeStatus.Available);
        }

        [Fact]
        public void GivenAvailableHome_WhenClosedWithoutSale_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.AvailableHome();

            var act = () => sut.CloseSelectionWithoutSale("selection-1");

            act.Should().Throw<BusinessRuleViolationException>();
        }
    }

    public class TheMarkUnderContractMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenHomeInSelection_WhenMarkedUnderContract_ThenHomeIsUnderContract()
        {
            var sut = _testBed.HomeInSelection();

            var underContract = sut.MarkUnderContract(_testBed.Today);

            sut.Evolve(underContract)!.Status.Should().Be(HomeStatus.UnderContract);
        }

        [Fact]
        public void GivenLeasedHome_WhenMarkedUnderContract_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.LeasedHome();

            var act = () => sut.MarkUnderContract(_testBed.Today);

            act.Should().Throw<BusinessRuleViolationException>();
        }
    }

    public class TheReleaseContractMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenHomeUnderContract_WhenContractReleased_ThenHomeIsAvailableAndCanBeOfferedAgain()
        {
            var sut = _testBed.HomeUnderContract();

            var released = sut.ReleaseContract(_testBed.Today.AddDays(30));

            sut = sut.Evolve(released)!;
            sut.Status.Should().Be(HomeStatus.Available);
            sut.OpenSelection("selection-2", _testBed.Today.AddDays(30)).Should().ContainSingle().Which.Should().BeOfType<HomeSelectionOpened>();
        }

        [Fact]
        public void GivenHomeInSelection_WhenContractReleased_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.HomeInSelection();

            var act = () => sut.ReleaseContract(_testBed.Today);

            act.Should().Throw<BusinessRuleViolationException>();
        }
    }

    public class TheMarkLeasedMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenHomeUnderContract_WhenMarkedLeased_ThenHomeIsLeased()
        {
            var sut = _testBed.HomeUnderContract();

            var leased = sut.MarkLeased(_testBed.Today.AddDays(60));

            leased.LeasedDate.Should().Be(_testBed.Today.AddDays(60));
            sut.Evolve(leased)!.Status.Should().Be(HomeStatus.Leased);
        }

        [Fact]
        public void GivenAvailableHome_WhenMarkedLeased_ThenHomeIsLeased()
        {
            var sut = _testBed.AvailableHome();

            var leased = sut.MarkLeased(_testBed.Today);

            sut.Evolve(leased)!.Status.Should().Be(HomeStatus.Leased);
        }

        [Fact]
        public void GivenLeasedHome_WhenMarkedLeased_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.LeasedHome();

            var act = () => sut.MarkLeased(_testBed.Today);

            act.Should().Throw<BusinessRuleViolationException>();
        }
    }
}
