using CommonGround.PropertyRegistry.Application.EventHandlers;
using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Contracts.Events;
using FluentAssertions;
using Wolverine.Marten;
using Xunit;

namespace CommonGround.PropertyRegistry.Tests.Application.EventHandlers;

public class HomeAvailabilityUpdateTests
{
    public class TheRecordMethod
    {
        private static readonly DateTimeOffset ChosenAt = new(2026, 7, 20, 15, 0, 0, TimeSpan.Zero);

        [Fact]
        public void GivenNoAvailabilityYet_WhenRecorded_ThenInsertsANewOneForTheHome()
        {
            var (op, _) = HomeAvailabilityUpdate.Record(
                "home-1", availability: null, view => view.RecordApplicationSelected("selection-1", "application-1", new DateOnly(2026, 7, 20), ChosenAt));

            var inserted = op.Should().BeOfType<InsertDoc<HomeAvailabilityView>>().Which.Document.Should().BeOfType<HomeAvailabilityView>().Subject;
            inserted.Id.Should().Be("home-1");
            inserted.Selections.Should().ContainSingle().Which.Id.Should().Be("selection-1");
        }

        [Fact]
        public void GivenExistingAvailability_WhenRecorded_ThenUpdatesIt()
        {
            var existing = new HomeAvailabilityView { Id = "home-1" };

            var (op, _) = HomeAvailabilityUpdate.Record("home-1", existing, view => view.RecordLeased(new DateOnly(2026, 12, 1)));

            op.Should().BeOfType<UpdateDoc<HomeAvailabilityView>>().Which.Document.Should().BeSameAs(existing);
            existing.SaleStage.Should().Be(HomeSaleStage.NotForSale.DisplayName);
        }

        [Fact]
        public void GivenNewsThatChangesTheStage_WhenRecorded_ThenPublishesTheNewStageWithTheNextSequence()
        {
            var existing = new HomeAvailabilityView { Id = "home-1", SaleStageSequence = 4 };

            var (_, messages) = HomeAvailabilityUpdate.Record(
                "home-1", existing, view => view.RecordApplicationSelected("selection-1", "application-1", new DateOnly(2026, 7, 20), ChosenAt));

            messages.Should().ContainSingle().Which.Should().BeEquivalentTo(
                new HomeSaleStageChanged("home-1", "UnderContract", new DateOnly(2026, 7, 20), 5, "selection-1"),
                options => options.Excluding(e => e.EventId).Excluding(e => e.OccurredAt));
            existing.SaleStageSequence.Should().Be(5);
        }

        [Fact]
        public void GivenNewsThatChangesNothingHomeBuyersUses_WhenRecorded_ThenPublishesNothing()
        {
            var existing = new HomeAvailabilityView { Id = "home-1", SaleStageSequence = 4 };

            var (_, messages) = HomeAvailabilityUpdate.Record("home-1", existing, view => view.RecordResaleWithdrawn("resale-1"));

            messages.Should().BeEmpty();
            existing.SaleStageSequence.Should().Be(4);
        }
    }
}
