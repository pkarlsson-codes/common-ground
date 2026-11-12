using CommonGround.HomeBuyers.Domain.IncomeLimitSchedules;
using CommonGround.HomeBuyers.Domain.IncomeLimitSchedules.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Domain.IncomeLimitSchedules;

public class IncomeLimitScheduleTests
{
    public class TestBed
    {
        public UserId StaffUser { get; } = UserId.Generate();
        public IncomeLimitScheduleId ScheduleId { get; } = IncomeLimitScheduleId.Generate();
        public DateOnly EffectiveFrom { get; } = new(2026, 4, 1);

        public static IncomeLimitLevel Level(int percent, decimal fourPersonLimit) =>
            IncomeLimitLevel.Create(percent, [.. Enumerable.Range(1, 8).Select(size => Money.Create(fourPersonLimit + ((size - 4) * 5_000)))]);

        public IncomeLimitLevel Fifty { get; } = Level(50, 55_400);
        public IncomeLimitLevel Eighty { get; } = Level(80, 88_600);

        public IncomeLimitScheduleEntered Entered() =>
            IncomeLimitSchedule.Enter(ScheduleId, EffectiveFrom, [Eighty, Fifty], scheduleOnSameDate: null, StaffUser);

        public IncomeLimitSchedule EnteredSchedule() => EventReplay.Replay<IncomeLimitSchedule>(Entered());
    }

    public class TheEnterMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenLevelsOutOfOrder_WhenEntered_ThenHoldsThemLowestPercentFirst()
        {
            var sut = EventReplay.Replay<IncomeLimitSchedule>(_testBed.Entered());

            sut.Levels.Should().Equal(_testBed.Fifty, _testBed.Eighty);
        }

        [Fact]
        public void GivenNoLevels_WhenEntered_ThenThrowsLevelsRequired()
        {
            var act = () => IncomeLimitSchedule.Enter(_testBed.ScheduleId, _testBed.EffectiveFrom, [], null, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.income_limit_schedule.levels.required");
        }

        [Fact]
        public void GivenTwoLevelsWithTheSamePercent_WhenEntered_ThenThrowsDuplicatePercent()
        {
            var act = () => IncomeLimitSchedule.Enter(
                _testBed.ScheduleId, _testBed.EffectiveFrom, [_testBed.Eighty, TestBed.Level(80, 90_000)], null, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.income_limit_schedule.levels.duplicate_percent");
        }

        [Fact]
        public void GivenAnotherScheduleOnTheSameDate_WhenEntered_ThenThrowsEffectiveFromTaken()
        {
            var act = () => IncomeLimitSchedule.Enter(
                _testBed.ScheduleId, _testBed.EffectiveFrom, [_testBed.Eighty], IncomeLimitScheduleId.Generate(), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.income_limit_schedule.effective_from.taken");
        }
    }

    public class TheCorrectMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenNewDateAndLevels_WhenCorrected_ThenReplacesBoth()
        {
            var sut = _testBed.EnteredSchedule();
            var correctedDate = _testBed.EffectiveFrom.AddDays(14);
            var corrected80 = TestBed.Level(80, 89_000);

            var (corrected, _) = sut.Decide(s => s.Correct(correctedDate, [corrected80], null, _testBed.StaffUser));

            corrected.Should().BeEquivalentTo(new { EffectiveFrom = correctedDate, Levels = new[] { corrected80 } });
        }

        [Fact]
        public void GivenTheSameDateAndLevels_WhenCorrected_ThenDecidesNothing()
        {
            var sut = _testBed.EnteredSchedule();

            var corrected = sut.Correct(_testBed.EffectiveFrom, [_testBed.Eighty, _testBed.Fifty], null, _testBed.StaffUser);

            corrected.Should().BeNull();
        }

        [Fact]
        public void GivenItselfAsTheScheduleOnTheDate_WhenCorrected_ThenIsNotRefused()
        {
            var sut = _testBed.EnteredSchedule();

            var corrected = sut.Correct(_testBed.EffectiveFrom, [_testBed.Eighty], _testBed.ScheduleId, _testBed.StaffUser);

            corrected.Should().NotBeNull();
        }

        [Fact]
        public void GivenAnotherScheduleOnTheCorrectedDate_WhenCorrected_ThenThrowsEffectiveFromTaken()
        {
            var sut = _testBed.EnteredSchedule();

            var act = () => sut.Correct(_testBed.EffectiveFrom.AddYears(-1), [_testBed.Eighty], IncomeLimitScheduleId.Generate(), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.income_limit_schedule.effective_from.taken");
        }
    }

    public class TheLevelForMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenAPercentTheScheduleHas_WhenAsked_ThenReturnsThatLevel()
        {
            var sut = _testBed.EnteredSchedule();

            sut.LevelFor(80).Should().Be(_testBed.Eighty);
        }

        [Fact]
        public void GivenAPercentTheScheduleLacks_WhenAsked_ThenReturnsNull()
        {
            var sut = _testBed.EnteredSchedule();

            sut.LevelFor(60).Should().BeNull();
        }
    }
}
