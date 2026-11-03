using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Application.WorkItems;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;
using Xunit;

namespace CommonGround.PropertyRegistry.Tests.Application.WorkItems;

public class PropertyRegistryWorkItemsTests
{
    public class TestBed
    {
        public IanaTimeZone Zone { get; } = IanaTimeZone.Create("America/New_York");

        public HomeView Home { get; } = new()
        {
            Id = "home-1",
            RegisteredAt = new DateTimeOffset(2026, 9, 1, 2, 0, 0, TimeSpan.Zero),
        };
    }

    public class TheForHomeMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenNoAvailabilityYet_WhenDerived_ThenAvailableToStartASelectionSinceTheTrustDateItWasRegistered()
        {
            var item = PropertyRegistryWorkItems.ForHome(_testBed.Home, null, "14 Alder Lane", _testBed.Zone);

            item.Should().BeEquivalentTo(new
            {
                Kind = PropertyRegistryWorkItems.HomeAvailable,
                Label = "14 Alder Lane",
                WaitingSince = new DateOnly(2026, 8, 31),
                Actions = new[] { PropertyRegistryWorkItems.StartSelection },
            });
        }

        [Fact]
        public void GivenAvailableAgainAfterAPurchaseFellThrough_WhenDerived_ThenWaitsSinceThen()
        {
            var availability = new HomeAvailabilityView { Id = "home-1", AvailableAgainDate = new DateOnly(2026, 10, 15) };

            var item = PropertyRegistryWorkItems.ForHome(_testBed.Home, availability, "14 Alder Lane", _testBed.Zone);

            item!.WaitingSince.Should().Be(new DateOnly(2026, 10, 15));
        }

        [Fact]
        public void GivenHomeInSelection_WhenDerived_ThenNothingIsWaiting()
        {
            var availability = new HomeAvailabilityView { Id = "home-1", Status = HomeStatus.InSelection.DisplayName };

            var item = PropertyRegistryWorkItems.ForHome(_testBed.Home, availability, "14 Alder Lane", _testBed.Zone);

            item.Should().BeNull();
        }
    }
}
