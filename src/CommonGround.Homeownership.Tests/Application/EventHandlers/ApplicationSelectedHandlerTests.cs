using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.Homeownership.Application.EventHandlers;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
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
            IncomeLimitLevels:
            [
                new SelectedIncomeLimitLevel(50, [31_650m, 36_200m, 40_700m, 45_200m, 48_850m, 52_450m, 56_100m, 59_700m]),
                new SelectedIncomeLimitLevel(80, [50_000m, 57_150m, 64_300m, 71_400m, 77_150m, 82_850m, 88_550m, 94_250m]),
            ]);

        [Fact]
        public void GivenNoOpenWorkflow_WhenApplicationSelected_ThenStartsAPurchaseWorkflow()
        {
            var purchase = ApplicationSelectedHandler.Handle(Selected, existingOpenWorkflow: null);

            purchase.Should().BeAssignableTo<IStartStream>();
        }

        [Fact]
        public void GivenALimitTheSelectionJudgedBy_WhenApplicationSelected_ThenThePurchaseKeepsTheWholeScheduleAndTheDeclaredIncome()
        {
            var purchase = ApplicationSelectedHandler.Handle(Selected, existingOpenWorkflow: null);

            var initiated = purchase.Should().BeAssignableTo<IStartStream>().Which.Events.Single().Should().BeOfType<PurchaseWorkflowInitiated>().Subject;
            initiated.Should().BeEquivalentTo(new
            {
                DeclaredAnnualIncome = Money.Create(55_000m),
                DeclaredHouseholdSize = 2,
                IncomeLimitPercentOfAmi = 80,
                IncomeLimitsEffectiveFrom = new DateOnly(2026, 4, 1),
                IncomeLimitLevels = Selected.IncomeLimitLevels!
                    .Select(level => new IncomeLimitLevelEntry(level.PercentOfAmi, [.. level.Limits.Select(Money.Create)]))
                    .ToList(),
            });
        }

        [Fact]
        public void GivenOpenWorkflow_WhenApplicationSelectedAgain_ThenStartsNothing()
        {
            var purchase = ApplicationSelectedHandler.Handle(Selected, new PurchaseWorkflowView { Id = "purchase-1" });

            purchase.Should().NotBeAssignableTo<IStartStream>();
        }
    }
}
