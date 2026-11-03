using CommonGround.API.Today;
using CommonGround.SharedKernel.Application.WorkItems;
using FluentAssertions;

namespace CommonGround.API.Tests.Today;

public class WorkUrgencyTests
{
    public class TestBed
    {
        public DateOnly Today { get; } = new(2026, 11, 2);

        public WorkItem Item(string label, DateOnly? due, DateOnly waitingSince) =>
            new(WorkArea.Applicants, "kind", "record", label, label, "detail", due, waitingSince, []);
    }

    public class TheOrderMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenOverdueDueAndWaitingItems_WhenOrdered_ThenOverdueFirstThenDueSoonestThenWaitingLongest()
        {
            var today = _testBed.Today;
            var items = new[]
            {
                _testBed.Item("waiting recently", null, today.AddDays(-1)),
                _testBed.Item("due later", today.AddDays(10), today),
                _testBed.Item("overdue", today.AddDays(-3), today.AddDays(-20)),
                _testBed.Item("waiting longest", null, today.AddDays(-40)),
                _testBed.Item("due soon", today.AddDays(2), today),
            };

            var ordered = WorkUrgency.Order(items);

            ordered.Select(i => i.Label).Should().Equal("overdue", "due soon", "due later", "waiting longest", "waiting recently");
        }
    }

    public class TheUrgencyOfMethod
    {
        private readonly TestBed _testBed = new();

        [Theory]
        [InlineData(-1, WorkUrgency.Overdue)]
        [InlineData(0, WorkUrgency.Soon)]
        [InlineData(7, WorkUrgency.Soon)]
        [InlineData(8, WorkUrgency.Plain)]
        public void GivenDueInDays_WhenAssessed_ThenOverdueBeforeTodayAndSoonWithinAWeek(int days, string expected)
        {
            var item = _testBed.Item("item", _testBed.Today.AddDays(days), _testBed.Today);

            WorkUrgency.UrgencyOf(item, _testBed.Today).Should().Be(expected);
        }

        [Fact]
        public void GivenNoDueDate_WhenAssessed_ThenPlain()
        {
            var item = _testBed.Item("item", null, _testBed.Today.AddDays(-100));

            WorkUrgency.UrgencyOf(item, _testBed.Today).Should().Be(WorkUrgency.Plain);
        }
    }
}
