using CommonGround.SharedKernel.Domain;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;
using FluentAssertions;

namespace CommonGround.Stewardship.Tests.Domain.ForeclosurePreventionCases;

public class LeaseDefaultNoticeTests
{
    public class TheCreateMethod
    {
        private static readonly DateOnly SentOn = new(2026, 3, 1);
        private static readonly DateOnly CureBy = new(2026, 3, 31);

        [Theory]
        [InlineData(0)]
        [InlineData(451)]
        public void GivenStillToPayNotBetweenZeroAndTheAmountOwed_WhenCreated_ThenThrows(decimal stillToPay)
        {
            var act = () => LeaseDefaultNotice.Create(SentOn, Money.Create(450m), CureBy, Money.Create(stillToPay));

            act.Should().Throw<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("stillToPay");
        }

        [Fact]
        public void GivenCureByBeforeSentOn_WhenCreated_ThenThrows()
        {
            var act = () => LeaseDefaultNotice.Create(SentOn, Money.Create(450m), SentOn.AddDays(-1), Money.Create(450m));

            act.Should().Throw<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("cureBy");
        }
    }
}
