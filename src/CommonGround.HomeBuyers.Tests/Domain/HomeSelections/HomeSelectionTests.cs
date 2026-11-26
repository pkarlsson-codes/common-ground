using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.HomeSelections.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Domain.HomeSelections;

public class HomeSelectionTests
{
    public class TheSelectByPriorityMethod
    {
        private readonly SelectionTestBed _testBed = new();

        private HouseholdSelectedByPriority Select(
            WaitingListComparison? comparison = null,
            HomeownerApplicationId? applicationId = null,
            IReadOnlyCollection<SelectionPriority>? priorities = null,
            string reason = "Longest waiting",
            SelectionTerms? terms = null) =>
            HomeSelection.SelectByPriority(
                HomeSelectionId.Generate(),
                _testBed.HomeId,
                comparison ?? _testBed.Compare(),
                applicationId ?? _testBed.A,
                priorities ?? [SelectionPriority.WaitingListTenure],
                reason,
                terms ?? _testBed.Terms(),
                _testBed.Today,
                _testBed.StaffUser);

        [Fact]
        public void GivenHouseholdThatCanBeChosen_WhenChosen_ThenRecordsTheWholeWaitingListWithEachOutcome()
        {
            var chosen = Select(comparison: _testBed.CompareWithIncomeLimit());

            chosen.Should().BeEquivalentTo(new
            {
                HomeId = _testBed.HomeId,
                ApplicationId = _testBed.A.Value,
                SelectedDate = _testBed.Today,
                SelectedBy = _testBed.StaffUser.Value,
                GroundLeaseTemplateId = "template-1",
                PurchasePrice = 200000m,
                IncomeLimitPercentOfAmi = 80,
                IncomeLimitsEffectiveFrom = new DateOnly(2026, 4, 1),
                Priorities = new[] { SelectionPriority.WaitingListTenure },
                Reason = "Longest waiting",
            });
            chosen.Applications.Select(a => (a.ApplicationId, a.Outcome, a.Reasons.Count)).Should().Equal(
                (_testBed.A.Value, SelectionOutcome.Chosen, 0),
                (_testBed.B.Value, SelectionOutcome.CouldNotBeChosen, 1),
                (_testBed.C.Value, SelectionOutcome.PassedOver, 0));
            chosen.Applications[1].Reasons.Should().Equal(CannotBeChosenReason.OverIncomeLimit);
            chosen.Applications[1].IncomeLimit.Should().Be(Money.Create(57000m));
        }

        [Fact]
        public void GivenNoPriorities_WhenChosen_ThenThrowsPrioritiesRequired()
        {
            var act = () => Select(priorities: []);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.select_by_priority.priorities_required");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void GivenBlankReason_WhenChosen_ThenThrowsReasonRequired(string reason)
        {
            var act = () => Select(reason: reason);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.select_by_priority.reason_required");
        }

        [Fact]
        public void GivenDateAfterToday_WhenChosen_ThenThrowsSelectedDateInFuture()
        {
            var tomorrow = _testBed.Today.AddDays(1);

            var act = () => Select(comparison: _testBed.Compare(asOf: tomorrow), terms: _testBed.Terms(tomorrow));

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.choose.selected_date_in_future");
        }

        [Fact]
        public void GivenWaitingListComparedOnAnotherDate_WhenChosen_ThenThrowsArgumentException()
        {
            var act = () => Select(comparison: _testBed.Compare(asOf: _testBed.Today.AddDays(-1)));

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GivenNobodyWaiting_WhenChosen_ThenThrowsWaitingListEmpty()
        {
            var act = () => Select(comparison: _testBed.Compare(waiting: []));

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.choose.waiting_list_empty");
        }

        [Fact]
        public void GivenHomeWithALimitAndNoLimitsInEffect_WhenChosen_ThenThrowsNoIncomeLimitsInEffect()
        {
            var act = () => Select(comparison: _testBed.Compare(incomeLimitPercentOfAmi: 80, limits: null));

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.choose.no_income_limits_in_effect");
        }

        [Fact]
        public void GivenApplicationNotOnTheWaitingList_WhenChosen_ThenThrowsNotOnWaitingList()
        {
            var act = () => Select(applicationId: HomeownerApplicationId.Generate());

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.choose.not_on_waiting_list");
        }

        [Fact]
        public void GivenHouseholdOverTheIncomeLimit_WhenChosen_ThenThrowsCannotBeChosen()
        {
            var act = () => Select(comparison: _testBed.CompareWithIncomeLimit(), applicationId: _testBed.B);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.choose.cannot_be_chosen");
        }

        [Fact]
        public void GivenTheChoice_WhenReplayed_ThenIsTheHouseholdChosenForTheHome()
        {
            var chosen = Select();

            var sut = EventReplay.Replay<HomeSelection>(chosen);

            sut.Should().BeEquivalentTo(new
            {
                Id = HomeSelectionId.Create(chosen.AggregateId),
                HomeId = _testBed.HomeId,
                SelectedApplicationId = _testBed.A,
                SelectedDate = _testBed.Today,
            });
        }
    }

    public class TheDrawLotteryMethod
    {
        private readonly SelectionTestBed _testBed = new();

        private HouseholdSelectedByLottery Draw(
            WaitingListComparison? comparison = null,
            IReadOnlyCollection<HomeownerApplicationId>? tied = null,
            int seed = 42) =>
            HomeSelection.DrawLottery(
                HomeSelectionId.Generate(),
                _testBed.HomeId,
                comparison ?? _testBed.Compare(),
                tied,
                seed,
                _testBed.Terms(),
                _testBed.Today,
                _testBed.StaffUser);

        [Fact]
        public void GivenNoTie_WhenDrawn_ThenDrawsFromEveryHouseholdThatCanBeChosen()
        {
            var drawn = Draw(comparison: _testBed.CompareWithIncomeLimit());

            drawn.TiedApplicationIds.Should().BeNull();
            drawn.PoolApplicationIds.Should().Equal(_testBed.A.Value, _testBed.C.Value);
            drawn.ApplicationId.Should().BeOneOf(_testBed.A.Value, _testBed.C.Value);
            drawn.Applications.Single(a => a.ApplicationId == _testBed.B.Value).Outcome.Should().Be(SelectionOutcome.CouldNotBeChosen);
            drawn.Applications.Single(a => a.ApplicationId == drawn.ApplicationId).Outcome.Should().Be(SelectionOutcome.Chosen);
            drawn.Applications.Count(a => a.Outcome == SelectionOutcome.PassedOver).Should().Be(1);
        }

        [Fact]
        public void GivenTheSameSeed_WhenDrawnAgain_ThenTheSameHouseholdWins()
        {
            var comparison = _testBed.Compare();

            Draw(comparison, seed: 7).ApplicationId.Should().Be(Draw(comparison, seed: 7).ApplicationId);
        }

        [Fact]
        public void GivenATie_WhenDrawn_ThenDrawsOnlyFromTheTiedAndPassesOverTheRest()
        {
            var drawn = Draw(tied: [_testBed.A, _testBed.C]);

            drawn.TiedApplicationIds.Should().Equal(_testBed.A.Value, _testBed.C.Value);
            drawn.PoolApplicationIds.Should().Equal(_testBed.A.Value, _testBed.C.Value);
            drawn.Applications.Single(a => a.ApplicationId == _testBed.B.Value).Outcome.Should().Be(SelectionOutcome.PassedOver);
        }

        [Fact]
        public void GivenATieWithAHouseholdThatCannotBeChosen_WhenDrawn_ThenLeavesItOutOfThePool()
        {
            var drawn = Draw(comparison: _testBed.CompareWithIncomeLimit(), tied: [_testBed.A, _testBed.B]);

            drawn.TiedApplicationIds.Should().Equal(_testBed.A.Value, _testBed.B.Value);
            drawn.PoolApplicationIds.Should().Equal(_testBed.A.Value);
            drawn.ApplicationId.Should().Be(_testBed.A.Value);
        }

        [Fact]
        public void GivenTieOfOne_WhenDrawn_ThenThrowsTiedPoolTooSmall()
        {
            var act = () => Draw(tied: [_testBed.A, _testBed.A]);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.draw_lottery.tied_pool_too_small");
        }

        [Fact]
        public void GivenTiedApplicationNotOnTheWaitingList_WhenDrawn_ThenThrowsNotOnWaitingList()
        {
            var act = () => Draw(tied: [_testBed.A, HomeownerApplicationId.Generate()]);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.choose.not_on_waiting_list");
        }

        [Fact]
        public void GivenNobodyInThePoolCanBeChosen_WhenDrawn_ThenThrowsNoEligibleCandidates()
        {
            var overLimit = SelectionTestBed.Ready("application-d", 90000m, 1, false, new DateOnly(2026, 6, 1));

            var act = () => Draw(comparison: _testBed.Compare(waiting: [overLimit], incomeLimitPercentOfAmi: 80, limits: _testBed.Limits));

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.draw_lottery.no_eligible_candidates");
        }
    }
}
