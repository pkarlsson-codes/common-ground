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

        public static HomeFactsView Home(string label, int? percent) =>
            new() { Id = $"home-{label}", Label = label, IncomeLimitPercentOfAmi = percent };

        public static IncomeLimitScheduleView Schedule(string id, DateOnly effectiveFrom, params int[] percents) => new()
        {
            Id = id,
            EffectiveFrom = effectiveFrom,
            Levels = [.. percents.Select(p => new IncomeLimitLevelEntry { PercentOfAmi = p })],
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

    public class TheForIncomeLimitsMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenNoHomeHasAnIncomeLimit_WhenDerived_ThenNothingIsWaitingEvenWithNoSchedules()
        {
            var items = HomeBuyersWorkItems.ForIncomeLimits([TestBed.Home("Maple Row", null)], [], _testBed.Today);

            items.Should().BeEmpty();
        }

        [Fact]
        public void GivenNoScheduleInEffect_WhenDerived_ThenMissingNamesEveryPercentAndAtMostThreeHomes()
        {
            var homes = new[]
            {
                TestBed.Home("D Street", 80), TestBed.Home("A Street", 120), TestBed.Home("C Street", 80),
                TestBed.Home("B Street", 80), TestBed.Home("Maple Row", null),
            };
            var future = TestBed.Schedule("schedule-future", _testBed.Today.AddDays(1), 80, 120);

            var items = HomeBuyersWorkItems.ForIncomeLimits(homes, [future], _testBed.Today);

            items.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                Area = "selections",
                Kind = HomeBuyersWorkItems.IncomeLimitsMissing,
                RecordType = "income-limits",
                RecordId = string.Empty,
                Detail = "No 80%, 120% limits in effect; households can't be chosen for A Street, B Street, C Street and 1 more",
                WaitingSince = _testBed.Today,
                Actions = new[] { HomeBuyersWorkItems.EnterIncomeLimits },
            });
        }

        [Fact]
        public void GivenScheduleInEffectLacksALevel_WhenDerived_ThenMissingNamesOnlyThatLevelsHomes()
        {
            var homes = new[] { TestBed.Home("A Street", 80), TestBed.Home("B Street", 60) };
            var schedule = TestBed.Schedule("schedule-1", _testBed.Today.AddMonths(-2), 50, 80);

            var items = HomeBuyersWorkItems.ForIncomeLimits(homes, [schedule], _testBed.Today);

            items.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                Kind = HomeBuyersWorkItems.IncomeLimitsMissing,
                RecordId = "schedule-1",
                Detail = "No 60% limits in effect; households can't be chosen for B Street",
            });
        }

        [Theory]
        [InlineData(12, false)]
        [InlineData(13, true)]
        public void GivenScheduleInEffectMonthsOld_WhenDerived_ThenOutOfDateOnlyPastTwelveMonths(int monthsOld, bool stale)
        {
            var effectiveFrom = _testBed.Today.AddMonths(-monthsOld);
            var schedule = TestBed.Schedule("schedule-1", effectiveFrom, 80);

            var items = HomeBuyersWorkItems.ForIncomeLimits([TestBed.Home("A Street", 80)], [schedule], _testBed.Today);

            if (stale)
            {
                items.Should().ContainSingle().Which.Should().BeEquivalentTo(new
                {
                    Kind = HomeBuyersWorkItems.IncomeLimitsStale,
                    RecordId = "schedule-1",
                    Detail = "In effect since Oct 2, 2025, over 12 months ago; HUD publishes new limits every year",
                    WaitingSince = effectiveFrom.AddMonths(12),
                    Actions = new[] { HomeBuyersWorkItems.EnterIncomeLimits },
                });
            }
            else
            {
                items.Should().BeEmpty();
            }
        }

        [Fact]
        public void GivenOldScheduleInEffectAndALaterOneEntered_WhenDerived_ThenNotOutOfDate()
        {
            var old = TestBed.Schedule("schedule-old", _testBed.Today.AddYears(-2), 80);
            var next = TestBed.Schedule("schedule-next", _testBed.Today.AddMonths(1), 80);

            var items = HomeBuyersWorkItems.ForIncomeLimits([TestBed.Home("A Street", 80)], [old, next], _testBed.Today);

            items.Should().BeEmpty();
        }
    }
}
