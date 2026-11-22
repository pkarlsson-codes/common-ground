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
        public void GivenNoAvailabilityYet_WhenDerived_ThenAvailableToStartASelectionWithNoWaitingDate()
        {
            var item = PropertyRegistryWorkItems.ForHome(_testBed.Home, null, "14 Alder Lane");

            item.Should().BeEquivalentTo(new
            {
                Kind = PropertyRegistryWorkItems.HomeAvailable,
                Label = "14 Alder Lane",
                WaitingSince = (DateOnly?)null,
                Actions = new[] { PropertyRegistryWorkItems.StartSelection },
            });
        }

        [Fact]
        public void GivenAvailableAgainAfterAPurchaseFellThrough_WhenDerived_ThenWaitsSinceThen()
        {
            var availability = new HomeAvailabilityView { Id = "home-1", AvailableAgainDate = new DateOnly(2026, 10, 15) };

            var item = PropertyRegistryWorkItems.ForHome(_testBed.Home, availability, "14 Alder Lane");

            item!.WaitingSince.Should().Be(new DateOnly(2026, 10, 15));
        }

        [Theory]
        [InlineData("NotForSale")]
        [InlineData("AwaitingTrustDecision")]
        [InlineData("InSelection")]
        [InlineData("UnderContract")]
        public void GivenHomeNotOffered_WhenDerived_ThenNothingIsWaiting(string saleStage)
        {
            var availability = new HomeAvailabilityView { Id = "home-1", SaleStage = saleStage };

            var item = PropertyRegistryWorkItems.ForHome(_testBed.Home, availability, "14 Alder Lane");

            item.Should().BeNull();
        }

        [Fact]
        public void GivenHomeOffered_WhenDerived_ThenAvailableToStartASelection()
        {
            var availability = new HomeAvailabilityView { Id = "home-1", SaleStage = HomeSaleStage.Offered.DisplayName };

            var item = PropertyRegistryWorkItems.ForHome(_testBed.Home, availability, "14 Alder Lane");

            item!.Kind.Should().Be(PropertyRegistryWorkItems.HomeAvailable);
        }
    }
}
