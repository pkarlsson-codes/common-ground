using CommonGround.HomeBuyers.Application.EventHandlers;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.Homeownership.Contracts.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.Testing;
using FluentAssertions;
using JasperFx.Events;

namespace CommonGround.HomeBuyers.Tests.Application.EventHandlers;

public class PurchaseFellThroughHandlerTests
{
    public class TheHandleMethod
    {
        private readonly HandlerTestBed _testBed = new();

        private DateOnly Today => _testBed.SelectedDate.AddDays(30);

        [Fact]
        public void GivenSelectedApplication_WhenPurchaseFellThrough_ThenReturnsTheHouseholdToTheWaitingList()
        {
            var household = _testBed.ReadyHousehold();
            var application = new StubEventStream<HomeownerApplication>(_testBed.SelectedApplication(household));

            PurchaseFellThroughHandler.Handle(FellThrough(application.Aggregate!.Id.Value), new(application, household, Today));

            application.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<ApplicationReturnedToWaitingList>();
        }

        [Fact]
        public void GivenTheHouseholdsIncomeChangedDuringThePurchase_WhenItFellThrough_ThenTheApplicationTakesItAndIsMarked()
        {
            var household = _testBed.ReadyHousehold();
            var application = new StubEventStream<HomeownerApplication>(_testBed.SelectedApplication(household));
            household = household.Evolve(household.UpdateCircumstances(
                HouseholdCircumstances.Create(false, Money.Create(58000m)), _testBed.StaffUser))!;

            PurchaseFellThroughHandler.Handle(FellThrough(application.Aggregate!.Id.Value), new(application, household, Today));

            application.EventsAppended.Should().HaveCount(2);
            application.EventsAppended[0].Should().BeOfType<ApplicationReturnedToWaitingList>();
            application.EventsAppended[1].Should().BeOfType<ApplicationDeclarationUpdated>()
                .Which.Should().BeEquivalentTo(new
                {
                    AnnualIncome = Money.Create(58000m),
                    QualificationToConfirm = new QualificationToConfirmMark(Today, Money.Create(65000m), 1),
                });
        }

        [Fact]
        public void GivenPurchaseStartedDirectly_WhenItFellThrough_ThenDoesNothing()
        {
            var act = () => PurchaseFellThroughHandler.Handle(FellThrough(applicationId: null), decision: null);

            act.Should().NotThrow();
        }

        private PurchaseFellThrough FellThrough(string? applicationId) =>
            new("purchase-1", HandlerTestBed.HomeId, applicationId, null, "Financing fell through", Today, _testBed.StaffUser.Value);
    }
}
