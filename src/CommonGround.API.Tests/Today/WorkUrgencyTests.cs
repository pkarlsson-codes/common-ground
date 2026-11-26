using CommonGround.API.Today;
using CommonGround.HomeBuyers.Application.WorkItems;
using CommonGround.Homeownership.Application.WorkItems;
using CommonGround.Reminders.Application.WorkItems;
using CommonGround.SharedKernel.Application.WorkItems;
using FluentAssertions;

namespace CommonGround.API.Tests.Today;

public class WorkUrgencyTests
{
    public class TestBed
    {
        public DateOnly Today { get; } = new(2026, 11, 2);

        public WorkItem Item(string label, DateOnly? due, DateOnly waitingSince) =>
            Item(label, "kind", due, waitingSince);

        public WorkItem Item(string label, string kind, DateOnly? due, DateOnly waitingSince) =>
            new(WorkArea.Applicants, kind, "record", label, label, "detail", due, waitingSince, []);
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

    public class TheOrderMethodWithKinds
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenStewardshipKinds_WhenOrdered_ThenTheyFollowTheKindOrderWhateverTheirDates()
        {
            var today = _testBed.Today;
            var old = new DateOnly(2020, 1, 1);
            var items = new[]
            {
                _testBed.Item("ending soon", HomeownershipWorkItems.InsuranceEndingSoon, old, old),
                _testBed.Item("statement", HomeownershipWorkItems.ResalePriceStatementDue, old, old),
                _testBed.Item("loan closing", HomeownershipWorkItems.MortgageLoanClosingUnrecorded, old, old),
                _testBed.Item("resale closing", HomeownershipWorkItems.ResaleClosingWaiting, old, old),
                _testBed.Item("fee notice", HomeownershipWorkItems.LeaseFeeNoticeDue, old, old),
                _testBed.Item("lapsed", HomeownershipWorkItems.InsuranceLapsed, old, old),
                _testBed.Item("short", HomeownershipWorkItems.InsuranceShortOfLease, null, old),
                _testBed.Item("improvement", HomeownershipWorkItems.ImprovementRequestWaiting, old, old),
                _testBed.Item("mortgage request", HomeownershipWorkItems.MortgageRequestPending, old, old),
                _testBed.Item("default", HomeownershipWorkItems.LeaseDefaultUncured, today.AddDays(5), today),
                _testBed.Item("resale decision", HomeownershipWorkItems.ResaleDecisionDue, today.AddDays(20), today),
            };

            WorkUrgency.Order(items).Select(i => i.Label).Should().Equal(
                "resale decision", "default", "mortgage request", "improvement", "short", "lapsed",
                "fee notice", "resale closing", "loan closing", "statement", "ending soon");
        }

        [Fact]
        public void GivenAReminderAndOverdueWork_WhenOrdered_ThenTheReminderComesFirst()
        {
            var today = _testBed.Today;
            var items = new[]
            {
                _testBed.Item("overdue default", HomeownershipWorkItems.LeaseDefaultUncured, today.AddDays(-30), today.AddDays(-60)),
                _testBed.Item("call the lender", RemindersWorkItems.Reminder, null, today),
            };

            WorkUrgency.Order(items).Select(i => i.Label).Should().Equal("call the lender", "overdue default");
        }

        [Fact]
        public void GivenTwoItemsOfOneKind_WhenOrdered_ThenTheEarlierDueDateComesFirst()
        {
            var today = _testBed.Today;
            var items = new[]
            {
                _testBed.Item("later", HomeownershipWorkItems.InsuranceLapsed, today, today),
                _testBed.Item("earlier", HomeownershipWorkItems.InsuranceLapsed, today.AddDays(-9), today),
            };

            WorkUrgency.Order(items).Select(i => i.Label).Should().Equal("earlier", "later");
        }

        [Fact]
        public void GivenAnApplicantsKindAndAnUnlistedKind_WhenOrdered_ThenTheUnlistedKindIsLast()
        {
            var today = _testBed.Today;
            var items = new[]
            {
                _testBed.Item("unlisted", "future-kind", today.AddDays(-100), today),
                _testBed.Item("application", HomeBuyersWorkItems.ApplicationAwaitingDecision, null, today),
            };

            WorkUrgency.Order(items).Select(i => i.Label).Should().Equal("application", "unlisted");
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
