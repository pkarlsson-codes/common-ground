using CommonGround.HomeBuyers.Application.EventHandlers;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.PropertyRegistry.Contracts.Events;
using FluentAssertions;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Tests.Application.EventHandlers;

public class HomeSaleStageChangedHandlerTests
{
    public class TheHandleMethod
    {
        [Fact]
        public void GivenTheHomesFacts_WhenALaterStageArrives_ThenStoresIt()
        {
            var facts = new HomeFactsView { Id = HandlerTestBed.HomeId, SaleStageSequence = 1 };

            var op = HomeSaleStageChangedHandler.Handle(
                new HomeSaleStageChanged(HandlerTestBed.HomeId, "UnderContract", new DateOnly(2026, 7, 20), 2, "selection-1"), facts);

            op.Should().BeOfType<UpdateDoc<HomeFactsView>>();
            facts.Should().BeEquivalentTo(new
            {
                SaleStage = "UnderContract",
                SaleStageSince = new DateOnly(2026, 7, 20),
                SaleStageSequence = 2,
                SaleStageSelectionId = "selection-1",
            });
        }

        [Fact]
        public void GivenALaterStageAlreadyStored_WhenAnEarlierOneArrivesLate_ThenKeepsTheLaterOne()
        {
            var facts = new HomeFactsView { Id = HandlerTestBed.HomeId, SaleStage = "UnderContract", SaleStageSequence = 3 };

            var op = HomeSaleStageChangedHandler.Handle(
                new HomeSaleStageChanged(HandlerTestBed.HomeId, "Offered", null, 2, null), facts);

            op.Should().NotBeOfType<UpdateDoc<HomeFactsView>>();
            facts.SaleStage.Should().Be("UnderContract");
        }

        [Fact]
        public void GivenNoFactsForTheHome_WhenAStageArrives_ThenThrows()
        {
            var act = () => HomeSaleStageChangedHandler.Handle(
                new HomeSaleStageChanged(HandlerTestBed.HomeId, "Offered", null, 1, null), null);

            act.Should().Throw<InvalidOperationException>().WithMessage($"*{HandlerTestBed.HomeId}*");
        }
    }
}
