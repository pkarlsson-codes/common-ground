using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.HomeBuyers.Domain.Households;
using FluentAssertions;
using JasperFx.Events;

namespace CommonGround.HomeBuyers.Tests.Application.Commands;

public class HomeSelectionCompletionTests
{
    public class TheCompleteMethod
    {
        private readonly HandlerTestBed _testBed = new();

        [Fact]
        public void GivenChosenEligibleAndNotReadyCandidates_WhenCompleted_ThenSelectsTheChosenAndPassesOverOnlyTheEligible()
        {
            var chosenHousehold = _testBed.ReadyHousehold();
            var eligibleHousehold = _testBed.ReadyHousehold();
            var notReadyHousehold = _testBed.NewHousehold(ready: false);
            (HomeownerApplication, Household)[] candidates =
            [
                (_testBed.ApprovedApplication(chosenHousehold), chosenHousehold),
                (_testBed.ApprovedApplication(eligibleHousehold), eligibleHousehold),
                (_testBed.ApprovedApplication(notReadyHousehold), notReadyHousehold),
            ];
            var completion = _testBed.Completion(_testBed.OpenSelection(candidates), candidates);

            HomeSelectionCompletion.Complete(
                completion, candidates[0].Item1.Id, "template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser);

            completion.Candidates.Select(c => ((StubEventStream<HomeownerApplication>)c.Application).EventsAppended.SingleOrDefault()?.GetType())
                .Should().Equal(typeof(ApplicationSelectedForHome), typeof(ApplicationPassedOver), null);
        }
    }
}
