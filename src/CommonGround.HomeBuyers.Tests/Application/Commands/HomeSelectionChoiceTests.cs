using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Application.Commands;

public class HomeSelectionChoiceTests
{
    public class TheEnsureOfferedMethod
    {
        private static readonly DateOnly SelectedDate = new(2026, 7, 20);

        [Fact]
        public void GivenHomeNeverSold_WhenChecked_ThenAllowsTheChoice()
        {
            var act = () => HomeSelectionChoice.EnsureOffered(new HomeFactsView { Id = "home-1" }, SelectedDate);

            act.Should().NotThrow();
        }

        [Theory]
        [InlineData("UnderContract")]
        [InlineData("NotForSale")]
        [InlineData("AwaitingTrustDecision")]
        [InlineData("HeldByTrust")]
        public void GivenHomeNotOffered_WhenChecked_ThenThrowsHomeNotAvailable(string saleStage)
        {
            var act = () => HomeSelectionChoice.EnsureOffered(new HomeFactsView { Id = "home-1", SaleStage = saleStage }, SelectedDate);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.choose.home_not_available");
        }

        [Fact]
        public void GivenAChoicePropertyRegistryHasNotHeardOfYet_WhenChecked_ThenThrowsHomeNotAvailableUnderContract()
        {
            var home = new HomeFactsView { Id = "home-1", LatestSelectionId = "selection-1" };

            var act = () => HomeSelectionChoice.EnsureOffered(home, SelectedDate);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Args["saleStage"].Should().Be("UnderContract");
        }

        [Fact]
        public void GivenAPurchaseThatFellThroughAfterTheLatestChoice_WhenChecked_ThenAllowsTheChoice()
        {
            var home = new HomeFactsView
            {
                Id = "home-1",
                LatestSelectionId = "selection-1",
                SaleStageSelectionId = "selection-1",
                SaleStageSince = new DateOnly(2026, 7, 1),
            };

            var act = () => HomeSelectionChoice.EnsureOffered(home, SelectedDate);

            act.Should().NotThrow();
        }

        [Fact]
        public void GivenADateBeforeTheHomeBecameAvailable_WhenChecked_ThenThrowsSelectedDateBeforeAvailable()
        {
            var home = new HomeFactsView { Id = "home-1", SaleStageSince = SelectedDate.AddDays(1) };

            var act = () => HomeSelectionChoice.EnsureOffered(home, SelectedDate);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.home_selection.choose.selected_date_before_available");
        }
    }
}
