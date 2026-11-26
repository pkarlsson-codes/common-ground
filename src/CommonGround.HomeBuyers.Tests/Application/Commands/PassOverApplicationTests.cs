using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using FluentAssertions;
using CommonGround.Testing;
using JasperFx.Events;

namespace CommonGround.HomeBuyers.Tests.Application.Commands;

public class PassOverApplicationTests
{
    public class TheHandleMethod
    {
        private readonly HandlerTestBed _testBed = new();

        [Fact]
        public void GivenApplicationTheSelectionMarkedPassedOver_WhenPassedOver_ThenRecordsThePassOver()
        {
            var application = new StubEventStream<HomeownerApplication>(_testBed.ApprovedApplication(_testBed.ReadyHousehold()));

            PassOverApplicationHandler.Handle(Command(application.Aggregate!, HomeSelectionId.Generate()), application);

            application.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<ApplicationPassedOver>();
        }

        [Fact]
        public void GivenHouseholdWhoseReadinessLapsedSinceTheChoice_WhenPassedOver_ThenStillRecordsThePassOver()
        {
            var application = new StubEventStream<HomeownerApplication>(_testBed.ApprovedApplication(_testBed.NewHousehold(ready: false)));

            PassOverApplicationHandler.Handle(Command(application.Aggregate!, HomeSelectionId.Generate()), application);

            application.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<ApplicationPassedOver>();
        }

        [Fact]
        public void GivenPassOverAlreadyRecorded_WhenPassedOverAgain_ThenRecordsNothing()
        {
            var selectionId = HomeSelectionId.Generate();
            var approved = _testBed.ApprovedApplication(_testBed.ReadyHousehold());
            var passedOver = approved.Evolve(approved.PassOver(selectionId, HandlerTestBed.HomeId)!)!;
            var application = new StubEventStream<HomeownerApplication>(passedOver);

            PassOverApplicationHandler.Handle(Command(passedOver, selectionId), application);

            application.EventsAppended.Should().BeEmpty();
        }

        private static PassOverApplication Command(HomeownerApplication application, HomeSelectionId selectionId) =>
            new(application.Id, selectionId, HandlerTestBed.HomeId);
    }
}
