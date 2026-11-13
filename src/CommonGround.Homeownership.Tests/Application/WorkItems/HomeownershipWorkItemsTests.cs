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
                Detail = "Alice Johnson: waiting on appraisal, financing, inspection, title, lease signing",
                DueDate = (DateOnly?)null,
                WaitingSince = new DateOnly(2026, 10, 9),
                Actions = new[]
                {
                    HomeownershipWorkItems.RecordAppraisal, HomeownershipWorkItems.RecordFinancing,
                    HomeownershipWorkItems.RecordInspection, HomeownershipWorkItems.RecordTitle,
                    HomeownershipWorkItems.RecordLeaseExecution,
                },
            }, o => o.WithStrictOrdering());
        }

        [Fact]
        public void GivenEveryStepRecorded_WhenDerived_ThenIsReadyToScheduleClosing()
        {
            var workflow = _testBed.Workflow("InProgress");
            workflow.EligibilityDetails = new EligibilityDetailsEntry();
            workflow.AppraisalDetails = new AppraisalDetailsEntry();
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
                    HomeownershipWorkItems.RecordEligibility, HomeownershipWorkItems.RecordAppraisal,
                    HomeownershipWorkItems.RecordFinancing, HomeownershipWorkItems.RecordInspection,
                    HomeownershipWorkItems.RecordTitle, HomeownershipWorkItems.RecordLeaseExecution,
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

    public class TheForMortgageRequestMethod
    {
        private readonly TestBed _testBed = new();

        private static MortgageRequestView Request(string status) => new()
        {
            Id = "mortgagerequest-1",
            GroundLeaseId = "groundlease-1",
            HomeId = "home-1",
            HomeownerFullName = "Dana Whitfield",
            Status = status,
            ReceivedOn = new DateOnly(2026, 10, 20),
            Lender = "Columbia Bank",
            Principal = 175_000m,
            ExpectedClosingOn = new DateOnly(2026, 11, 1),
        };

        [Fact]
        public void GivenPendingRequestWithoutAppraisal_WhenDerived_ThenWaitsSinceReceiptDueOnTheClosingAndAsksForTheAppraisal()
        {
            var item = HomeownershipWorkItems.ForMortgageRequest(Request("Pending"), "14 Alder Lane", _testBed.Today);

            item.Should().BeEquivalentTo(new
            {
                Kind = HomeownershipWorkItems.MortgageRequestPending,
                RecordType = "ground-lease",
                RecordId = "groundlease-1",
                Detail = "Dana Whitfield: mortgage request, $175,000 from Columbia Bank, waiting for the appraisal",
                DueDate = (DateOnly?)new DateOnly(2026, 11, 1),
                WaitingSince = new DateOnly(2026, 10, 20),
                Actions = new[] { HomeownershipWorkItems.RecordMortgageRequestAppraisal },
            });
        }

        [Fact]
        public void GivenPendingRequestWithAppraisal_WhenDerived_ThenAsksForTheDecision()
        {
            var request = Request("Pending");
            request.PurchaseOptionPrice = 220_000m;

            var item = HomeownershipWorkItems.ForMortgageRequest(request, null, _testBed.Today);

            item!.Actions.Should().Equal(HomeownershipWorkItems.DecideMortgageRequest);
        }

        [Fact]
        public void GivenApprovedLoanPastItsClosing_WhenDerived_ThenWaitsSinceTheDayAfterTheClosing()
        {
            var item = HomeownershipWorkItems.ForMortgageRequest(Request("Approved"), null, _testBed.Today);

            item.Should().BeEquivalentTo(new
            {
                Kind = HomeownershipWorkItems.MortgageLoanClosingUnrecorded,
                DueDate = (DateOnly?)null,
                WaitingSince = new DateOnly(2026, 11, 2),
                Actions = new[] { HomeownershipWorkItems.RecordMortgageLoanClosed },
            });
        }

        [Theory]
        [InlineData("Approved", "2026-11-02")]
        [InlineData("Closed", "2026-11-01")]
        [InlineData("Denied", "2026-11-01")]
        public void GivenRequestNeedingNothing_WhenDerived_ThenHasNoItem(string status, string expectedClosingOn)
        {
            var request = Request(status);
            request.ExpectedClosingOn = DateOnly.Parse(expectedClosingOn, System.Globalization.CultureInfo.InvariantCulture);

            HomeownershipWorkItems.ForMortgageRequest(request, null, _testBed.Today).Should().BeNull();
        }
    }
}
