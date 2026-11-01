using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.PropertyRegistry.Application.EventHandlers;
using CommonGround.PropertyRegistry.Domain.Homes.Events;
using FluentAssertions;
using Xunit;

namespace CommonGround.PropertyRegistry.Tests.Application.EventHandlers;

public class HomeSelectionCancelledHandlerTests
{
    public class TheHandleMethod
    {
        private readonly HomeEventHandlerTestBed _testBed = new();

        [Fact]
        public void GivenHomeInSelection_WhenSelectionCancelled_ThenClosesTheSelection()
        {
            var stream = _testBed.InSelection();

            HomeSelectionCancelledHandler.Handle(new HomeSelectionCancelled("selection-1", _testBed.HomeId.Value), stream);

            stream.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<HomeSelectionClosedWithoutSale>();
        }

        [Fact]
        public void GivenLeasedHome_WhenSelectionCancelled_ThenAppendsNothing()
        {
            var stream = _testBed.Leased();

            HomeSelectionCancelledHandler.Handle(new HomeSelectionCancelled("selection-1", _testBed.HomeId.Value), stream);

            stream.EventsAppended.Should().BeEmpty();
        }
    }
}
