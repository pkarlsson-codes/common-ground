using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using FluentAssertions;
using JasperFx.Events;

namespace CommonGround.HomeBuyers.Tests.Application.Commands;

public class PassOverApplicationTests
{
    public class TheHandleMethod
    {
        private readonly HandlerTestBed _testBed = new();

        [Fact]
        public void GivenCandidateWhoCouldHaveBeenSelected_WhenPassedOver_ThenRecordsThePassOver()
        {
            var (application, command, target) = Arrange(_testBed.ReadyHousehold());

            PassOverApplicationHandler.Handle(command, target);

            application.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<ApplicationPassedOver>();
        }

        [Fact]
        public void GivenCandidateWhoIsNotReady_WhenPassedOver_ThenRecordsNothing()
        {
            var (application, command, target) = Arrange(_testBed.NewHousehold(ready: false));

            PassOverApplicationHandler.Handle(command, target);

            application.EventsAppended.Should().BeEmpty();
        }

        private (StubEventStream<HomeownerApplication> Application, PassOverApplication Command, PassOverTarget Target) Arrange(Household household)
        {
            var application = new StubEventStream<HomeownerApplication>(_testBed.ApprovedApplication(household));
            var command = new PassOverApplication(
                application.Aggregate!.Id, HomeSelectionId.Generate(), HandlerTestBed.HomeId, _testBed.SelectedDate);
            return (application, command, new PassOverTarget(application, household));
        }
    }
}
