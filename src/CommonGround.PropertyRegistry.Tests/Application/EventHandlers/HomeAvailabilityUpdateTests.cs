using CommonGround.PropertyRegistry.Application.EventHandlers;
using CommonGround.PropertyRegistry.Application.ReadModels;
using FluentAssertions;
using Wolverine.Marten;
using Xunit;

namespace CommonGround.PropertyRegistry.Tests.Application.EventHandlers;

public class HomeAvailabilityUpdateTests
{
    public class TheRecordMethod
    {
        [Fact]
        public void GivenNoAvailabilityYet_WhenRecorded_ThenInsertsANewOneForTheHome()
        {
            var op = HomeAvailabilityUpdate.Record("home-1", availability: null, view => view.RecordSelectionCancelled("selection-1"));

            var inserted = op.Should().BeOfType<InsertDoc<HomeAvailabilityView>>().Which.Document.Should().BeOfType<HomeAvailabilityView>().Subject;
            inserted.Id.Should().Be("home-1");
            inserted.Selections.Should().ContainSingle().Which.Cancelled.Should().BeTrue();
        }

        [Fact]
        public void GivenExistingAvailability_WhenRecorded_ThenUpdatesIt()
        {
            var existing = new HomeAvailabilityView { Id = "home-1" };

            var op = HomeAvailabilityUpdate.Record("home-1", existing, view => view.RecordLeased(new DateOnly(2026, 12, 1)));

            op.Should().BeOfType<UpdateDoc<HomeAvailabilityView>>().Which.Document.Should().BeSameAs(existing);
            existing.SaleStage.Should().Be(HomeSaleStage.NotForSale.DisplayName);
        }
    }
}
