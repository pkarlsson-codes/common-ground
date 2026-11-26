using CommonGround.HomeBuyers.Application.EventHandlers;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.HomeSelections.Events;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Application.EventHandlers;

public class PassOverCandidatesHandlerTests
{
    public class TheHandleMethod
    {
        private static readonly RecordedApplication[] Applications =
        [
            Recorded("application-a", SelectionOutcome.PassedOver),
            Recorded("application-b", SelectionOutcome.Chosen),
            Recorded("application-c", SelectionOutcome.CouldNotBeChosen, CannotBeChosenReason.OverIncomeLimit),
            Recorded("application-d", SelectionOutcome.PassedOver),
        ];

        [Fact]
        public void GivenChoiceByPriority_WhenHandled_ThenPassesOverExactlyTheApplicationsMarkedPassedOver()
        {
            var chosen = new HouseholdSelectedByPriority(
                "selection-1", HandlerTestBed.HomeId, "application-b", new DateOnly(2026, 7, 20), "user-1", "template-1", 200000m,
                null, null, null, Applications, [SelectionPriority.WaitingListTenure], "Longest waiting");

            var commands = PassOverCandidatesHandler.Handle(chosen);

            commands.Select(c => c.ApplicationId.Value).Should().Equal("application-a", "application-d");
            commands.Should().AllSatisfy(c =>
            {
                c.SelectionId.Value.Should().Be("selection-1");
                c.HomeId.Should().Be(HandlerTestBed.HomeId);
            });
        }

        [Fact]
        public void GivenLottery_WhenHandled_ThenPassesOverExactlyTheApplicationsMarkedPassedOver()
        {
            var drawn = new HouseholdSelectedByLottery(
                "selection-1", HandlerTestBed.HomeId, "application-b", new DateOnly(2026, 7, 20), "user-1", "template-1", 200000m,
                null, null, null, Applications, null, ["application-a", "application-b", "application-d"], 42);

            var commands = PassOverCandidatesHandler.Handle(drawn);

            commands.Select(c => c.ApplicationId.Value).Should().Equal("application-a", "application-d");
        }

        private static RecordedApplication Recorded(string applicationId, SelectionOutcome outcome, params CannotBeChosenReason[] reasons) =>
            new(applicationId, "household-1", Money.Create(50000m), 2, false, new DateOnly(2026, 6, 1), null, outcome, reasons);
    }
}
