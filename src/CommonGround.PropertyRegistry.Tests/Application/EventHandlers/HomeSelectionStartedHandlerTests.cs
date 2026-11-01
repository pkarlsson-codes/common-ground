using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.PropertyRegistry.Application.EventHandlers;
using CommonGround.PropertyRegistry.Domain.Homes.Events;
using FluentAssertions;
using Xunit;

namespace CommonGround.PropertyRegistry.Tests.Application.EventHandlers;

public class HomeSelectionStartedHandlerTests
{
    public class TheHandleMethod
    {
        private readonly HomeEventHandlerTestBed _testBed = new();

        [Fact]
        public void GivenAvailableHome_WhenSelectionStarted_ThenOpensSelectionOnThatDate()
        {
            var stream = _testBed.Available();

            HomeSelectionStartedHandler.Handle(Started(), stream, _testBed.Today);

            stream.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<HomeSelectionOpened>()
                .Which.OpenedDate.Should().Be(_testBed.Today);
        }

        [Fact]
        public void GivenHomeUnderContract_WhenSelectionStarted_ThenReleasesTheContractAndOpensSelection()
        {
            var stream = _testBed.UnderContract();

            HomeSelectionStartedHandler.Handle(Started(), stream, _testBed.Today);

            stream.EventsAppended.Should().SatisfyRespectively(
                e => e.Should().BeOfType<HomeContractReleased>(),
                e => e.Should().BeOfType<HomeSelectionOpened>());
        }

        [Fact]
        public void GivenLeasedHome_WhenSelectionStarted_ThenAppendsNothing()
        {
            var stream = _testBed.Leased();

            HomeSelectionStartedHandler.Handle(Started(), stream, _testBed.Today);

            stream.EventsAppended.Should().BeEmpty();
        }

        private HomeSelectionStarted Started() => new("selection-2", _testBed.HomeId.Value, DateTimeOffset.UtcNow);
    }
}
