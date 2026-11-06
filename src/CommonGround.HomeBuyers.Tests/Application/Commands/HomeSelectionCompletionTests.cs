using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using FluentAssertions;
using JasperFx.Events;

namespace CommonGround.HomeBuyers.Tests.Application.Commands;

public class HomeSelectionCompletionTests
{
    public class TheCompleteMethod
    {
        private readonly HandlerTestBed _testBed = new();

        [Fact]
        public void GivenChosenCandidate_WhenCompleted_ThenSelectsTheirApplicationForTheHome()
        {
            var chosenHousehold = _testBed.ReadyHousehold();
            var otherHousehold = _testBed.ReadyHousehold();
            var chosenApplication = _testBed.ApprovedApplication(chosenHousehold);
            var selection = _testBed.OpenSelection(
                (chosenApplication, chosenHousehold),
                (_testBed.ApprovedApplication(otherHousehold), otherHousehold));
            var chosen = _testBed.Chosen(chosenApplication, chosenHousehold);

            HomeSelectionCompletion.Complete(selection, chosen, "template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser);

            ((StubEventStream<HomeownerApplication>)chosen.Application).EventsAppended.Should().ContainSingle()
                .Which.Should().BeOfType<ApplicationSelectedForHome>()
                .Which.HomeSelectionId.Should().Be(selection.Id.Value);
        }
    }
}
