using CommonGround.Homeownership.Application.Queries;
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

        /// <summary>
        /// An active lease from Jan 1, 2025 requiring $280,000 replacement value and $300,000 liability, with the
        /// given certificates.
        /// </summary>
        public LeaseInsurance Insured(params InsuranceCertificateEntry[] certificates)
        {
            var lease = new GroundLeaseView
            {
                Id = "groundlease-1",
                HomeownerFullName = "Dana Whitfield",
                LeaseStartDate = new DateOnly(2025, 1, 1),
                InsuranceRequirements = [new() { EffectiveFrom = new DateOnly(2025, 1, 1), ReplacementValue = 280_000m, LiabilityPerOccurrence = 300_000m }],
                InsuranceCertificates = [.. certificates],
            };
            return new LeaseInsurance(lease, "14 Alder Lane", LeaseInsurance.CoverOf(lease));
        }

        public static InsuranceCertificateEntry Certificate(
            DateOnly policyEnd, decimal dwelling = 280_000m, decimal liability = 300_000m, bool trustNamed = true) =>
            new()
            {
                CertificateId = "insurancecertificate-1",
                PolicyStart = policyEnd.AddYears(-1),
                PolicyEnd = policyEnd,
                DwellingCoverage = dwelling,
                LiabilityPerOccurrence = liability,
                TrustNamedAdditionalInsured = trustNamed,
                ReceivedOn = new DateOnly(2026, 10, 20),
            };

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
                Detail = "Alice Johnson: waiting on appraisal, financing, inspection, title, lease signing, disclosure",
                DueDate = (DateOnly?)null,
                WaitingSince = new DateOnly(2026, 10, 9),
                Actions = new[]
                {
                    HomeownershipWorkItems.RecordAppraisal, HomeownershipWorkItems.RecordFinancing,
                    HomeownershipWorkItems.RecordInspection, HomeownershipWorkItems.RecordTitle,
                    HomeownershipWorkItems.RecordLeaseExecution, HomeownershipWorkItems.RecordDisclosure,
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
            workflow.DisclosureDetails = new DisclosureDetailsEntry();

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
                    HomeownershipWorkItems.RecordDisclosure,
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

    public class TheForImprovementRequestMethod
    {
        private static ImprovementRequestView Request(string status) => new()
        {
            Id = "improvementrequest-1",
            GroundLeaseId = "groundlease-1",
            HomeId = "home-1",
            HomeownerFullName = "Dana Whitfield",
            Status = status,
            ReceivedOn = new DateOnly(2026, 10, 20),
            ReplyDueOn = new DateOnly(2026, 11, 3),
            Description = "Replace the roof",
        };

        [Fact]
        public void GivenWaitingRequest_WhenDerived_ThenWaitsSinceReceiptDueTwoWeeksLaterAndAsksForTheDecision()
        {
            var item = HomeownershipWorkItems.ForImprovementRequest(Request("Waiting for decision"), "14 Alder Lane");

            item.Should().BeEquivalentTo(new
            {
                Kind = HomeownershipWorkItems.ImprovementRequestWaiting,
                RecordType = "ground-lease",
                RecordId = "groundlease-1",
                Detail = "Dana Whitfield: improvement request to decide, Replace the roof",
                DueDate = (DateOnly?)new DateOnly(2026, 11, 3),
                WaitingSince = new DateOnly(2026, 10, 20),
                Actions = new[] { HomeownershipWorkItems.DecideImprovementRequest },
            });
        }

        [Theory]
        [InlineData("Approved")]
        [InlineData("Denied")]
        [InlineData("Completed")]
        [InlineData("Withdrawn")]
        public void GivenRequestPastTheDecision_WhenDerived_ThenHasNoItem(string status)
        {
            HomeownershipWorkItems.ForImprovementRequest(Request(status), null).Should().BeNull();
        }
    }

    public class TheForResaleMethod
    {
        private static ResaleView Resale(string status = "Open", decimal? appraised = null, DateOnly? exercised = null) => new()
        {
            Id = "resale-1",
            HomeId = "home-1",
            HomeownerFullName = "Grace Whitaker",
            Status = status,
            NoticeDate = new DateOnly(2026, 10, 26),
            OptionWindowEndsOn = new DateOnly(2026, 11, 25),
            AppraisedValue = appraised,
            OptionExercisedDate = exercised,
        };

        [Fact]
        public void GivenOpenResaleWithNoAppraisal_WhenDerived_ThenWaitsSinceNoticeDueOnWindowEndAndOffersAppraisalAndBuy()
        {
            var item = HomeownershipWorkItems.ForResale(Resale(), "14 Alder Lane");

            item.Should().BeEquivalentTo(new
            {
                Kind = HomeownershipWorkItems.ResaleDecisionDue,
                RecordType = "resale",
                RecordId = "resale-1",
                Detail = "Grace Whitaker: purchase option to decide by Nov 25, 2026, no appraisal yet",
                DueDate = (DateOnly?)new DateOnly(2026, 11, 25),
                WaitingSince = new DateOnly(2026, 10, 26),
                Actions = new[] { HomeownershipWorkItems.RecordResaleAppraisal, HomeownershipWorkItems.ExercisePurchaseOption },
            });
        }

        [Fact]
        public void GivenOpenResaleWithAppraisal_WhenDerived_ThenOffersOnlyBuy()
        {
            var item = HomeownershipWorkItems.ForResale(Resale(appraised: 250_000m), null);

            item!.Actions.Should().Equal(HomeownershipWorkItems.ExercisePurchaseOption);
        }

        [Fact]
        public void GivenOptionExercised_WhenDerived_ThenWaitsSinceExercisedWithNoDueDateAndOffersTheClosing()
        {
            var item = HomeownershipWorkItems.ForResale(Resale(appraised: 250_000m, exercised: new DateOnly(2026, 11, 10)), null);

            item.Should().BeEquivalentTo(new
            {
                Kind = HomeownershipWorkItems.ResaleClosingWaiting,
                RecordType = "resale",
                RecordId = "resale-1",
                Detail = "Grace Whitaker: buyback closing to record, option exercised Nov 10, 2026",
                DueDate = (DateOnly?)null,
                WaitingSince = new DateOnly(2026, 11, 10),
                Actions = new[] { HomeownershipWorkItems.RecordBuybackClosing },
            });
        }

        [Theory]
        [InlineData("Withdrawn")]
        [InlineData("Closed")]
        public void GivenResaleThatIsNotOpen_WhenDerived_ThenHasNoItem(string status)
        {
            HomeownershipWorkItems.ForResale(Resale(status), null).Should().BeNull();
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

    public class TheForInsuranceCertificateDueMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenCoverEndingThisMonth_WhenDerived_ThenDueOnThePolicyEndAndWaitingFrom30DaysBefore()
        {
            var insurance = _testBed.Insured(TestBed.Certificate(new DateOnly(2026, 11, 20)));

            var item = HomeownershipWorkItems.ForInsuranceCertificateDue(insurance, _testBed.Today);

            item.Should().BeEquivalentTo(new
            {
                Kind = HomeownershipWorkItems.InsuranceCertificateDue,
                Label = "14 Alder Lane",
                Detail = "Dana Whitfield: policy ends Nov 20, 2026",
                DueDate = (DateOnly?)new DateOnly(2026, 11, 20),
                WaitingSince = new DateOnly(2026, 10, 21),
                Actions = new[] { HomeownershipWorkItems.RecordInsuranceCertificate },
            });
        }

        [Fact]
        public void GivenCoverThatEnded_WhenDerived_ThenSaysThePolicyEnded()
        {
            var insurance = _testBed.Insured(TestBed.Certificate(new DateOnly(2026, 10, 1)));

            HomeownershipWorkItems.ForInsuranceCertificateDue(insurance, _testBed.Today).Detail
                .Should().Be("Dana Whitfield: policy ended Oct 1, 2026");
        }

        [Fact]
        public void GivenNoCertificate_WhenDerived_ThenDueOnTheLeaseStart()
        {
            var item = HomeownershipWorkItems.ForInsuranceCertificateDue(_testBed.Insured(), _testBed.Today);

            item.Detail.Should().Be("Dana Whitfield: no certificate on file");
            item.DueDate.Should().Be(new DateOnly(2025, 1, 1));
        }
    }

    public class TheForInsuranceShortOfLeaseMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenACertificateShortOnEverything_WhenDerived_ThenNamesEachShortfallDueOnTheDayItWasReceived()
        {
            var insurance = _testBed.Insured(TestBed.Certificate(new DateOnly(2027, 10, 1), 250_000m, 100_000m, false));

            var item = HomeownershipWorkItems.ForInsuranceShortOfLease(insurance);

            item.Should().BeEquivalentTo(new
            {
                Kind = HomeownershipWorkItems.InsuranceShortOfLease,
                Detail = "Dana Whitfield: insurance short of the lease, dwelling $250,000.00 below the $280,000.00 replacement value; " +
                    "liability $100,000.00 below the $300,000.00 required; trust not named as additional insured",
                DueDate = (DateOnly?)new DateOnly(2026, 10, 20),
                WaitingSince = new DateOnly(2026, 10, 20),
                Actions = Array.Empty<string>(),
            });
        }

        [Fact]
        public void GivenACertificateThatMeetsTheLease_WhenDerived_ThenThrows()
        {
            var insurance = _testBed.Insured(TestBed.Certificate(new DateOnly(2027, 10, 1)));

            var act = () => HomeownershipWorkItems.ForInsuranceShortOfLease(insurance);

            act.Should().Throw<ArgumentException>();
        }
    }
}
