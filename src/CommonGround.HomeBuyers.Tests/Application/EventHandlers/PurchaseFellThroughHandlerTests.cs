using CommonGround.HomeBuyers.Application.EventHandlers;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.Homeownership.Contracts.Events;
using FluentAssertions;
using JasperFx.Events;

namespace CommonGround.HomeBuyers.Tests.Application.EventHandlers;

public class PurchaseFellThroughHandlerTests
{
    public class TheHandleMethod
    {
        private readonly HandlerTestBed _testBed = new();

        [Fact]
        public void GivenSelectedApplication_WhenPurchaseFellThrough_ThenReturnsTheHouseholdToTheWaitingList()
        {
            var application = new StubEventStream<HomeownerApplication>(_testBed.SelectedApplication(_testBed.ReadyHousehold()));

            PurchaseFellThroughHandler.Handle(FellThrough(application.Aggregate!.Id.Value), application);

            application.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<ApplicationReturnedToWaitingList>();
        }

        [Fact]
        public void GivenPurchaseStartedDirectly_WhenItFellThrough_ThenDoesNothing()
        {
            var act = () => PurchaseFellThroughHandler.Handle(FellThrough(applicationId: null), application: null);

            act.Should().NotThrow();
        }

        private PurchaseFellThrough FellThrough(string? applicationId) =>
            new("purchase-1", HandlerTestBed.HomeId, applicationId, null, "Financing fell through", _testBed.SelectedDate.AddDays(30), _testBed.StaffUser.Value);
    }
}
