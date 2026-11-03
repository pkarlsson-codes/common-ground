using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Application.WorkItems;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Application.WorkItems;

public class HomeownershipWorkItemsTests
{
    public class TestBed
    {
        public DateOnly Today { get; } = new(2026, 11, 2);
        public IanaTimeZone Zone { get; } = IanaTimeZone.Create("America/New_York");

        // 02:00 UTC on Oct 10 is still Oct 9 in New York.
        public DateTimeOffset StepEnteredAt { get; } = new(2026, 10, 10, 2, 0, 0, TimeSpan.Zero);

        public PurchaseWorkflowView Workflow(string phase) =>
            new() { Id = "purchase-1", HomeId = "home-1", Phase = phase, BuyerFullName = "Alice Johnson" };
    }

    public class TheForPurchaseMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenPurchaseWithEligibilityVerified_WhenDerived_ThenWaitsOnAndOffersTheOtherStepsSinceItLastMovedOn()
        {
            var workflow = _testBed.Workflow("InProgress");
            workflow.EligibilityDetails = new EligibilityDetailsEntry();

            var item = HomeownershipWorkItems.ForPurchase(workflow, _testBed.StepEnteredAt, "14 Alder Lane", _testBed.Today, _testBed.Zone);

            item.Should().BeEquivalentTo(new
            {
                Kind = HomeownershipWorkItems.PurchaseStepWaiting,
                Label = "14 Alder Lane",
                Detail = "Alice Johnson: waiting on financing, inspection, title, lease signing",
                DueDate = (DateOnly?)null,
                WaitingSince = new DateOnly(2026, 10, 9),
                Actions = new[]
                {
                    HomeownershipWorkItems.RecordFinancing, HomeownershipWorkItems.RecordInspection,
                    HomeownershipWorkItems.RecordTitle, HomeownershipWorkItems.RecordLeaseExecution,
                },
            }, o => o.WithStrictOrdering());
        }

        [Fact]
        public void GivenEveryStepRecorded_WhenDerived_ThenIsReadyToScheduleClosing()
        {
            var workflow = _testBed.Workflow("InProgress");
            workflow.EligibilityDetails = new EligibilityDetailsEntry();
            workflow.FinancingDetails = new FinancingDetailsEntry();
            workflow.InspectionResult = new InspectionResultEntry();
            workflow.TitleDetails = new TitleDetailsEntry();
            workflow.LeaseExecutionDetails = new LeaseExecutionDetailsEntry();

            var item = HomeownershipWorkItems.ForPurchase(workflow, _testBed.StepEnteredAt, "14 Alder Lane", _testBed.Today, _testBed.Zone);

            item.Should().BeEquivalentTo(new
            {
                Detail = "Alice Johnson: ready to schedule closing",
                Actions = new[] { HomeownershipWorkItems.ScheduleClosing },
            });
        }

        [Fact]
        public void GivenStalledPurchase_WhenDerived_ThenIsDueByItsResolutionTargetAndResumedByRecordingAStep()
        {
            var workflow = _testBed.Workflow("Stalled");
            workflow.StalledReason = "Lender paused underwriting";
            workflow.StalledDate = new DateOnly(2026, 10, 20);
            workflow.ResolutionTargetDate = new DateOnly(2026, 11, 20);

            var item = HomeownershipWorkItems.ForPurchase(workflow, _testBed.StepEnteredAt, "14 Alder Lane", _testBed.Today, _testBed.Zone);

            item.Should().BeEquivalentTo(new
            {
                Kind = HomeownershipWorkItems.PurchaseStalled,
                Detail = "Alice Johnson: stalled, Lender paused underwriting",
                DueDate = (DateOnly?)new DateOnly(2026, 11, 20),
                WaitingSince = new DateOnly(2026, 10, 20),
                Actions = new[]
                {
                    HomeownershipWorkItems.RecordEligibility, HomeownershipWorkItems.RecordFinancing,
                    HomeownershipWorkItems.RecordInspection, HomeownershipWorkItems.RecordTitle,
                    HomeownershipWorkItems.RecordLeaseExecution,
                },
            });
        }

        [Theory]
        [InlineData(14, HomeownershipWorkItems.ClosingScheduled)]
        [InlineData(15, HomeownershipWorkItems.PurchaseStepWaiting)]
        public void GivenClosingScheduledInDays_WhenDerived_ThenClosingComesUpOnlyWithinTheWindow(int days, string kind)
        {
            var workflow = _testBed.Workflow("ClosingScheduled");
            workflow.ScheduledClosingDate = _testBed.Today.AddDays(days);

            var item = HomeownershipWorkItems.ForPurchase(workflow, _testBed.StepEnteredAt, "14 Alder Lane", _testBed.Today, _testBed.Zone);

            item.Kind.Should().Be(kind);
        }

        [Fact]
        public void GivenClosingScheduled_WhenDerived_ThenOffersToRecordTheClosing()
        {
            var workflow = _testBed.Workflow("ClosingScheduled");
            workflow.ScheduledClosingDate = _testBed.Today.AddDays(3);

            var item = HomeownershipWorkItems.ForPurchase(workflow, _testBed.StepEnteredAt, "14 Alder Lane", _testBed.Today, _testBed.Zone);

            item.Actions.Should().Equal(HomeownershipWorkItems.RecordClosing);
        }
    }
}
