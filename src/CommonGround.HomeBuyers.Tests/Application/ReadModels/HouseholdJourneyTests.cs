using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Application.ReadModels;

public class HouseholdJourneyTests
{
    public class TestBed
    {
        public DateOnly Today { get; } = new(2026, 10, 1);
        public DateOnly PrequalifiedOn { get; } = new(2026, 3, 20);

        public HouseholdView Household { get; } = new() { Id = "household-1" };

        public List<HomeownerApplicationListItemView> Applications { get; } = [];

        public TestBed Ready()
        {
            Household.HousingCounselingCompletedDate = new DateOnly(2026, 3, 1);
            Household.CltOrientationCompletedDate = new DateOnly(2026, 3, 10);
            Household.MortgagePrequalification = new MortgagePrequalificationEntry { ExpiryDate = new DateOnly(2027, 1, 1) };
            return this;
        }

        public TestBed WithApplication(ApplicationPhase phase, DateOnly? approved = null, DateOnly? selected = null, DateOnly? purchased = null)
        {
            Applications.Add(new HomeownerApplicationListItemView
            {
                Id = $"application-{Applications.Count + 1}",
                HouseholdId = Household.Id,
                Phase = phase.DisplayName,
                ApprovedDate = approved,
                SelectedHomeId = selected is null ? null : "home-1",
                SelectedDate = selected,
                PurchasedDate = purchased,
            });
            return this;
        }

        public HouseholdJourney Derive() => HouseholdJourney.Derive(Household, Applications, PrequalifiedOn, Today);
    }

    public class TheDeriveMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenReadinessIncomplete_WhenDerived_ThenIsAtIntake()
        {
            var journey = _testBed.Derive();

            journey.Stage.Should().Be(HouseholdJourney.Intake);
            journey.ReadyDate.Should().BeNull();
        }

        [Fact]
        public void GivenReadinessComplete_WhenDerived_ThenIsReadyFromTheLastItemRecorded()
        {
            var journey = _testBed.Ready().Derive();

            journey.Stage.Should().Be(HouseholdJourney.Ready);
            journey.ReadyDate.Should().Be(_testBed.PrequalifiedOn);
        }

        [Fact]
        public void GivenPrequalificationExpired_WhenDerived_ThenIsBackAtIntake()
        {
            _testBed.Ready().Household.MortgagePrequalification!.ExpiryDate = _testBed.Today.AddDays(-1);

            var journey = _testBed.Derive();

            journey.Stage.Should().Be(HouseholdJourney.Intake);
        }

        [Fact]
        public void GivenSubmittedApplication_WhenDerived_ThenIsReadyWithTheApplicationAwaitingADecision()
        {
            var journey = _testBed.Ready().WithApplication(ApplicationPhase.Submitted).Derive();

            journey.Stage.Should().Be(HouseholdJourney.Ready);
            journey.CurrentApplicationId.Should().Be("application-1");
        }

        [Fact]
        public void GivenApprovedApplication_WhenDerived_ThenIsWaitingSinceApproval()
        {
            var journey = _testBed.Ready().WithApplication(ApplicationPhase.Approved, approved: new DateOnly(2026, 4, 18)).Derive();

            journey.Stage.Should().Be(HouseholdJourney.Waiting);
            journey.ApprovedDate.Should().Be(new DateOnly(2026, 4, 18));
        }

        [Fact]
        public void GivenSelectedApplication_WhenDerived_ThenIsPurchasingTheHomeItWasSelectedFor()
        {
            var journey = _testBed.Ready()
                .WithApplication(ApplicationPhase.Selected, approved: new DateOnly(2026, 4, 18), selected: new DateOnly(2026, 9, 12))
                .Derive();

            journey.Stage.Should().Be(HouseholdJourney.Purchasing);
            journey.SelectedDate.Should().Be(new DateOnly(2026, 9, 12));
            journey.SelectedHomeId.Should().Be("home-1");
        }

        [Fact]
        public void GivenPurchasedApplication_WhenDerived_ThenIsHomeownerEvenIfReadinessHasSinceLapsed()
        {
            _testBed.WithApplication(ApplicationPhase.Purchased,
                approved: new DateOnly(2026, 4, 18), selected: new DateOnly(2026, 9, 12), purchased: new DateOnly(2026, 10, 30));

            var journey = _testBed.Derive();

            journey.Stage.Should().Be(HouseholdJourney.Homeowner);
            journey.PurchasedDate.Should().Be(new DateOnly(2026, 10, 30));
        }

        [Fact]
        public void GivenPurchaseFellThrough_WhenDerived_ThenIsWaitingAgainWithTheOriginalApprovalDate()
        {
            // A fallen-through application is approved again, with its selection cleared.
            var journey = _testBed.Ready().WithApplication(ApplicationPhase.Approved, approved: new DateOnly(2026, 4, 18)).Derive();

            journey.Stage.Should().Be(HouseholdJourney.Waiting);
            journey.ApprovedDate.Should().Be(new DateOnly(2026, 4, 18));
            journey.SelectedDate.Should().BeNull();
        }

        [Fact]
        public void GivenOnlyAWithdrawnApplication_WhenDerived_ThenIsBackToReady()
        {
            var journey = _testBed.Ready().WithApplication(ApplicationPhase.Withdrawn, approved: new DateOnly(2026, 4, 18)).Derive();

            journey.Stage.Should().Be(HouseholdJourney.Ready);
            journey.ApprovedDate.Should().BeNull();
        }
    }
}
