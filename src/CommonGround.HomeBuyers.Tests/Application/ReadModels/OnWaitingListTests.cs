using CommonGround.HomeBuyers.Application.ReadModels;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Application.ReadModels;

public class OnWaitingListTests
{
    public class TheIsSatisfiedByMethod
    {
        [Theory]
        [InlineData("Submitted", false)]
        [InlineData("Approved", true)]
        [InlineData("Selected", false)]
        [InlineData("Denied", false)]
        [InlineData("Withdrawn", false)]
        [InlineData("Purchased", false)]
        public void GivenApplicationInPhase_ThenMatchesOnlyApproved(string phase, bool expected)
        {
            var view = new HomeownerApplicationListItemView { Phase = phase };

            OnWaitingList<HomeownerApplicationListItemView>.Instance.IsSatisfiedBy(view).Should().Be(expected);
        }
    }
}
