using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Application.EventHandlers;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.HomeSelections.Events;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.Homeownership.Contracts.Events;
using CommonGround.SharedKernel.Domain;
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
            var next = NextSelection(waiting, incomeLimitPercentOfAmi: null);

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
                FellThrough(application),
                new FallenThroughPurchase(application, selection, new SelectionStart([], null, null, null, _testBed.SelectedDate)));

            op.Should().NotBeAssignableTo<IStartStream>();
            application.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<ApplicationReturnedToWaitingList>();
        }

        [Fact]
        public void GivenHomeHasIncomeLimitButNoLimitsInEffect_WhenPurchaseFellThrough_ThenStartsNoSelectionAndReturnsTheHousehold()
        {
            var (application, selection) = SelectedApplicationWithSelection();
            var next = NextSelection(_testBed.ReadyHousehold(), incomeLimitPercentOfAmi: 80);

            var op = PurchaseFellThroughHandler.Handle(FellThrough(application), new FallenThroughPurchase(application, selection, next));

            op.Should().NotBeAssignableTo<IStartStream>();
            selection.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<HomeSelectionPurchaseFellThrough>();
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

        private SelectionStart NextSelection(Household waiting, int? incomeLimitPercentOfAmi) =>
            new(
                [SelectionCandidate.Create(
                    _testBed.ApprovedApplication(waiting).Id, waiting.Id, Money.Create(65000m), 1, false, new DateOnly(2026, 6, 1), incomeLimit: null)],
                ActiveSelectionId: null,
                incomeLimitPercentOfAmi,
                ScheduleInEffect: null,
                _testBed.SelectedDate);

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
