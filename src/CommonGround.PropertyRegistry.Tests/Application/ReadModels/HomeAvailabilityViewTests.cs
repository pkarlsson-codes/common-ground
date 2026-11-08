using CommonGround.PropertyRegistry.Application.ReadModels;
using FluentAssertions;
using Xunit;

namespace CommonGround.PropertyRegistry.Tests.Application.ReadModels;

public class HomeAvailabilityViewTests
{
    public class TestBed
    {
        public DateTimeOffset FirstStartedAt { get; } = new(2026, 10, 1, 15, 0, 0, TimeSpan.Zero);

        public DateOnly Today { get; } = new(2026, 10, 1);

        public HomeAvailabilityView Sut { get; } = new() { Id = "home-1" };

        public HomeAvailabilityView InSelection()
        {
            Sut.RecordSelectionStarted("selection-1", FirstStartedAt, Today);
            return Sut;
        }

        public HomeAvailabilityView UnderContract()
        {
            InSelection().RecordApplicationSelected("selection-1", "application-1", Today.AddDays(7));
            return Sut;
        }
    }

    public class TheConstructor
    {
        [Fact]
        public void GivenNoNews_WhenCreated_ThenHomeIsAvailable()
        {
            var sut = new HomeAvailabilityView();

            sut.Status.Should().Be(HomeStatus.Available.DisplayName);
        }
    }

    public class TheOccupancyAndSaleStage
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenNoNews_WhenCreated_ThenVacantAndOffered()
        {
            var sut = new HomeAvailabilityView();

            sut.Occupancy.Should().Be(HomeOccupancy.Vacant.DisplayName);
            sut.SaleStage.Should().Be(HomeSaleStage.Offered.DisplayName);
        }

        [Fact]
        public void GivenSelectionStarted_WhenRefreshed_ThenVacantAndInSelection()
        {
            var sut = _testBed.InSelection();

            sut.Occupancy.Should().Be(HomeOccupancy.Vacant.DisplayName);
            sut.SaleStage.Should().Be(HomeSaleStage.InSelection.DisplayName);
        }

        [Fact]
        public void GivenApplicationSelected_WhenRefreshed_ThenVacantAndUnderContract()
        {
            var sut = _testBed.UnderContract();

            sut.Occupancy.Should().Be(HomeOccupancy.Vacant.DisplayName);
            sut.SaleStage.Should().Be(HomeSaleStage.UnderContract.DisplayName);
        }

        [Fact]
        public void GivenApplicationSelectedBeforeItsSelectionStarted_WhenStartArrives_ThenVacantAndUnderContract()
        {
            _testBed.Sut.RecordApplicationSelected("selection-1", "application-1", _testBed.Today.AddDays(7));

            _testBed.Sut.RecordSelectionStarted("selection-1", _testBed.FirstStartedAt, _testBed.Today);

            _testBed.Sut.Occupancy.Should().Be(HomeOccupancy.Vacant.DisplayName);
            _testBed.Sut.SaleStage.Should().Be(HomeSaleStage.UnderContract.DisplayName);
        }

        [Fact]
        public void GivenPurchaseFellThrough_WhenRefreshed_ThenVacantAndOffered()
        {
            var sut = _testBed.UnderContract();

            sut.RecordPurchaseFellThrough("application-1", _testBed.Today.AddDays(20));

            sut.Occupancy.Should().Be(HomeOccupancy.Vacant.DisplayName);
            sut.SaleStage.Should().Be(HomeSaleStage.Offered.DisplayName);
        }

        [Fact]
        public void GivenHomeUnderContract_WhenLeased_ThenOccupiedAndNotForSale()
        {
            var sut = _testBed.UnderContract();

            sut.RecordLeased(_testBed.Today.AddDays(60));

            sut.Occupancy.Should().Be(HomeOccupancy.Occupied.DisplayName);
            sut.SaleStage.Should().Be(HomeSaleStage.NotForSale.DisplayName);
        }

        [Fact]
        public void GivenLeasedHome_WhenResaleOpened_ThenOccupiedAndAwaitingTrustDecision()
        {
            _testBed.Sut.RecordLeased(_testBed.Today.AddYears(-3));

            _testBed.Sut.RecordResaleOpened("resale-1", _testBed.Today);

            _testBed.Sut.Occupancy.Should().Be(HomeOccupancy.Occupied.DisplayName);
            _testBed.Sut.SaleStage.Should().Be(HomeSaleStage.AwaitingTrustDecision.DisplayName);
        }

        [Fact]
        public void GivenResaleOpenedBeforeNewsOfTheLease_WhenLeased_ThenOccupiedAndAwaitingTrustDecision()
        {
            _testBed.Sut.RecordResaleOpened("resale-1", _testBed.Today);

            _testBed.Sut.RecordLeased(_testBed.Today.AddYears(-3));

            _testBed.Sut.Occupancy.Should().Be(HomeOccupancy.Occupied.DisplayName);
            _testBed.Sut.SaleStage.Should().Be(HomeSaleStage.AwaitingTrustDecision.DisplayName);
        }

        [Fact]
        public void GivenResaleWithdrawn_WhenRefreshed_ThenOccupiedAndNotForSale()
        {
            _testBed.Sut.RecordLeased(_testBed.Today.AddYears(-3));
            _testBed.Sut.RecordResaleOpened("resale-1", _testBed.Today);

            _testBed.Sut.RecordResaleWithdrawn("resale-1");

            _testBed.Sut.Occupancy.Should().Be(HomeOccupancy.Occupied.DisplayName);
            _testBed.Sut.SaleStage.Should().Be(HomeSaleStage.NotForSale.DisplayName);
        }
    }

    public class TheRecordSelectionStartedMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenAvailableHome_WhenSelectionStarted_ThenHomeIsInSelectionFromThatDate()
        {
            _testBed.Sut.RecordSelectionStarted("selection-1", _testBed.FirstStartedAt, _testBed.Today);

            _testBed.Sut.Status.Should().Be(HomeStatus.InSelection.DisplayName);
            _testBed.Sut.SelectionOpenedDate.Should().Be(_testBed.Today);
        }

        [Fact]
        public void GivenHomeUnderContract_WhenNextSelectionStartsBeforeNewsOfTheFallThrough_ThenHomeIsInSelection()
        {
            var sut = _testBed.UnderContract();

            sut.RecordSelectionStarted("selection-2", _testBed.FirstStartedAt.AddDays(30), _testBed.Today.AddDays(30));

            sut.Status.Should().Be(HomeStatus.InSelection.DisplayName);
            sut.SelectionOpenedDate.Should().Be(_testBed.Today.AddDays(30));
            sut.UnderContractDate.Should().BeNull();
        }

        [Fact]
        public void GivenApplicationSelectedFirst_WhenItsSelectionsStartArrives_ThenHomeIsUnderContract()
        {
            _testBed.Sut.RecordApplicationSelected("selection-1", "application-1", _testBed.Today.AddDays(7));

            _testBed.Sut.RecordSelectionStarted("selection-1", _testBed.FirstStartedAt, _testBed.Today);

            _testBed.Sut.Status.Should().Be(HomeStatus.UnderContract.DisplayName);
            _testBed.Sut.SelectionOpenedDate.Should().Be(_testBed.Today);
            _testBed.Sut.UnderContractDate.Should().Be(_testBed.Today.AddDays(7));
        }

        [Fact]
        public void GivenSelectionCancelled_WhenItsStartArrivesLate_ThenHomeIsAvailable()
        {
            _testBed.Sut.RecordSelectionCancelled("selection-1");

            _testBed.Sut.RecordSelectionStarted("selection-1", _testBed.FirstStartedAt, _testBed.Today);

            _testBed.Sut.Status.Should().Be(HomeStatus.Available.DisplayName);
        }

        [Fact]
        public void GivenLaterSelectionStarted_WhenAnEarlierSelectionsStartArrives_ThenLaterSelectionCounts()
        {
            _testBed.Sut.RecordSelectionStarted("selection-2", _testBed.FirstStartedAt.AddDays(30), _testBed.Today.AddDays(30));

            _testBed.Sut.RecordSelectionStarted("selection-1", _testBed.FirstStartedAt, _testBed.Today);

            _testBed.Sut.SelectionOpenedDate.Should().Be(_testBed.Today.AddDays(30));
        }
    }

    public class TheRecordSelectionCancelledMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenHomeInSelection_WhenSelectionCancelled_ThenHomeIsAvailable()
        {
            var sut = _testBed.InSelection();

            sut.RecordSelectionCancelled("selection-1");

            sut.Status.Should().Be(HomeStatus.Available.DisplayName);
            sut.SelectionOpenedDate.Should().BeNull();
        }

        [Fact]
        public void GivenNextSelectionStarted_WhenEarlierSelectionsCancellationArrives_ThenHomeStaysInSelection()
        {
            var sut = _testBed.InSelection();
            sut.RecordSelectionStarted("selection-2", _testBed.FirstStartedAt.AddDays(1), _testBed.Today.AddDays(1));

            sut.RecordSelectionCancelled("selection-1");

            sut.Status.Should().Be(HomeStatus.InSelection.DisplayName);
        }
    }

    public class TheRecordApplicationSelectedMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenHomeInSelection_WhenApplicationSelected_ThenHomeIsUnderContractFromThatDate()
        {
            var sut = _testBed.InSelection();

            sut.RecordApplicationSelected("selection-1", "application-1", _testBed.Today.AddDays(7));

            sut.Status.Should().Be(HomeStatus.UnderContract.DisplayName);
            sut.UnderContractDate.Should().Be(_testBed.Today.AddDays(7));
        }

        [Fact]
        public void GivenPurchaseAlreadyFellThrough_WhenApplicationSelectedArrivesLate_ThenHomeIsAvailableAgain()
        {
            var sut = _testBed.InSelection();
            sut.RecordPurchaseFellThrough("application-1", _testBed.Today.AddDays(30));

            sut.RecordApplicationSelected("selection-1", "application-1", _testBed.Today.AddDays(7));

            sut.Status.Should().Be(HomeStatus.Available.DisplayName);
            sut.AvailableAgainDate.Should().Be(_testBed.Today.AddDays(30));
            sut.FellThroughBeforeSelected.Should().BeEmpty();
        }
    }

    public class TheRecordPurchaseFellThroughMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenHomeUnderContract_WhenPurchaseFellThrough_ThenHomeIsAvailableAgainFromThatDate()
        {
            var sut = _testBed.UnderContract();

            sut.RecordPurchaseFellThrough("application-1", _testBed.Today.AddDays(30));

            sut.Status.Should().Be(HomeStatus.Available.DisplayName);
            sut.AvailableAgainDate.Should().Be(_testBed.Today.AddDays(30));
            sut.SelectionOpenedDate.Should().BeNull();
            sut.UnderContractDate.Should().BeNull();
        }

        [Fact]
        public void GivenNextSelectionStarted_WhenEarlierPurchaseFellThroughArrives_ThenHomeStaysInSelection()
        {
            var sut = _testBed.UnderContract();
            sut.RecordSelectionStarted("selection-2", _testBed.FirstStartedAt.AddDays(30), _testBed.Today.AddDays(30));

            sut.RecordPurchaseFellThrough("application-1", _testBed.Today.AddDays(30));

            sut.Status.Should().Be(HomeStatus.InSelection.DisplayName);
            sut.AvailableAgainDate.Should().Be(_testBed.Today.AddDays(30));
        }

        [Fact]
        public void GivenHomeInSelection_WhenDirectPurchaseFellThrough_ThenNothingChanges()
        {
            var sut = _testBed.InSelection();

            sut.RecordPurchaseFellThrough(applicationId: null, _testBed.Today);

            sut.Status.Should().Be(HomeStatus.InSelection.DisplayName);
            sut.AvailableAgainDate.Should().BeNull();
        }
    }

    public class TheRecordLeasedMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenHomeUnderContract_WhenLeased_ThenHomeIsLeasedFromTheLeaseStart()
        {
            var sut = _testBed.UnderContract();

            sut.RecordLeased(_testBed.Today.AddDays(60));

            sut.Status.Should().Be(HomeStatus.Leased.DisplayName);
            sut.LeasedDate.Should().Be(_testBed.Today.AddDays(60));
        }

        [Fact]
        public void GivenAvailableHome_WhenLeasedDirectly_ThenHomeIsLeasedFromTheLeaseStart()
        {
            var sut = _testBed.Sut;

            sut.RecordLeased(_testBed.Today.AddYears(-6));

            sut.Status.Should().Be(HomeStatus.Leased.DisplayName);
            sut.LeasedDate.Should().Be(_testBed.Today.AddYears(-6));
        }

        [Fact]
        public void GivenLeasedHome_WhenALateSelectionStarts_ThenHomeStaysLeased()
        {
            var sut = _testBed.UnderContract();
            sut.RecordLeased(_testBed.Today.AddDays(60));

            sut.RecordSelectionStarted("selection-2", _testBed.FirstStartedAt.AddDays(90), _testBed.Today.AddDays(90));

            sut.Status.Should().Be(HomeStatus.Leased.DisplayName);
        }
    }

    public class TheRecordResaleOpenedMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenLeasedHome_WhenResaleOpened_ThenHomeIsInResaleFromTheNoticeDate()
        {
            var sut = _testBed.Sut;
            sut.RecordLeased(_testBed.Today.AddYears(-3));

            sut.RecordResaleOpened("resale-1", _testBed.Today);

            sut.Status.Should().Be(HomeStatus.Resale.DisplayName);
            sut.ResaleNoticeDate.Should().Be(_testBed.Today);
        }

        [Fact]
        public void GivenResaleOpenedBeforeNewsOfTheLease_WhenLeased_ThenHomeStaysInResale()
        {
            var sut = _testBed.Sut;
            sut.RecordResaleOpened("resale-1", _testBed.Today);

            sut.RecordLeased(_testBed.Today.AddYears(-3));

            sut.Status.Should().Be(HomeStatus.Resale.DisplayName);
        }
    }

    public class TheRecordResaleWithdrawnMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenHomeInResale_WhenResaleWithdrawn_ThenHomeIsLeasedAgain()
        {
            var sut = _testBed.Sut;
            sut.RecordLeased(_testBed.Today.AddYears(-3));
            sut.RecordResaleOpened("resale-1", _testBed.Today);

            sut.RecordResaleWithdrawn("resale-1");

            sut.Status.Should().Be(HomeStatus.Leased.DisplayName);
            sut.ResaleNoticeDate.Should().BeNull();
        }

        [Fact]
        public void GivenWithdrawalArrivesFirst_WhenItsOpeningArrives_ThenHomeStaysLeased()
        {
            var sut = _testBed.Sut;
            sut.RecordLeased(_testBed.Today.AddYears(-3));
            sut.RecordResaleWithdrawn("resale-1");

            sut.RecordResaleOpened("resale-1", _testBed.Today);

            sut.Status.Should().Be(HomeStatus.Leased.DisplayName);
        }

        [Fact]
        public void GivenEarlierResaleWithdrawn_WhenAnotherOpens_ThenHomeIsInResaleFromTheNewNotice()
        {
            var sut = _testBed.Sut;
            sut.RecordLeased(_testBed.Today.AddYears(-3));
            sut.RecordResaleOpened("resale-1", _testBed.Today.AddDays(-40));
            sut.RecordResaleWithdrawn("resale-1");

            sut.RecordResaleOpened("resale-2", _testBed.Today);

            sut.ResaleNoticeDate.Should().Be(_testBed.Today);
        }
    }
}
