using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.HomeSelections.Events;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.IncomeLimitSchedules;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Domain.HomeSelections;

public class HomeSelectionTests
{
    public class TestBed
    {
        public UserId StaffUser { get; } = UserId.Generate();

        public IReadOnlyList<SelectionCandidate> Candidates { get; } =
        [
            Candidate("application-a", 48000m, 3, true, new DateOnly(2026, 3, 1)),
            Candidate("application-b", 62000m, 2, false, new DateOnly(2026, 4, 1)),
            Candidate("application-c", 55000m, 4, false, new DateOnly(2026, 5, 1)),
        ];

        public HomeownerApplicationId A => Candidates[0].ApplicationId;
        public HomeownerApplicationId B => Candidates[1].ApplicationId;
        public HomeownerApplicationId C => Candidates[2].ApplicationId;

        public string HomeId { get; } = "home-1";

        public DateOnly Today { get; } = new(2026, 11, 12);

        /// <summary>A schedule with an 80% level whose 2-person limit (57,000) is below application B's income.</summary>
        public IncomeLimitSchedule Schedule { get; } = EventReplay.Replay<IncomeLimitSchedule>(IncomeLimitSchedule.Enter(
            IncomeLimitScheduleId.Generate(),
            new DateOnly(2026, 4, 1),
            [IncomeLimitLevel.Create(80, [.. new[] { 50000m, 57000m, 64000m, 71000m, 77000m, 82000m, 88000m, 94000m }.Select(Money.Create)])],
            scheduleOnSameDate: null,
            UserId.Generate()));

        public HomeSelectionStarted Start(
            int? incomeLimitPercentOfAmi = null, IncomeLimitSchedule? scheduleInEffect = null, IReadOnlyList<SelectionCandidate>? candidates = null, HomeSelectionId? activeSelectionId = null) =>
            HomeSelection.Start(
                HomeSelectionId.Generate(),
                HomeId,
                candidates ?? Candidates,
                incomeLimitPercentOfAmi,
                scheduleInEffect,
                Today,
                activeSelectionId,
                StaffUser);

        public HomeSelection OpenSelection() => EventReplay.Replay<HomeSelection>(Start());

        public HomeSelection OpenSelectionWithIncomeLimit() => EventReplay.Replay<HomeSelection>(Start(80, Schedule));

        public HomeSelection CompletedSelection()
        {
            var selection = OpenSelection();
            selection = selection.Decide(a => a.SelectByPriority(A, [SelectionPriority.LowerIncome], "Lowest income", StaffUser)).Aggregate;
            return selection;
        }

        public HomeSelection CancelledSelection()
        {
            var selection = OpenSelection();
            selection = selection.Decide(a => a.Cancel("Home withdrawn from sale", StaffUser)).Aggregate;
            return selection;
        }

        public HashSet<HomeownerApplicationId> AllEligible() => Eligible(A, B, C);

        public static HashSet<HomeownerApplicationId> Eligible(params HomeownerApplicationId[] applicationIds) => [.. applicationIds];

        private static SelectionCandidate Candidate(
            string applicationId, decimal annualIncome, int householdSize, bool requiresAccessibility, DateOnly waitingSince) =>
            SelectionCandidate.Create(
                HomeownerApplicationId.Create(applicationId),
                HouseholdId.Generate(),
                Money.Create(annualIncome),
                householdSize,
                requiresAccessibility,
                waitingSince,
                incomeLimit: null);
    }

    public class TheStartMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenHomeHasActiveSelection_WhenStarted_ThenThrowsHomeHasActiveSelection()
        {
            var act = () => _testBed.Start(activeSelectionId: HomeSelectionId.Generate());

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.start.home_has_active_selection");
        }

        [Fact]
        public void GivenNobodyWaiting_WhenStarted_ThenThrowsWaitingListEmpty()
        {
            var act = () => _testBed.Start(candidates: []);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.start.waiting_list_empty");
        }

        [Fact]
        public void GivenHomeWithNoIncomeLimit_WhenStarted_ThenIsOpenWithTheCandidatesSnapshottedWithoutLimits()
        {
            var sut = EventReplay.Replay<HomeSelection>(_testBed.Start());

            sut.Status.Should().Be(HomeSelectionStatus.Open);
            sut.Candidates.Should().Equal(_testBed.Candidates);
            sut.IncomeLimitPercentOfAmi.Should().BeNull();
            sut.IncomeLimits.Should().BeNull();
        }

        [Fact]
        public void GivenHomeWithNoIncomeLimitAndAScheduleInEffect_WhenStarted_ThenKeepsTheScheduleButJudgesNoCandidate()
        {
            var sut = EventReplay.Replay<HomeSelection>(_testBed.Start(scheduleInEffect: _testBed.Schedule));

            sut.IncomeLimits.Should().Be(_testBed.Schedule.Table);
            sut.IncomeLimitLevel.Should().BeNull();
            sut.Candidates.Should().OnlyContain(c => c.IncomeLimit == null);
        }

        [Fact]
        public void GivenHomeWithIncomeLimitAndNoScheduleInEffect_WhenStarted_ThenThrowsNoIncomeLimitsInEffect()
        {
            var act = () => _testBed.Start(80, scheduleInEffect: null);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Should().BeEquivalentTo(new
                {
                    Code = "home_buyers.home_selection.start.no_income_limits_in_effect",
                    Args = new Dictionary<string, object?>
                    {
                        ["homeId"] = _testBed.HomeId,
                        ["percentOfAmi"] = 80,
                        ["today"] = _testBed.Today,
                    },
                });
        }

        [Fact]
        public void GivenHomeWithIncomeLimitTheScheduleHasNoLevelFor_WhenStarted_ThenThrowsNoIncomeLimitsInEffect()
        {
            var act = () => _testBed.Start(60, _testBed.Schedule);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.start.no_income_limits_in_effect");
        }

        [Fact]
        public void GivenHomeWithIncomeLimit_WhenStarted_ThenRecordsTheLimitsAndEachCandidatesLimitForItsSize()
        {
            var sut = EventReplay.Replay<HomeSelection>(_testBed.Start(80, _testBed.Schedule));

            sut.IncomeLimitPercentOfAmi.Should().Be(80);
            sut.IncomeLimits.Should().Be(_testBed.Schedule.Table);
            sut.IncomeLimitLevel.Should().Be(_testBed.Schedule.LevelFor(80));
            sut.Candidates.Select(c => (c.IncomeLimit!.Amount, c.IsOverIncomeLimit)).Should().Equal(
                (64000m, false),
                (57000m, true),
                (71000m, false));
        }
    }

    public class TheSelectByPriorityMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenNoPriorities_WhenSelected_ThenThrowsPrioritiesRequired()
        {
            var sut = _testBed.OpenSelection();

            var act = () => sut.SelectByPriority(_testBed.A, [], "Lowest income", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.select_by_priority.priorities_required");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void GivenBlankReason_WhenSelected_ThenThrowsReasonRequired(string reason)
        {
            var sut = _testBed.OpenSelection();

            var act = () => sut.SelectByPriority(_testBed.A, [SelectionPriority.LowerIncome], reason, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.select_by_priority.reason_required");
        }

        [Fact]
        public void GivenApplicationNotACandidate_WhenSelected_ThenThrowsNotACandidate()
        {
            var sut = _testBed.OpenSelection();

            var act = () => sut.SelectByPriority(
                HomeownerApplicationId.Generate(), [SelectionPriority.LowerIncome], "Lowest income", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.modify.not_a_candidate");
        }

        [Fact]
        public void GivenCompletedSelection_WhenSelected_ThenThrowsNotOpen()
        {
            var sut = _testBed.CompletedSelection();

            var act = () => sut.SelectByPriority(_testBed.B, [SelectionPriority.LowerIncome], "Lowest income", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.modify.not_open");
        }

        [Fact]
        public void GivenCandidateOverIncomeLimit_WhenSelected_ThenThrowsOverIncomeLimit()
        {
            var sut = _testBed.OpenSelectionWithIncomeLimit();

            var act = () => sut.SelectByPriority(_testBed.B, [SelectionPriority.LowerIncome], "Lowest income", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.select_by_priority.over_income_limit");
        }

        [Fact]
        public void GivenRepeatedPriority_WhenSelected_ThenCompletesCitingEachPriorityOnce()
        {
            var sut = _testBed.OpenSelection();

            (sut, var events) = sut.Decide(a => a.SelectByPriority(
                _testBed.B,
                [SelectionPriority.AccessibilityNeed, SelectionPriority.LowerIncome, SelectionPriority.AccessibilityNeed],
                "Needs the accessible unit",
                _testBed.StaffUser));

            sut.Status.Should().Be(HomeSelectionStatus.Completed);
            sut.SelectedApplicationId.Should().Be(_testBed.B);
            events.OfType<HouseholdSelectedByPriority>().Should().ContainSingle()
                .Which.Priorities.Should().Equal(SelectionPriority.AccessibilityNeed, SelectionPriority.LowerIncome);
        }
    }

    public class TheDrawLotteryMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenOneTiedCandidate_WhenDrawn_ThenThrowsTiedPoolTooSmall()
        {
            var sut = _testBed.OpenSelection();

            var act = () => sut.DrawLottery([_testBed.A], _testBed.AllEligible(), 42, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.draw_lottery.tied_pool_too_small");
        }

        [Fact]
        public void GivenSameCandidateMarkedTiedTwice_WhenDrawn_ThenCountsItOnceAndThrowsTiedPoolTooSmall()
        {
            var sut = _testBed.OpenSelection();

            var act = () => sut.DrawLottery([_testBed.A, _testBed.A], _testBed.AllEligible(), 42, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.draw_lottery.tied_pool_too_small");
        }

        [Fact]
        public void GivenTiedApplicationNotACandidate_WhenDrawn_ThenThrowsNotACandidate()
        {
            var sut = _testBed.OpenSelection();

            var act = () => sut.DrawLottery(
                [_testBed.A, HomeownerApplicationId.Generate()], _testBed.AllEligible(), 42, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.modify.not_a_candidate");
        }

        [Fact]
        public void GivenNoEligibleCandidates_WhenDrawn_ThenThrowsNoEligibleCandidates()
        {
            var sut = _testBed.OpenSelection();

            var act = () => sut.DrawLottery(null, TestBed.Eligible(), 42, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.draw_lottery.no_eligible_candidates");
        }

        [Fact]
        public void GivenCancelledSelection_WhenDrawn_ThenThrowsNotOpen()
        {
            var sut = _testBed.CancelledSelection();

            var act = () => sut.DrawLottery(null, _testBed.AllEligible(), 42, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.modify.not_open");
        }

        [Fact]
        public void GivenNoTies_WhenDrawn_ThenDrawsFromEveryCandidateAndCompletes()
        {
            var sut = _testBed.OpenSelection();

            (sut, var events) = sut.Decide(a => a.DrawLottery(null, _testBed.AllEligible(), 42, _testBed.StaffUser));

            sut.Status.Should().Be(HomeSelectionStatus.Completed);
            var drawn = events.OfType<HouseholdSelectedByLottery>().Should().ContainSingle().Subject;
            drawn.PoolApplicationIds.Should().Equal("application-a", "application-b", "application-c");
            drawn.Seed.Should().Be(42);
            sut.SelectedApplicationId?.Value.Should().Be(drawn.ApplicationId);
        }

        [Fact]
        public void GivenTiedCandidates_WhenDrawn_ThenDrawsOnlyBetweenThem()
        {
            var sut = _testBed.OpenSelection();

            (sut, var events) = sut.Decide(a => a.DrawLottery([_testBed.C, _testBed.A], _testBed.AllEligible(), 42, _testBed.StaffUser));

            events.OfType<HouseholdSelectedByLottery>().Should().ContainSingle()
                .Which.PoolApplicationIds.Should().Equal("application-a", "application-c");
        }

        [Fact]
        public void GivenIneligibleCandidates_WhenDrawn_ThenLeavesThemOutOfThePoolAndRecordsThem()
        {
            var sut = _testBed.OpenSelection();

            (sut, var events) = sut.Decide(a => a.DrawLottery(null, TestBed.Eligible(_testBed.B), 42, _testBed.StaffUser));

            sut.SelectedApplicationId.Should().Be(_testBed.B);
            events.OfType<HouseholdSelectedByLottery>().Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new
                {
                    PoolApplicationIds = new[] { "application-b" },
                    IneligibleApplicationIds = new[] { "application-a", "application-c" },
                });
        }

        [Fact]
        public void GivenCandidateOverIncomeLimit_WhenDrawn_ThenLeavesThemOutOfThePoolAndRecordsThem()
        {
            var sut = _testBed.OpenSelectionWithIncomeLimit();

            (_, var events) = sut.Decide(a => a.DrawLottery(null, _testBed.AllEligible(), 42, _testBed.StaffUser));

            events.OfType<HouseholdSelectedByLottery>().Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new
                {
                    PoolApplicationIds = new[] { "application-a", "application-c" },
                    IneligibleApplicationIds = new[] { "application-b" },
                });
        }
    }

    public class TheCancelMethod
    {
        private readonly TestBed _testBed = new();

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void GivenBlankReason_WhenCancelled_ThenThrowsReasonRequired(string reason)
        {
            var sut = _testBed.OpenSelection();

            var act = () => sut.Cancel(reason, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.cancel.reason_required");
        }

        [Fact]
        public void GivenCompletedSelection_WhenCancelled_ThenThrowsNotOpen()
        {
            var sut = _testBed.CompletedSelection();

            var act = () => sut.Cancel("Changed our minds", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.modify.not_open");
        }

        [Fact]
        public void GivenOpenSelection_WhenCancelled_ThenIsCancelledWithNobodySelected()
        {
            var sut = _testBed.OpenSelection();

            sut = sut.Decide(a => a.Cancel("Home withdrawn from sale", _testBed.StaffUser)).Aggregate;

            sut.Status.Should().Be(HomeSelectionStatus.Cancelled);
            sut.CancellationReason.Should().Be("Home withdrawn from sale");
            sut.SelectedApplicationId.Should().BeNull();
        }
    }

    public class TheRecordPurchaseFellThroughMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenOpenSelection_WhenPurchaseFellThrough_ThenThrowsNotCompleted()
        {
            var sut = _testBed.OpenSelection();

            var act = () => sut.RecordPurchaseFellThrough("Financing fell through", new DateOnly(2026, 9, 1));

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.record_purchase_fell_through.not_completed");
        }

        [Fact]
        public void GivenCompletedSelection_WhenPurchaseFellThrough_ThenHasFallenThroughKeepingTheChosenHousehold()
        {
            var sut = _testBed.CompletedSelection();
            var chosen = sut.SelectedApplicationId;

            sut = sut.Decide(a => a.RecordPurchaseFellThrough("Financing fell through", new DateOnly(2026, 9, 1))).Aggregate;

            sut.Status.Should().Be(HomeSelectionStatus.FellThrough);
            sut.SelectedApplicationId.Should().Be(chosen);
        }
    }
}

