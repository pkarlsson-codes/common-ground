using CommonGround.Reminders.Domain.Reminders;
using CommonGround.Reminders.Domain.Reminders.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using FluentAssertions;

namespace CommonGround.Reminders.Tests.Domain.Reminders;

public class ReminderTests
{
    private static readonly DateOnly Today = new(2026, 11, 23);
    private static readonly UserId Staff = UserId.Generate();
    private static readonly ReminderId Id = ReminderId.Generate();

    private static ReminderSet SetFor(DateOnly remindOn, string note = "call the lender about the rate lock", string? section = "stewardship") =>
        Reminder.Set(Id, remindOn, note, section, Today, Staff);

    public class TheSetMethod
    {
        [Fact]
        public void GivenANoteAndAFutureDate_WhenSet_ThenRecordsTheTrimmedNoteSectionAndWhoSetIt()
        {
            var set = Reminder.Set(Id, Today.AddDays(3), "  call the lender about the rate lock ", "stewardship", Today, Staff);

            set.Should().BeEquivalentTo(new
            {
                AggregateId = Id.Value,
                RemindOn = Today.AddDays(3),
                Note = "call the lender about the rate lock",
                Section = "stewardship",
                SetBy = Staff.Value,
            });
        }

        [Fact]
        public void GivenTodaysDate_WhenSet_ThenIsAllowed()
        {
            SetFor(Today).RemindOn.Should().Be(Today);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void GivenNoSection_WhenSet_ThenHasNone(string? section)
        {
            SetFor(Today, section: section).Section.Should().BeNull();
        }

        [Fact]
        public void GivenADateBeforeToday_WhenSet_ThenIsRefused()
        {
            var act = () => SetFor(Today.AddDays(-1));

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("reminders.reminder.set.remind_on_in_past");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void GivenAnEmptyNote_WhenSet_ThenIsRefused(string note)
        {
            var act = () => SetFor(Today, note);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("reminders.reminder.set.note_required");
        }

        [Fact]
        public void GivenANoteLongerThanAReminderHolds_WhenSet_ThenIsRefused()
        {
            var act = () => SetFor(Today, new string('x', Reminder.NoteMaxLength + 1));

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("reminders.reminder.set.note_too_long");
        }

        [Theory]
        [InlineData("reminders")]
        [InlineData("resales")]
        public void GivenASectionTodayDoesNotHave_WhenSet_ThenIsRefused(string section)
        {
            var act = () => SetFor(Today, section: section);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("reminders.reminder.set.section_unknown");
        }
    }

    public class TheMarkDoneMethod
    {
        [Fact]
        public void GivenAReminderNotDone_WhenMarkedDone_ThenRecordsWhoMarkedIt()
        {
            var reminder = EventReplay.Replay<Reminder>(SetFor(Today));
            var colleague = UserId.Generate();

            var (after, events) = reminder.Decide(r => r.MarkDone(colleague));

            events.Should().ContainSingle().Which.Should().BeEquivalentTo(new ReminderMarkedDone(Id.Value, colleague.Value), o => o.Excluding(e => e.OccurredAt));
            after.IsDone.Should().BeTrue();
        }

        [Fact]
        public void GivenAReminderAlreadyDone_WhenMarkedDone_ThenNothingHappens()
        {
            var reminder = EventReplay.Replay<Reminder>(SetFor(Today), new ReminderMarkedDone(Id.Value, Staff.Value));

            reminder.MarkDone(UserId.Generate()).Should().BeNull();
        }
    }
}
