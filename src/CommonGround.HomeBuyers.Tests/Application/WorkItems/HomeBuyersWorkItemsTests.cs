using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Application.WorkItems;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Application.WorkItems;

public class HomeBuyersWorkItemsTests
{
    public class TestBed
    {
        public DateOnly Today { get; } = new(2026, 11, 2);
        public IanaTimeZone Zone { get; } = IanaTimeZone.Create("America/New_York");

        // 01:00 UTC on Oct 20 is still Oct 19 in New York.
        public DateTimeOffset SubmittedAt { get; } = new(2026, 10, 20, 1, 0, 0, TimeSpan.Zero);

        public HomeownerApplicationListItemView Application(string phase) =>
            new() { Id = "application-1", HouseholdId = "household-1", Phase = phase, SubmittedAt = SubmittedAt };

        public HouseholdListItemView ReadyHousehold(DateOnly prequalificationExpiry) => new()
        {
            Id = "household-1",
            PrimaryMemberName = "Alice Johnson",
            IsHousingCounselingCompleted = true,
            IsCltOrientationCompleted = true,
            MortgagePrequalificationExpiryDate = prequalificationExpiry,
        };
    }

    public class TheForApplicationMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenSubmittedApplicationOfReadyHousehold_WhenDerived_ThenAwaitsApprovalOrDenialSinceTheTrustDateItWasSubmitted()
        {
            var household = _testBed.ReadyHousehold(_testBed.Today.AddDays(90));

            var items = HomeBuyersWorkItems.ForApplication(_testBed.Application("Submitted"), household, _testBed.Today, _testBed.Zone);

            items.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                Kind = HomeBuyersWorkItems.ApplicationAwaitingDecision,
                RecordId = "application-1",
                Label = "Alice Johnson",
                WaitingSince = new DateOnly(2026, 10, 19),
                Actions = new[] { HomeBuyersWorkItems.ApproveApplication, HomeBuyersWorkItems.DenyApplication },
            });
        }

        [Fact]
        public void GivenApprovedApplicationOfReadyHousehold_WhenDerived_ThenNothingIsWaiting()
        {
            var household = _testBed.ReadyHousehold(_testBed.Today.AddDays(90));

            var items = HomeBuyersWorkItems.ForApplication(_testBed.Application("Approved"), household, _testBed.Today, _testBed.Zone);

            items.Should().BeEmpty();
        }

        [Fact]
        public void GivenHouseholdMissingOrientationAndPrequalification_WhenDerived_ThenListsBothMissingStepsAndOffersToRecordEach()
        {
            var household = _testBed.ReadyHousehold(_testBed.Today);
            household.IsCltOrientationCompleted = false;
            household.MortgagePrequalificationExpiryDate = null;

            var items = HomeBuyersWorkItems.ForApplication(_testBed.Application("Approved"), household, _testBed.Today, _testBed.Zone);

            items.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                Kind = HomeBuyersWorkItems.ReadinessStepMissing,
                RecordType = "household",
                RecordId = "household-1",
                Detail = "Missing CLT orientation, mortgage prequalification",
                Actions = new[] { HomeBuyersWorkItems.RecordCltOrientation, HomeBuyersWorkItems.RecordMortgagePrequalification },
            }, o => o.WithStrictOrdering());
        }

        [Fact]
        public void GivenPrequalificationExpiredYesterday_WhenDerived_ThenNeedsACurrentOne()
        {
            var household = _testBed.ReadyHousehold(_testBed.Today.AddDays(-1));

            var items = HomeBuyersWorkItems.ForApplication(_testBed.Application("Approved"), household, _testBed.Today, _testBed.Zone);

            items.Should().ContainSingle().Which.Detail.Should().Be("Missing a current mortgage prequalification");
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(30, true)]
        [InlineData(31, false)]
        public void GivenPrequalificationExpiringInDays_WhenDerived_ThenExpiringOnlyWithinTheWindow(int days, bool expiring)
        {
            var expiry = _testBed.Today.AddDays(days);
            var household = _testBed.ReadyHousehold(expiry);

            var items = HomeBuyersWorkItems.ForApplication(_testBed.Application("Approved"), household, _testBed.Today, _testBed.Zone);

            if (expiring)
            {
                items.Should().ContainSingle().Which.Should().BeEquivalentTo(new
                {
                    Kind = HomeBuyersWorkItems.PrequalificationExpiring,
                    DueDate = (DateOnly?)expiry,
                    WaitingSince = expiry.AddDays(-30),
                    Actions = new[] { HomeBuyersWorkItems.RecordMortgagePrequalification },
                });
            }
            else
            {
                items.Should().BeEmpty();
            }
        }

        [Fact]
        public void GivenHouseholdNotArrived_WhenDerived_ThenOnlyTheDecisionIsWaiting()
        {
            var items = HomeBuyersWorkItems.ForApplication(_testBed.Application("Submitted"), null, _testBed.Today, _testBed.Zone);

            items.Should().ContainSingle().Which.Kind.Should().Be(HomeBuyersWorkItems.ApplicationAwaitingDecision);
        }
    }

    public class TheForSelectionMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenOpenSelection_WhenDerived_ThenWaitsSinceItStartedWithItsCandidateCount()
        {
            var selection = new HomeSelectionView
            {
                Id = "selection-1",
                HomeId = "home-1",
                StartedAt = new DateTimeOffset(2026, 10, 28, 15, 0, 0, TimeSpan.Zero),
                Candidates = [new HomeSelectionCandidateEntry(), new HomeSelectionCandidateEntry()],
            };

            var item = HomeBuyersWorkItems.ForSelection(selection, "14 Alder Lane", _testBed.Zone);

            item.Should().BeEquivalentTo(new
            {
                Kind = HomeBuyersWorkItems.SelectionOpen,
                Label = "14 Alder Lane",
                Detail = "Open, 2 candidates to choose from",
                WaitingSince = new DateOnly(2026, 10, 28),
                Actions = new[] { HomeBuyersWorkItems.ChooseHousehold },
            });
        }
    }
}
