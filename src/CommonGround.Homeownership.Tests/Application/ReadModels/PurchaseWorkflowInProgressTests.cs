using CommonGround.Homeownership.Application.ReadModels;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Application.ReadModels;

public class PurchaseWorkflowInProgressTests
{
    public class TheIsSatisfiedByMethod
    {
        [Theory]
        [InlineData("Initiated", true)]
        [InlineData("InProgress", true)]
        [InlineData("ClosingScheduled", true)]
        [InlineData("Stalled", true)]
        [InlineData("Closed", false)]
        [InlineData("Failed", false)]
        [InlineData("Withdrawn", false)]
        public void GivenWorkflowInPhase_ThenMatchesOnlyUnfinishedPhases(string phase, bool expected)
        {
            var view = new PurchaseWorkflowListItemView { Phase = phase };

            PurchaseWorkflowInProgress<PurchaseWorkflowListItemView>.Instance.IsSatisfiedBy(view).Should().Be(expected);
        }
    }
}
