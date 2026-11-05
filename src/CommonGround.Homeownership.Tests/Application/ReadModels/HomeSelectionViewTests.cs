using CommonGround.Homeownership.Application.ReadModels;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Application.ReadModels;

public class HomeSelectionViewTests
{
    public class TheRecordStartedMethod
    {
        [Fact]
        public void GivenNoOtherNews_WhenStarted_ThenSelectionIsOpen()
        {
            var sut = new HomeSelectionView { Id = "homeselection-1", HomeId = "home-1" };

            sut.RecordStarted();

            sut.Open.Should().BeTrue();
        }

        [Fact]
        public void GivenNewsOfItsEndArrivedFirst_WhenStarted_ThenSelectionStaysClosed()
        {
            var sut = new HomeSelectionView { Id = "homeselection-1", HomeId = "home-1" };
            sut.RecordEnded();

            sut.RecordStarted();

            sut.Open.Should().BeFalse();
        }
    }

    public class TheRecordEndedMethod
    {
        [Fact]
        public void GivenOpenSelection_WhenEnded_ThenSelectionIsClosed()
        {
            var sut = new HomeSelectionView { Id = "homeselection-1", HomeId = "home-1" };
            sut.RecordStarted();

            sut.RecordEnded();

            sut.Open.Should().BeFalse();
        }
    }
}
