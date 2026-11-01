using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.Homeownership.Application.EventHandlers;
using CommonGround.Homeownership.Application.ReadModels;
using FluentAssertions;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Tests.Application.EventHandlers;

public class ApplicationSelectedHandlerTests
{
    public class TheHandleMethod
    {
        private static readonly ApplicationSelected Selected = new(
            "application-1", "household-1", "Alice Johnson", "alice@example.com", null,
            "home-1", "land-parcel-1", "groundleasetemplate-1", 250_000m, new DateOnly(2026, 6, 1), "user-1");

        [Fact]
        public void GivenNoOpenWorkflow_WhenApplicationSelected_ThenStartsAPurchaseWorkflow()
        {
            var op = ApplicationSelectedHandler.Handle(Selected, existingOpenWorkflow: null);

            op.Should().BeAssignableTo<IStartStream>();
        }

        [Fact]
        public void GivenOpenWorkflow_WhenApplicationSelectedAgain_ThenStartsNothing()
        {
            var op = ApplicationSelectedHandler.Handle(Selected, new PurchaseWorkflowView { Id = "purchase-1" });

            op.Should().NotBeAssignableTo<IStartStream>();
        }
    }
}
