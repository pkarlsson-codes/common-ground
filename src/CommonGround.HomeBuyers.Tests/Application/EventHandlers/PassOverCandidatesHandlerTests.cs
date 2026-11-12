using CommonGround.HomeBuyers.Application.EventHandlers;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Application.EventHandlers;

public class PassOverCandidatesHandlerTests
{
    public class TheHandleMethod
    {
        private readonly HandlerTestBed _testBed = new();

        [Fact]
        public void GivenSelectionWithThreeCandidates_WhenOneIsSelected_ThenPassesOverTheOtherTwo()
        {
            string[] applicationIds = [.. Enumerable.Range(0, 3).Select(_ => HomeownerApplicationId.Generate().Value)];
            var selection = new HomeSelectionView
            {
                Id = HomeSelectionId.Generate().Value,
                HomeId = HandlerTestBed.HomeId,
                Candidates = [.. applicationIds.Select(id => new HomeSelectionCandidateEntry { ApplicationId = id })],
            };
            var selected = new ApplicationSelectedForHome(
                applicationIds[1], "household-1", "Alice Johnson", "alice@example.com", null, selection.Id,
                HandlerTestBed.HomeId, "land-parcel-1", "template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser.Value);

            var commands = PassOverCandidatesHandler.Handle(selected, selection).ToList();

            commands.Select(c => c.ApplicationId.Value).Should().Equal(applicationIds[0], applicationIds[2]);
            commands.Should().AllSatisfy(c =>
            {
                c.SelectionId.Value.Should().Be(selection.Id);
                c.HomeId.Should().Be(HandlerTestBed.HomeId);
                c.SelectedDate.Should().Be(_testBed.SelectedDate);
            });
        }

        [Fact]
        public void GivenCandidateOverIncomeLimit_WhenAnotherIsSelected_ThenDoesNotPassItOver()
        {
            string[] applicationIds = [.. Enumerable.Range(0, 3).Select(_ => HomeownerApplicationId.Generate().Value)];
            var selection = new HomeSelectionView
            {
                Id = HomeSelectionId.Generate().Value,
                HomeId = HandlerTestBed.HomeId,
                Candidates = [.. applicationIds.Select((id, i) => new HomeSelectionCandidateEntry { ApplicationId = id, IsOverIncomeLimit = i == 2 })],
            };
            var selected = new ApplicationSelectedForHome(
                applicationIds[1], "household-1", "Alice Johnson", "alice@example.com", null, selection.Id,
                HandlerTestBed.HomeId, "land-parcel-1", "template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser.Value);

            var commands = PassOverCandidatesHandler.Handle(selected, selection).ToList();

            commands.Select(c => c.ApplicationId.Value).Should().Equal(applicationIds[0]);
        }
    }
}
