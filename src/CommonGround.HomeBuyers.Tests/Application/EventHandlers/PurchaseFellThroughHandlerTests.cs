using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Application.EventHandlers;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.HomeSelections.Events;
using CommonGround.Homeownership.Contracts.Events;
using FluentAssertions;
using JasperFx.Events;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Tests.Application.EventHandlers;

public class PurchaseFellThroughHandlerTests
{
    public class TheHandleMethod
    {
        private readonly HandlerTestBed _testBed = new();

        [Fact]
        public void GivenSelectedApplicationAndOthersWaiting_WhenPurchaseFellThrough_ThenEndsTheSelectionStartsAnotherAndReturnsTheHousehold()
        {
            var (application, selection) = SelectedApplicationWithSelection();
            var waiting = _testBed.ReadyHousehold();
            var next = new SelectionStart([SelectionCandidate.Create(
                _testBed.ApprovedApplication(waiting).Id, waiting.Id, 65000m, 1, false, new DateOnly(2026, 6, 1))], null);

            var op = PurchaseFellThroughHandler.Handle(FellThrough(application), new FallenThroughPurchase(application, selection, next));

            op.Should().BeAssignableTo<IStartStream>();
            selection.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<HomeSelectionPurchaseFellThrough>();
            application.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<ApplicationReturnedToWaitingList>();
        }

        [Fact]
        public void GivenSelectedApplicationAndNobodyWaiting_WhenPurchaseFellThrough_ThenStartsNoSelection()
        {
            var (application, selection) = SelectedApplicationWithSelection();

            var op = PurchaseFellThroughHandler.Handle(
                FellThrough(application), new FallenThroughPurchase(application, selection, new SelectionStart([], null)));

            op.Should().NotBeAssignableTo<IStartStream>();
            application.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<ApplicationReturnedToWaitingList>();
        }

        [Fact]
        public void GivenApplicationNoLongerSelected_WhenPurchaseFellThrough_ThenChangesNothing()
        {
            var application = new StubEventStream<HomeownerApplication>(_testBed.ApprovedApplication(_testBed.ReadyHousehold()));

            var op = PurchaseFellThroughHandler.Handle(FellThrough(application), new FallenThroughPurchase(application, null, null));

            op.Should().NotBeAssignableTo<IStartStream>();
            application.EventsAppended.Should().BeEmpty();
        }

        private (StubEventStream<HomeownerApplication> Application, StubEventStream<HomeSelection> Selection) SelectedApplicationWithSelection()
        {
            var household = _testBed.ReadyHousehold();
            var selected = _testBed.SelectedApplication(household);
            var selection = _testBed.OpenSelection((selected, household));
            selection = Testing.EventReplay.Evolve(selection, selection.SelectByPriority(
                selected.Id, [SelectionPriority.LowerIncome], "Lowest income", _testBed.StaffUser))!;
            return (new StubEventStream<HomeownerApplication>(selected), new StubEventStream<HomeSelection>(selection));
        }

        private PurchaseFellThrough FellThrough(IEventStream<HomeownerApplication> application) =>
            new("purchase-1", HandlerTestBed.HomeId, application.Aggregate!.Id.Value, null, "Financing fell through", new DateOnly(2026, 8, 1), _testBed.StaffUser.Value);
    }
}
