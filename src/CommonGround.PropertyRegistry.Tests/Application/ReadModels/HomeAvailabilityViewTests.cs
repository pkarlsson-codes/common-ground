using CommonGround.PropertyRegistry.Application.ReadModels;
using FluentAssertions;
using Xunit;

namespace CommonGround.PropertyRegistry.Tests.Application.ReadModels;

public class HomeAvailabilityViewTests
{
    public class TestBed
    {
        public DateTimeOffset FirstChosenAt { get; } = new(2026, 10, 8, 15, 0, 0, TimeSpan.Zero);

        public DateOnly Today { get; } = new(2026, 10, 1);

        public HomeAvailabilityView Sut { get; } = new() { Id = "home-1" };

        public HomeAvailabilityView UnderContract()
        {
            Sut.RecordApplicationSelected("selection-1", "application-1", Today.AddDays(7), FirstChosenAt);
            return Sut;
        }
    }

    public class TheConstructor
    {
        [Fact]
        public void GivenNoNews_WhenCreated_ThenHomeIsAvailable()
        {
            var sut = new HomeAvailabilityView();

            sut.SaleStage.Should().Be(HomeSaleStage.Offered.DisplayName);
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
        public void GivenApplicationSelected_WhenRefreshed_ThenVacantAndUnderContract()
        {
            var sut = _testBed.UnderContract();

            sut.Occupancy.Should().Be(HomeOccupancy.Vacant.DisplayName);
            sut.SaleStage.Should().Be(HomeSaleStage.UnderContract.DisplayName);
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

    public class TheRecordApplicationSelectedMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenAvailableHome_WhenApplicationSelected_ThenHomeIsUnderContractFromTheDateOfTheChoice()
        {
            var sut = _testBed.Sut;

            sut.RecordApplicationSelected("selection-1", "application-1", _testBed.Today.AddDays(7), _testBed.FirstChosenAt);

            sut.SaleStage.Should().Be(HomeSaleStage.UnderContract.DisplayName);
            sut.UnderContractDate.Should().Be(_testBed.Today.AddDays(7));
            sut.SaleStageSince.Should().Be(_testBed.Today.AddDays(7));
            sut.LatestSelectionId.Should().Be("selection-1");
        }

        [Fact]
        public void GivenPurchaseAlreadyFellThrough_WhenApplicationSelectedArrivesLate_ThenHomeIsAvailableAgainAccountingForTheChoice()
        {
            var sut = _testBed.Sut;
            sut.RecordPurchaseFellThrough("application-1", _testBed.Today.AddDays(30));

            sut.RecordApplicationSelected("selection-1", "application-1", _testBed.Today.AddDays(7), _testBed.FirstChosenAt);

            sut.SaleStage.Should().Be(HomeSaleStage.Offered.DisplayName);
            sut.AvailableAgainDate.Should().Be(_testBed.Today.AddDays(30));
            sut.FellThroughBeforeSelected.Should().BeEmpty();
            sut.LatestSelectionId.Should().Be("selection-1");
        }

        [Fact]
        public void GivenLaterChoiceHeardOfFirst_WhenTheEarlierChoiceArrives_ThenTheLaterChoiceCounts()
        {
            var sut = _testBed.Sut;
            sut.RecordApplicationSelected("selection-2", "application-2", _testBed.Today.AddDays(40), _testBed.FirstChosenAt.AddDays(33));

            sut.RecordApplicationSelected("selection-1", "application-1", _testBed.Today.AddDays(7), _testBed.FirstChosenAt);

            sut.UnderContractDate.Should().Be(_testBed.Today.AddDays(40));
            sut.LatestSelectionId.Should().Be("selection-2");
        }

        [Fact]
        public void GivenChoiceBackdatedBeforeAnEarlierOne_WhenRecorded_ThenTheChoiceRecordedLastCounts()
        {
            var sut = _testBed.UnderContract();
            sut.RecordPurchaseFellThrough("application-1", _testBed.Today.AddDays(30));

            sut.RecordApplicationSelected("selection-2", "application-2", _testBed.Today.AddDays(30), _testBed.FirstChosenAt.AddDays(25));

            sut.SaleStage.Should().Be(HomeSaleStage.UnderContract.DisplayName);
            sut.LatestSelectionId.Should().Be("selection-2");
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

            sut.SaleStage.Should().Be(HomeSaleStage.Offered.DisplayName);
            sut.AvailableAgainDate.Should().Be(_testBed.Today.AddDays(30));
            sut.SaleStageSince.Should().Be(_testBed.Today.AddDays(30));
            sut.UnderContractDate.Should().BeNull();
        }

        [Fact]
        public void GivenNextChoiceHeardOfFirst_WhenTheEarlierPurchaseFellThroughArrives_ThenHomeStaysUnderContract()
        {
            var sut = _testBed.UnderContract();
            sut.RecordApplicationSelected("selection-2", "application-2", _testBed.Today.AddDays(35), _testBed.FirstChosenAt.AddDays(28));

            sut.RecordPurchaseFellThrough("application-1", _testBed.Today.AddDays(30));

            sut.SaleStage.Should().Be(HomeSaleStage.UnderContract.DisplayName);
            sut.UnderContractDate.Should().Be(_testBed.Today.AddDays(35));
            sut.AvailableAgainDate.Should().Be(_testBed.Today.AddDays(30));
        }

        [Fact]
        public void GivenHomeUnderContract_WhenDirectPurchaseFellThrough_ThenNothingChanges()
        {
            var sut = _testBed.UnderContract();

            sut.RecordPurchaseFellThrough(applicationId: null, _testBed.Today);

            sut.SaleStage.Should().Be(HomeSaleStage.UnderContract.DisplayName);
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

            sut.SaleStage.Should().Be(HomeSaleStage.NotForSale.DisplayName);
            sut.LeasedDate.Should().Be(_testBed.Today.AddDays(60));
        }

        [Fact]
        public void GivenAvailableHome_WhenLeasedDirectly_ThenHomeIsLeasedFromTheLeaseStart()
        {
            var sut = _testBed.Sut;

            sut.RecordLeased(_testBed.Today.AddYears(-6));

            sut.SaleStage.Should().Be(HomeSaleStage.NotForSale.DisplayName);
            sut.LeasedDate.Should().Be(_testBed.Today.AddYears(-6));
        }

        [Fact]
        public void GivenLeasedHome_WhenALateChoiceArrives_ThenHomeStaysLeased()
        {
            var sut = _testBed.UnderContract();
            sut.RecordLeased(_testBed.Today.AddDays(60));

            sut.RecordApplicationSelected("selection-2", "application-2", _testBed.Today.AddDays(90), _testBed.FirstChosenAt.AddDays(83));

            sut.SaleStage.Should().Be(HomeSaleStage.NotForSale.DisplayName);
            sut.SaleStageSince.Should().Be(_testBed.Today.AddDays(60));
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

            sut.SaleStage.Should().Be(HomeSaleStage.AwaitingTrustDecision.DisplayName);
            sut.ResaleNoticeDate.Should().Be(_testBed.Today);
        }

        [Fact]
        public void GivenResaleOpenedBeforeNewsOfTheLease_WhenLeased_ThenHomeStaysInResale()
        {
            var sut = _testBed.Sut;
            sut.RecordResaleOpened("resale-1", _testBed.Today);

            sut.RecordLeased(_testBed.Today.AddYears(-3));

            sut.SaleStage.Should().Be(HomeSaleStage.AwaitingTrustDecision.DisplayName);
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

            sut.SaleStage.Should().Be(HomeSaleStage.NotForSale.DisplayName);
            sut.ResaleNoticeDate.Should().BeNull();
        }

        [Fact]
        public void GivenWithdrawalArrivesFirst_WhenItsOpeningArrives_ThenHomeStaysLeased()
        {
            var sut = _testBed.Sut;
            sut.RecordLeased(_testBed.Today.AddYears(-3));
            sut.RecordResaleWithdrawn("resale-1");

            sut.RecordResaleOpened("resale-1", _testBed.Today);

            sut.SaleStage.Should().Be(HomeSaleStage.NotForSale.DisplayName);
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

    public class TheRecordResaleBoughtBackMethod
    {
        private readonly TestBed _testBed = new();

        private HomeAvailabilityView ResoldHome()
        {
            _testBed.Sut.RecordApplicationSelected("selection-old", "application-old", _testBed.Today.AddYears(-3).AddDays(5), _testBed.FirstChosenAt.AddYears(-3));
            _testBed.Sut.RecordLeased(_testBed.Today.AddYears(-3).AddDays(40));
            _testBed.Sut.RecordResaleOpened("resale-1", _testBed.Today.AddDays(-30));
            return _testBed.Sut;
        }

        [Fact]
        public void GivenOfferedForSale_WhenBoughtBack_ThenVacantAndOfferedWithNoStaleSelection()
        {
            var sut = ResoldHome();

            sut.RecordResaleBoughtBack("resale-1", _testBed.Today, true, null);

            sut.Occupancy.Should().Be(HomeOccupancy.Vacant.DisplayName);
            sut.SaleStage.Should().Be(HomeSaleStage.Offered.DisplayName);
            sut.UnderContractDate.Should().BeNull();
            sut.AvailableAgainDate.Should().Be(_testBed.Today);
            sut.AvailableAgainCause.Should().Be("TrustBoughtBack");
            sut.SaleStageSince.Should().Be(_testBed.Today);
            sut.LatestSelectionId.Should().Be("selection-old");
        }

        [Fact]
        public void GivenHeld_WhenBoughtBack_ThenVacantAndHeldByTrust()
        {
            var sut = ResoldHome();

            sut.RecordResaleBoughtBack("resale-1", _testBed.Today, false, "Roof work");

            sut.Occupancy.Should().Be(HomeOccupancy.Vacant.DisplayName);
            sut.SaleStage.Should().Be(HomeSaleStage.HeldByTrust.DisplayName);
            sut.HeldSinceDate.Should().Be(_testBed.Today);
            sut.HoldReason.Should().Be("Roof work");
            sut.SaleStageSince.Should().Be(_testBed.Today);
        }

        [Fact]
        public void GivenBuybackNewsBeforeNewsOfTheNotice_WhenOpenedAndLeased_ThenStillVacantAndHeld()
        {
            _testBed.Sut.RecordResaleBoughtBack("resale-1", _testBed.Today, false, "Roof work");
            _testBed.Sut.RecordResaleOpened("resale-1", _testBed.Today.AddDays(-30));
            _testBed.Sut.RecordLeased(_testBed.Today.AddYears(-3));

            _testBed.Sut.Occupancy.Should().Be(HomeOccupancy.Vacant.DisplayName);
            _testBed.Sut.SaleStage.Should().Be(HomeSaleStage.HeldByTrust.DisplayName);
        }

        [Fact]
        public void GivenHeldHome_WhenReleased_ThenOffered()
        {
            var sut = ResoldHome();
            sut.RecordResaleBoughtBack("resale-1", _testBed.Today, false, "Roof work");

            sut.Release(_testBed.Today.AddDays(10));

            sut.SaleStage.Should().Be(HomeSaleStage.Offered.DisplayName);
            sut.HeldSinceDate.Should().BeNull();
            sut.AvailableAgainDate.Should().Be(_testBed.Today.AddDays(10));
            sut.AvailableAgainCause.Should().Be("TrustReleased");
            sut.SaleStageSince.Should().Be(_testBed.Today.AddDays(10));
        }

        [Fact]
        public void GivenHomeNotHeld_WhenReleased_ThenThrowsBusinessRuleViolation()
        {
            var sut = ResoldHome();

            var act = () => sut.Release(_testBed.Today);

            act.Should().Throw<CommonGround.SharedKernel.Domain.Exceptions.BusinessRuleViolationException>()
                .Which.Code.Should().Be("property_registry.home.release_for_sale.not_held");
        }

        [Fact]
        public void GivenBoughtBackHome_WhenANewChoiceAndLeaseFollow_ThenTheNewOnesCount()
        {
            var sut = ResoldHome();
            sut.RecordResaleBoughtBack("resale-1", _testBed.Today, true, null);

            sut.RecordApplicationSelected("selection-new", "application-new", _testBed.Today.AddDays(1), _testBed.FirstChosenAt.AddDays(1));
            sut.SaleStage.Should().Be(HomeSaleStage.UnderContract.DisplayName);
            sut.LatestSelectionId.Should().Be("selection-new");

            sut.RecordLeased(_testBed.Today.AddDays(60));
            sut.Occupancy.Should().Be(HomeOccupancy.Occupied.DisplayName);
            sut.SaleStage.Should().Be(HomeSaleStage.NotForSale.DisplayName);
        }
    }
}
