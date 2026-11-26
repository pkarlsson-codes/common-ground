using CommonGround.HomeBuyers.Application.EventHandlers;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.PropertyRegistry.Contracts.Events;
using FluentAssertions;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Tests.Application.EventHandlers;

public class HomeIncomeLimitSetHandlerTests
{
    public class TheHandleMethod
    {
        [Fact]
        public void GivenTheHomesFacts_WhenTheLimitIsSet_ThenStoresTheNewLimit()
        {
            var facts = new HomeFactsView { Id = HandlerTestBed.HomeId, IncomeLimitPercentOfAmi = 80 };

            var op = HomeIncomeLimitSetHandler.Handle(new HomeIncomeLimitSet(HandlerTestBed.HomeId, 120), facts);

            op.Should().BeOfType<UpdateDoc<HomeFactsView>>();
            facts.IncomeLimitPercentOfAmi.Should().Be(120);
        }

        [Fact]
        public void GivenNoFactsForTheHome_WhenTheLimitIsSet_ThenThrows()
        {
            var act = () => HomeIncomeLimitSetHandler.Handle(new HomeIncomeLimitSet(HandlerTestBed.HomeId, 120), null);

            act.Should().Throw<InvalidOperationException>().WithMessage($"*{HandlerTestBed.HomeId}*");
        }
    }
}
