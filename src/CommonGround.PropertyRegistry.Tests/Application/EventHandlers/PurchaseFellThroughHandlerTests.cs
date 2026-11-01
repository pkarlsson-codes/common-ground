using CommonGround.Homeownership.Contracts.Events;
using CommonGround.PropertyRegistry.Application.EventHandlers;
using CommonGround.PropertyRegistry.Domain.Homes.Events;
using FluentAssertions;
using Xunit;

namespace CommonGround.PropertyRegistry.Tests.Application.EventHandlers;

public class PurchaseFellThroughHandlerTests
{
    public class TheHandleMethod
    {
        private readonly HomeEventHandlerTestBed _testBed = new();

        [Fact]
        public void GivenHomeUnderContract_WhenPurchaseFellThrough_ThenReleasesTheContract()
        {
            var stream = _testBed.UnderContract();

            PurchaseFellThroughHandler.Handle(FellThrough(), stream);

            stream.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<HomeContractReleased>();
        }

        [Fact]
        public void GivenAvailableHome_WhenPurchaseFellThrough_ThenAppendsNothing()
        {
            var stream = _testBed.Available();

            PurchaseFellThroughHandler.Handle(FellThrough(), stream);

            stream.EventsAppended.Should().BeEmpty();
        }

        private PurchaseFellThrough FellThrough() =>
            new("purchase-1", _testBed.HomeId.Value, null, null, "Financing fell through", _testBed.Today.AddDays(30), "user-1");
    }
}
