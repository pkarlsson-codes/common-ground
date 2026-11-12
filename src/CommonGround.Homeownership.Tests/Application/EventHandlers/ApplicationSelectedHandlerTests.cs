using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.Homeownership.Application.EventHandlers;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Tests.Application.EventHandlers;

public class ApplicationSelectedHandlerTests
{
    public class TheHandleMethod
    {
        private static readonly ApplicationSelected Selected = new(
            "application-1", "household-1", "Alice Johnson", "alice@example.com", null,
            "homeselection-1", "home-1", "land-parcel-1", "groundleasetemplate-1", 250_000m, new DateOnly(2026, 6, 1), "user-1",
            AnnualIncome: 55_000m,
            HouseholdSize: 2,
            IncomeLimitPercentOfAmi: 80,
            IncomeLimitsEffectiveFrom: new DateOnly(2026, 4, 1),
            IncomeLimits: [50_000m, 57_150m, 64_300m, 71_400m, 77_150m, 82_850m, 88_550m, 94_250m]);

        [Fact]
        public void GivenNoOpenWorkflow_WhenApplicationSelected_ThenStartsAPurchaseWorkflow()
        {
            var (purchase, _) = ApplicationSelectedHandler.Handle(Selected, existingOpenWorkflow: null, selection: null);

            purchase.Should().BeAssignableTo<IStartStream>();
        }

        [Fact]
        public void GivenALimitTheSelectionJudgedBy_WhenApplicationSelected_ThenThePurchaseKeepsItAndTheDeclaredIncome()
        {
            var (purchase, _) = ApplicationSelectedHandler.Handle(Selected, existingOpenWorkflow: null, selection: null);

            var initiated = purchase.Should().BeAssignableTo<IStartStream>().Which.Events.Single().Should().BeOfType<PurchaseWorkflowInitiated>().Subject;
            initiated.Should().BeEquivalentTo(new
            {
                DeclaredAnnualIncome = Money.Create(55_000m),
                DeclaredHouseholdSize = 2,
                IncomeLimitPercentOfAmi = 80,
                IncomeLimitsEffectiveFrom = new DateOnly(2026, 4, 1),
                IncomeLimits = Selected.IncomeLimits!.Select(Money.Create).ToList(),
            });
        }

        [Fact]
        public void GivenOpenWorkflow_WhenApplicationSelectedAgain_ThenStartsNothing()
        {
            var (purchase, _) = ApplicationSelectedHandler.Handle(Selected, new PurchaseWorkflowView { Id = "purchase-1" }, selection: null);

            purchase.Should().NotBeAssignableTo<IStartStream>();
        }

        [Fact]
        public void GivenOpenSelection_WhenApplicationSelected_ThenEndsTheSelection()
        {
            var selection = new HomeSelectionView { Id = "homeselection-1", HomeId = "home-1" };
            selection.RecordStarted();

            var (_, selectionOp) = ApplicationSelectedHandler.Handle(Selected, existingOpenWorkflow: null, selection);

            selectionOp.Should().BeOfType<UpdateDoc<HomeSelectionView>>();
            selection.Open.Should().BeFalse();
        }

        [Fact]
        public void GivenNoNewsOfTheSelectionYet_WhenApplicationSelected_ThenRecordsItEnded()
        {
            var (_, selectionOp) = ApplicationSelectedHandler.Handle(Selected, existingOpenWorkflow: null, selection: null);

            var inserted = selectionOp.Should().BeOfType<InsertDoc<HomeSelectionView>>().Which.Document.Should().BeOfType<HomeSelectionView>().Subject;
            inserted.Should().BeEquivalentTo(new { Id = "homeselection-1", HomeId = "home-1", Ended = true, Open = false });
        }
    }
}
