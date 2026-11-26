using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Domain.HomeSelections;

public class WaitingListComparisonTests
{
    public class TheCreateMethod
    {
        private readonly SelectionTestBed _testBed = new();

        [Fact]
        public void GivenHomeWithNoIncomeLimit_WhenCompared_ThenEveryReadyHouseholdCanBeChosenInWaitingListOrder()
        {
            var sut = _testBed.Compare();

            sut.Applications.Select(a => a.Application.ApplicationId).Should().Equal(_testBed.A, _testBed.B, _testBed.C);
            sut.Applications.Should().AllSatisfy(a =>
            {
                a.IncomeLimit.Should().BeNull();
                a.CanBeChosen.Should().BeTrue();
            });
            sut.HasIncomeLimitsInEffect.Should().BeTrue();
        }

        [Fact]
        public void GivenHomeWithAnIncomeLimit_WhenCompared_ThenJudgesEachHouseholdAgainstTheLimitForItsSize()
        {
            var sut = _testBed.CompareWithIncomeLimit();

            sut.Applications.Select(a => a.IncomeLimit).Should().Equal(Money.Create(64000m), Money.Create(57000m), Money.Create(71000m));
            sut.Applications.Single(a => a.Application.ApplicationId == _testBed.B).Reasons
                .Should().Equal(CannotBeChosenReason.OverIncomeLimit);
            sut.Applications.Where(a => a.Application.ApplicationId != _testBed.B).Should().OnlyContain(a => a.CanBeChosen);
        }

        [Fact]
        public void GivenHouseholdMissingEveryReadinessStep_WhenCompared_ThenGivesEveryReason()
        {
            var notReady = SelectionTestBed.Ready("application-d", 40000m, 1, false, new DateOnly(2026, 6, 1)) with
            {
                HasPrimaryMember = false,
                IsHousingCounselingCompleted = false,
                IsCltOrientationCompleted = false,
                PrequalificationExpiryDate = null,
            };

            var sut = _testBed.Compare(waiting: [notReady]);

            sut.Applications.Single().Reasons.Should().Equal(
                CannotBeChosenReason.NoPrimaryMember,
                CannotBeChosenReason.HousingCounselingIncomplete,
                CannotBeChosenReason.CltOrientationIncomplete,
                CannotBeChosenReason.NoPrequalification);
        }

        [Fact]
        public void GivenPrequalificationExpiringOnTheDate_WhenCompared_ThenCanStillBeChosen()
        {
            var waiting = SelectionTestBed.Ready("application-d", 40000m, 1, false, new DateOnly(2026, 6, 1)) with
            {
                PrequalificationExpiryDate = _testBed.Today,
            };

            _testBed.Compare(waiting: [waiting]).Applications.Single().CanBeChosen.Should().BeTrue();
            _testBed.Compare(waiting: [waiting], asOf: _testBed.Today.AddDays(1)).Applications.Single().Reasons
                .Should().Equal(CannotBeChosenReason.PrequalificationExpired);
        }

        [Fact]
        public void GivenHomeWithALimitTheScheduleHasNoLevelFor_WhenCompared_ThenHasNoIncomeLimitsInEffect()
        {
            _testBed.Compare(incomeLimitPercentOfAmi: 60, limits: _testBed.Limits).HasIncomeLimitsInEffect.Should().BeFalse();
            _testBed.Compare(incomeLimitPercentOfAmi: 80, limits: null).HasIncomeLimitsInEffect.Should().BeFalse();
        }
    }
}
