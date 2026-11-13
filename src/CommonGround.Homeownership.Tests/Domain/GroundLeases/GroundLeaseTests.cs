using System.Globalization;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates.Events;
using CommonGround.Homeownership.Domain.MortgageRequests;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases;

public class GroundLeaseTests
{
    public class TestBed
    {
        public const string ParcelId = "land-parcel-1234";
        public const string HomeId = "home-5678";

        public UserId StaffUser { get; } = UserId.Generate();

        public GroundLeaseId LeaseId { get; } = GroundLeaseId.Generate();

        public Homeowner Homeowner { get; } = Homeowner.From("Dana Whitfield", "dana@example.com", "(503) 555-0123");

        public static readonly DateOnly LeaseStart = new(2025, 1, 1);

        public LeaseFeeTerms Fees { get; } = LeaseFeeTerms.Create(Money.Create(45m), Money.Create(15m));

        /// <summary>$60 a month at 36.5% a year: 6 cents a day of interest on an unpaid month.</summary>
        public LeaseFeeTerms FeesWithInterest { get; } = LeaseFeeTerms.Create(Money.Create(45m), Money.Create(15m), 36.5m);

        public GroundLeaseCreated Established(
            ResalePolicyTerms? terms = null, LeaseFeeTerms? fees = null, LeaseFeeTracking? tracking = null) =>
            GroundLease.Establish(
                LeaseId, ParcelId, HomeId, Homeowner, LeaseStart, 99, 60, fees ?? Fees, MortgageDebtCap.Create(90), 200_000m, 250_000m, "Standard affordability covenant",
                terms ?? ResalePolicyTerms.AppraisalBased(0.25m), HomeCommitments.None, StaffUser, tracking);

        public GroundLease Lease(ResalePolicyTerms? terms = null, LeaseFeeTerms? fees = null, LeaseFeeTracking? tracking = null) =>
            EventReplay.Replay<GroundLease>(Established(terms, fees, tracking));

        /// <summary>Tracked from 2025-03-15 with $200 owed that day.</summary>
        public static LeaseFeeTracking TrackedFromMarch15 { get; } =
            LeaseFeeTracking.Create(new DateOnly(2025, 3, 15), Money.Create(200m), LeaseStart);

        public GroundLeaseTemplate ActiveTemplate(string covenantTerms, int termYears, ResalePolicyTerms terms) =>
            EventReplay.Replay<GroundLeaseTemplate>(
                GroundLeaseTemplate.Define(GroundLeaseTemplateId.Generate(), "Template", termYears, 45, covenantTerms, terms, Fees, MortgageDebtCap.Create(90), StaffUser));

        public LeaseFeePaymentRecorded Payment(GroundLease lease, decimal amount, DateOnly received) =>
            lease.RecordLeaseFeePayment(LeaseFeePaymentId.Generate(), received, Money.Create(amount), PaymentMethod.Check, null, received, StaffUser);

        public GroundLeaseTemplate InactiveTemplate()
        {
            var template = ActiveTemplate("Covenants", 50, ResalePolicyTerms.AppraisalBased(0.25m));
            return template.Evolve(new GroundLeaseTemplateArchived(template.Id.Value, StaffUser.Value))!;
        }

        public PurchaseWorkflow Workflow() =>
            EventReplay.Replay<PurchaseWorkflow>(PurchaseWorkflow.Initiate(
                PurchaseWorkflowId.Generate(), GroundLeaseTemplateId.Generate(), ParcelId, HomeId,
                Homeowner.From("Buyer Name", "buyer@example.com"), 250000m, StaffUser));

        public GroundLeaseCreatedFromPurchase FromPurchase(GroundLeaseTemplate template) =>
            GroundLease.CreateFromPurchase(LeaseId, template, Workflow(), new DateOnly(2026, 1, 1), PermittedMortgageId.Generate(), StaffUser);
    }

    public class TheEstablishMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenValidInputs_WhenEstablished_ThenStartsAnActiveLeaseForTheHomeownerWithNoTemplate()
        {
            var established = _testBed.Established();

            EventReplay.Replay<GroundLease>(established).Should().BeEquivalentTo(new
            {
                LandParcelId = TestBed.ParcelId,
                UnitId = TestBed.HomeId,
                LeaseStartDate = new DateOnly(2025, 1, 1),
                TermYears = 99,
                OptionWindowDays = 60,
                Status = GroundLeaseStatus.Active,
                CurrentHomeowner = _testBed.Homeowner,
                TemplateId = (GroundLeaseTemplateId?)null,
                PurchaseWorkflowId = (PurchaseWorkflowId?)null,
                CreatedBy = _testBed.StaffUser,
                LeaseFeeTracking = LeaseFeeTracking.FromLeaseStart(new DateOnly(2025, 1, 1)),
                MortgageDebtCap = MortgageDebtCap.Create(90),
                PermittedMortgages = Array.Empty<PermittedMortgage>(),
            });
        }

        [Fact]
        public void GivenTracking_WhenEstablished_ThenTheLeaseKeepsIt()
        {
            var sut = _testBed.Lease(tracking: TestBed.TrackedFromMarch15);

            sut.LeaseFeeTracking.Should().Be(TestBed.TrackedFromMarch15);
        }

        [Fact]
        public void GivenTrackingFromBeforeTheLeaseStart_WhenEstablished_ThenThrowsBusinessRuleViolation()
        {
            var tracking = LeaseFeeTracking.Create(new DateOnly(2024, 12, 1), Money.Zero, new DateOnly(2024, 1, 1));

            var act = () => _testBed.Established(tracking: tracking);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.lease_fee_tracking.create.tracked_from_before_lease_start");
        }

        [Theory]
        [InlineData("groundlease-1", null, null, "homeownership.ground_lease.create.home_has_active_lease")]
        [InlineData(null, "purchaseworkflow-1", null, "homeownership.ground_lease.create.home_has_open_purchase")]
        [InlineData(null, null, "homeselection-1", "homeownership.ground_lease.create.home_in_selection")]
        public void GivenHomeAlreadyCommitted_WhenEstablished_ThenThrowsBusinessRuleViolation(
            string? activeLeaseId, string? openPurchaseWorkflowId, string? openHomeSelectionId, string expectedCode)
        {
            var act = () => GroundLease.Establish(
                _testBed.LeaseId, TestBed.ParcelId, TestBed.HomeId, _testBed.Homeowner, new DateOnly(2025, 1, 1),
                99, 60, LeaseFeeTerms.None, MortgageDebtCap.Create(90), 200_000m, 250_000m, "Terms", ResalePolicyTerms.AppraisalBased(0.25m),
                HomeCommitments.Create(activeLeaseId, openPurchaseWorkflowId, openHomeSelectionId), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Theory]
        [InlineData(0, 60, "Terms", "homeownership.ground_lease.create.term_years_non_positive")]
        [InlineData(99, 0, "Terms", "homeownership.ground_lease.create.option_window_days_non_positive")]
        [InlineData(99, 60, "", "homeownership.ground_lease.create.covenant_terms_required")]
        [InlineData(99, 60, "Terms", "homeownership.ground_lease.create.base_price_non_positive", 0, 250_000)]
        [InlineData(99, 60, "Terms", "homeownership.ground_lease.create.original_appraised_value_non_positive", 200_000, 0)]
        public void GivenInvalidInputs_WhenEstablished_ThenThrowsBusinessRuleViolation(
            int termYears, int optionWindowDays, string covenantTerms, string expectedCode, int basePrice = 200_000, int originalAppraisedValue = 250_000)
        {
            var act = () => GroundLease.Establish(
                _testBed.LeaseId, TestBed.ParcelId, TestBed.HomeId, _testBed.Homeowner, new DateOnly(2025, 1, 1),
                termYears, optionWindowDays, LeaseFeeTerms.None, MortgageDebtCap.Create(90), basePrice, originalAppraisedValue, covenantTerms, ResalePolicyTerms.AppraisalBased(0.25m), HomeCommitments.None, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GivenAppraisalTerms_WhenEstablished_ThenRoundTripsTheImprovementsCap(bool withCap)
        {
            var terms = ResalePolicyTerms.AppraisalBased(0.25m, withCap ? 12_000m : null);

            var sut = _testBed.Lease(terms);

            sut.ResalePolicyTerms.Should().BeEquivalentTo(new
            {
                FormulaKind = FormulaKind.AppraisalBased,
                AppreciationShare = 0.25m,
                ImprovementsCap = withCap ? 12_000m : (decimal?)null,
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GivenIndexTerms_WhenEstablished_ThenRoundTripsTheImprovementsCap(bool withCap)
        {
            var terms = ResalePolicyTerms.IndexBased("CPI", 100m, withCap ? 18_500m : null);

            var sut = _testBed.Lease(terms);

            sut.ResalePolicyTerms.Should().BeEquivalentTo(new
            {
                FormulaKind = FormulaKind.IndexBased,
                IndexType = "CPI",
                BaselineIndexValue = 100m,
                ImprovementsCap = withCap ? 18_500m : (decimal?)null,
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GivenFixedTerms_WhenEstablished_ThenRoundTripsTheImprovementsCap(bool withCap)
        {
            var terms = ResalePolicyTerms.FixedPercentage(0.03m, withCap ? 9_900m : null);

            var sut = _testBed.Lease(terms);

            sut.ResalePolicyTerms.Should().BeEquivalentTo(new
            {
                FormulaKind = FormulaKind.FixedPercentage,
                AnnualPercentage = 0.03m,
                ImprovementsCap = withCap ? 9_900m : (decimal?)null,
            });
        }
    }

    public class TheRecordStewardshipCheckInMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenNotes_WhenRecorded_ThenRecordsTheCheckIn()
        {
            var sut = _testBed.Lease();
            var checkInDate = new DateOnly(2025, 6, 15);

            var recorded = sut.RecordStewardshipCheckIn(checkInDate, "Property in good condition", _testBed.StaffUser);

            recorded.Should().BeEquivalentTo(new { CheckInDate = checkInDate, Notes = "Property in good condition" });
        }

        [Fact]
        public void GivenEmptyNotes_WhenRecorded_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Lease();

            var act = () => sut.RecordStewardshipCheckIn(new DateOnly(2025, 6, 15), "", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>();
        }
    }

    public class TheCreateFromPurchaseMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenActiveTemplate_WhenCreated_ThenTakesTheTemplatesTermsAndTheBuyerAsHomeowner()
        {
            var template = _testBed.ActiveTemplate("Template covenant terms", 99, ResalePolicyTerms.IndexBased("CPI", 100m, 7500m));

            var created = _testBed.FromPurchase(template);

            EventReplay.Replay<GroundLease>(created).Should().BeEquivalentTo(new
            {
                TemplateId = template.Id,
                TermYears = 99,
                OptionWindowDays = 45,
                CovenantTerms = "Template covenant terms",
                ResalePolicyTerms = new { FormulaKind = FormulaKind.IndexBased, IndexType = "CPI" },
                CurrentHomeowner = new { FullName = new { Value = "Buyer Name" }, Email = new { Value = "buyer@example.com" } },
            });
        }

        [Fact]
        public void GivenInactiveTemplate_WhenCreated_ThenThrowsBusinessRuleViolation()
        {
            var template = _testBed.InactiveTemplate();

            var act = () => _testBed.FromPurchase(template);

            act.Should().Throw<BusinessRuleViolationException>();
        }

        [Fact]
        public void GivenFixedTemplate_WhenCreated_ThenCopiesTheTermsIntoTheEvent()
        {
            var template = _testBed.ActiveTemplate("Template covenants", 40, ResalePolicyTerms.FixedPercentage(0.02m, 12000m));

            var created = _testBed.FromPurchase(template);

            created.Should().BeEquivalentTo(new
            {
                GroundLeaseTemplateId = template.Id.Value,
                TermYears = 40,
                CovenantTerms = "Template covenants",
                FormulaKind = FormulaKind.FixedPercentage,
                AnnualPercentage = 0.02m,
                ImprovementsCap = 12000m,
            });
        }

        [Theory]
        [MemberData(nameof(TemplateTerms))]
        public void GivenTemplateTerms_WhenCreated_ThenRoundTripsThem(ResalePolicyTerms terms)
        {
            var template = _testBed.ActiveTemplate("Covenants", 60, terms);

            var created = _testBed.FromPurchase(template);

            EventReplay.Replay<GroundLease>(created).ResalePolicyTerms.Should().BeEquivalentTo(terms);
        }

        [Fact]
        public void GivenFinancingCommitted_WhenCreated_ThenThePurchaseLoanIsTheFirstPermittedMortgage()
        {
            var template = _testBed.ActiveTemplate("Covenants", 99, ResalePolicyTerms.AppraisalBased(0.25m));
            var workflow = _testBed.Workflow()
                .Decide(w => w.RecordFinancingCommitted("First Community Bank", 180_000m, new DateOnly(2025, 11, 1), _testBed.StaffUser))
                .Aggregate;
            var mortgageId = PermittedMortgageId.Generate();

            var created = GroundLease.CreateFromPurchase(
                _testBed.LeaseId, template, workflow, new DateOnly(2026, 1, 1), mortgageId, _testBed.StaffUser);

            EventReplay.Replay<GroundLease>(created).Should().BeEquivalentTo(new
            {
                MortgageDebtCap = MortgageDebtCap.Create(90),
                PermittedMortgages = new[]
                {
                    new
                    {
                        Id = mortgageId,
                        Lender = "First Community Bank",
                        NoticeAddress = (string?)null,
                        OriginalPrincipal = Money.Create(180_000m),
                        PermittedOn = new DateOnly(2026, 1, 1),
                        Source = MortgagePermissionSource.LeaseSigning,
                        PaidOffOn = (DateOnly?)null,
                    },
                },
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void GivenNoLoan_WhenCreated_ThenNoMortgageIsPermitted(bool zeroCommitment)
        {
            var template = _testBed.ActiveTemplate("Covenants", 99, ResalePolicyTerms.AppraisalBased(0.25m));
            var workflow = zeroCommitment
                ? _testBed.Workflow().Decide(w => w.RecordFinancingCommitted("Cash", 0m, new DateOnly(2025, 11, 1), _testBed.StaffUser)).Aggregate
                : _testBed.Workflow();

            var created = GroundLease.CreateFromPurchase(
                _testBed.LeaseId, template, workflow, new DateOnly(2026, 1, 1), PermittedMortgageId.Generate(), _testBed.StaffUser);

            EventReplay.Replay<GroundLease>(created).PermittedMortgages.Should().BeEmpty();
        }

        public static TheoryData<ResalePolicyTerms> TemplateTerms() => new()
        {
            ResalePolicyTerms.AppraisalBased(0.33m, 15000m),
            ResalePolicyTerms.IndexBased("CPI", 210m, null),
            ResalePolicyTerms.FixedPercentage(0.015m, 5000m),
        };
    }

    public class TheStartResaleMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenActiveLeaseWithNoResale_WhenStarted_ThenTheLeaseHoldsTheOpenResale()
        {
            var sut = _testBed.Lease();
            var resaleId = ResaleId.Generate();

            var (started, _) = sut.Decide(l => l.StartResale(resaleId));

            started.OpenResaleId.Should().Be(resaleId);
        }

        [Fact]
        public void GivenOpenResale_WhenStarted_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Lease().Evolve(new ResaleStarted(_testBed.LeaseId.Value, ResaleId.Generate().Value))!;

            var act = () => sut.StartResale(ResaleId.Generate());

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.start_resale.resale_already_open");
        }

        [Fact]
        public void GivenClosedResale_WhenStarted_ThenStartsTheNewOne()
        {
            var closedId = ResaleId.Generate();
            var sut = _testBed.Lease().Evolve(
                new ResaleStarted(_testBed.LeaseId.Value, closedId.Value),
                new ResaleClosed(_testBed.LeaseId.Value, closedId.Value))!;
            var resaleId = ResaleId.Generate();

            var started = sut.StartResale(resaleId);

            started.ResaleId.Should().Be(resaleId.Value);
        }

        [Fact]
        public void GivenTerminatedLease_WhenStarted_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Lease().Evolve(
                new GroundLeaseTerminated(_testBed.LeaseId.Value, "Moved", new DateOnly(2026, 1, 1), _testBed.StaffUser.Value))!;

            var act = () => sut.StartResale(ResaleId.Generate());

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.start_resale.status_not_active");
        }
    }

    public class TheCloseResaleMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenTheOpenResale_WhenClosed_ThenTheLeaseHasNoOpenResale()
        {
            var resaleId = ResaleId.Generate();
            var sut = _testBed.Lease().Evolve(new ResaleStarted(_testBed.LeaseId.Value, resaleId.Value))!;

            var (closed, _) = sut.Decide(l => l.CloseResale(resaleId));

            closed.OpenResaleId.Should().BeNull();
        }

        [Fact]
        public void GivenAnotherResale_WhenClosed_ThenDecidesNothing()
        {
            var sut = _testBed.Lease().Evolve(new ResaleStarted(_testBed.LeaseId.Value, ResaleId.Generate().Value))!;

            var closed = sut.CloseResale(ResaleId.Generate());

            closed.Should().BeNull();
        }
    }

    public class TheRecordLeaseFeePaymentMethod
    {
        private readonly TestBed _testBed = new();

        [Theory]
        [InlineData(0, "2025-03-01", "homeownership.ground_lease.record_lease_fee_payment.amount_non_positive")]
        [InlineData(60, "2025-03-11", "homeownership.ground_lease.record_lease_fee_payment.received_date_in_future")]
        [InlineData(60, "2024-12-31", "homeownership.ground_lease.record_lease_fee_payment.received_date_before_fees_tracked")]
        public void GivenInvalidPayment_WhenRecorded_ThenThrowsBusinessRuleViolation(int amount, string received, string expectedCode)
        {
            var sut = _testBed.Lease();

            var act = () => sut.RecordLeaseFeePayment(
                LeaseFeePaymentId.Generate(), DateOnly.Parse(received, CultureInfo.InvariantCulture), Money.Create(amount), PaymentMethod.Check, null,
                new DateOnly(2025, 3, 10), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Fact]
        public void GivenTrackedLease_WhenPaymentReceivedBeforeTracking_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Lease(tracking: TestBed.TrackedFromMarch15);

            var act = () => sut.RecordLeaseFeePayment(
                LeaseFeePaymentId.Generate(), new DateOnly(2025, 3, 14), Money.Create(60m), PaymentMethod.Check, null,
                new DateOnly(2025, 4, 10), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.record_lease_fee_payment.received_date_before_fees_tracked");
        }

        [Fact]
        public void GivenTrackedLease_WhenStatementWorkedOut_ThenItOwesTheOpeningBalanceAndMonthsSinceTracking()
        {
            var sut = _testBed.Lease(tracking: TestBed.TrackedFromMarch15);

            sut.LeaseFeeStatementAsOf(new DateOnly(2025, 4, 10)).Should().BeEquivalentTo(new
            {
                Balance = Money.Create(260m),
                OldestUnpaidDueDate = (DateOnly?)new DateOnly(2025, 3, 15),
            });
        }

        [Fact]
        public void GivenPayment_WhenRecorded_ThenItCountsTowardTheBalance()
        {
            var sut = _testBed.Lease();

            var (paid, _) = sut.Decide(l => _testBed.Payment(l, 60m, new DateOnly(2025, 1, 3)));

            paid.LeaseFeeStatementAsOf(new DateOnly(2025, 2, 10)).Should().BeEquivalentTo(new
            {
                Balance = Money.Create(60m),
                OldestUnpaidDueDate = (DateOnly?)new DateOnly(2025, 2, 1),
            });
        }

        [Fact]
        public void GivenEndedLease_WhenRecorded_ThenStillTakesThePaymentSoArrearsCanBeCollected()
        {
            var sut = _testBed.Lease().Decide(l => l.Terminate("Sold", new DateOnly(2025, 2, 15), _testBed.StaffUser)).Aggregate;

            var (paid, _) = sut.Decide(l => _testBed.Payment(l, 120m, new DateOnly(2025, 6, 1)));

            paid.LeaseFeeStatementAsOf(new DateOnly(2025, 6, 1)).Balance.Amount.Should().Be(0m);
        }

        [Fact]
        public void GivenBlankReference_WhenRecorded_ThenRecordsNoReference()
        {
            var sut = _testBed.Lease();

            var recorded = sut.RecordLeaseFeePayment(
                LeaseFeePaymentId.Generate(), new DateOnly(2025, 1, 3), Money.Create(60m), PaymentMethod.Cash, "  ", new DateOnly(2025, 1, 3), _testBed.StaffUser);

            recorded.Reference.Should().BeNull();
        }
    }

    public class TheVoidLeaseFeePaymentMethod
    {
        private readonly TestBed _testBed = new();

        public (GroundLease Lease, LeaseFeePaymentId PaymentId) LeaseWithPayment()
        {
            var (lease, events) = _testBed.Lease().Decide(l => _testBed.Payment(l, 60m, new DateOnly(2025, 1, 3)));
            return (lease, LeaseFeePaymentId.Create(((LeaseFeePaymentRecorded)events.Single()).PaymentId));
        }

        [Fact]
        public void GivenPayment_WhenVoided_ThenItNoLongerCountsTowardTheBalance()
        {
            var (sut, paymentId) = LeaseWithPayment();

            var (voided, _) = sut.Decide(l => l.VoidLeaseFeePayment(paymentId, "Check bounced", _testBed.StaffUser));

            voided.LeaseFeeStatementAsOf(new DateOnly(2025, 1, 10)).Balance.Amount.Should().Be(60m);
        }

        [Fact]
        public void GivenNoReason_WhenVoided_ThenThrowsBusinessRuleViolation()
        {
            var (sut, paymentId) = LeaseWithPayment();

            var act = () => sut.VoidLeaseFeePayment(paymentId, " ", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.void_lease_fee_payment.reason_required");
        }

        [Fact]
        public void GivenPaymentNotOnTheLease_WhenVoided_ThenThrowsBusinessRuleViolation()
        {
            var (sut, _) = LeaseWithPayment();

            var act = () => sut.VoidLeaseFeePayment(LeaseFeePaymentId.Generate(), "Wrong lease", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.void_lease_fee_payment.payment_not_found");
        }

        [Fact]
        public void GivenVoidedPayment_WhenVoidedAgain_ThenThrowsBusinessRuleViolation()
        {
            var (lease, paymentId) = LeaseWithPayment();
            var sut = lease.Decide(l => l.VoidLeaseFeePayment(paymentId, "Check bounced", _testBed.StaffUser)).Aggregate;

            var act = () => sut.VoidLeaseFeePayment(paymentId, "Again", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.void_lease_fee_payment.already_voided");
        }
    }

    public class TheCorrectLeaseFeeTrackingMethod
    {
        private readonly TestBed _testBed = new();

        private static readonly DateOnly March15 = new(2025, 3, 15);

        [Fact]
        public void GivenLeaseTrackedFromItsStart_WhenCorrected_ThenItOwesTheOpeningBalanceAndMonthsSinceTheNewDay()
        {
            var sut = _testBed.Lease();

            var (corrected, _) = sut.Decide(l => l.CorrectLeaseFeeTracking(March15, Money.Create(200m), _testBed.StaffUser));

            corrected.LeaseFeeStatementAsOf(new DateOnly(2025, 4, 10)).Should().BeEquivalentTo(new
            {
                Balance = Money.Create(260m),
                OldestUnpaidDueDate = (DateOnly?)March15,
            });
        }

        [Theory]
        [InlineData("2024-12-31", 0, "homeownership.lease_fee_tracking.create.tracked_from_before_lease_start")]
        [InlineData("2025-03-15", -1, "homeownership.lease_fee_tracking.create.opening_balance_negative")]
        public void GivenInvalidTracking_WhenCorrected_ThenThrowsBusinessRuleViolation(string trackedFrom, int openingBalance, string expectedCode)
        {
            var sut = _testBed.Lease();

            var act = () => sut.CorrectLeaseFeeTracking(DateOnly.Parse(trackedFrom, CultureInfo.InvariantCulture), Money.Create(openingBalance), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Fact]
        public void GivenPaymentBeforeTheNewDay_WhenCorrected_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Lease().Decide(l => _testBed.Payment(l, 60m, new DateOnly(2025, 2, 3))).Aggregate;

            var act = () => sut.CorrectLeaseFeeTracking(March15, Money.Zero, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.correct_lease_fee_tracking.payment_before_fees_tracked");
        }

        [Fact]
        public void GivenVoidedPaymentBeforeTheNewDay_WhenCorrected_ThenCorrectsIt()
        {
            var (lease, events) = _testBed.Lease().Decide(l => _testBed.Payment(l, 60m, new DateOnly(2025, 2, 3)));
            var paymentId = LeaseFeePaymentId.Create(((LeaseFeePaymentRecorded)events.Single()).PaymentId);
            var sut = lease.Decide(l => l.VoidLeaseFeePayment(paymentId, "Before tracking", _testBed.StaffUser)).Aggregate;

            var (corrected, _) = sut.Decide(l => l.CorrectLeaseFeeTracking(March15, Money.Zero, _testBed.StaffUser));

            corrected.LeaseFeeTracking.TrackedFrom.Should().Be(March15);
        }
    }

    public class TheLeaseFeeStatementAsOfMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenTemplateWithInterest_WhenCreatedFromPurchase_ThenTheLeaseChargesTheTemplatesInterest()
        {
            var template = EventReplay.Replay<GroundLeaseTemplate>(GroundLeaseTemplate.Define(
                GroundLeaseTemplateId.Generate(), "Template", 99, 45, "Covenants", ResalePolicyTerms.AppraisalBased(0.25m),
                LeaseFeeTerms.Create(Money.Create(45m), Money.Create(15m), 12m, 20), MortgageDebtCap.Create(90), _testBed.StaffUser));

            var sut = EventReplay.Replay<GroundLease>(_testBed.FromPurchase(template));

            sut.LeaseFeeTerms.Should().BeEquivalentTo(new { LateInterestRate = 12m, InterestWaiverDays = 20 });
        }

        [Fact]
        public void GivenInterestWaived_WhenAsked_ThenTheWaiverLowersTheInterestDue()
        {
            var sut = _testBed.Lease(fees: _testBed.FeesWithInterest)
                .Decide(l => l.WaiveLeaseFeeInterest(LeaseFeeInterestWaiverId.Generate(), Money.Create(1m), "Hardship", new DateOnly(2025, 2, 1), _testBed.StaffUser))
                .Aggregate;

            var statement = sut.LeaseFeeStatementAsOf(new DateOnly(2025, 2, 1));

            statement.Should().BeEquivalentTo(new { InterestCharged = Money.Create(1.86m), InterestWaived = Money.Create(1m), InterestDue = Money.Create(0.86m) });
        }

        [Fact]
        public void GivenTerminatedLease_WhenAsked_ThenStopsChargingAfterTheTerminationDate()
        {
            var sut = _testBed.Lease().Decide(l => l.Terminate("Sold", new DateOnly(2025, 3, 20), _testBed.StaffUser)).Aggregate;

            var statement = sut.LeaseFeeStatementAsOf(new DateOnly(2025, 12, 1));

            statement.TotalCharged.Amount.Should().Be(180m);
        }

        [Fact]
        public void GivenLeaseFromPurchase_WhenAsked_ThenChargesTheTemplatesFee()
        {
            var template = _testBed.ActiveTemplate("Covenants", 99, ResalePolicyTerms.AppraisalBased(0.25m));
            var sut = EventReplay.Replay<GroundLease>(_testBed.FromPurchase(template));

            var statement = sut.LeaseFeeStatementAsOf(new DateOnly(2026, 1, 1));

            statement.MonthlyFee.Amount.Should().Be(60m);
        }
    }

    public class TheRecordLeaseFeeNoticeMethod
    {
        private readonly TestBed _testBed = new();

        private static readonly DateOnly Today = new(2025, 3, 15);

        [Theory]
        [InlineData("2025-03-16", "homeownership.ground_lease.record_lease_fee_notice.sent_date_in_future")]
        [InlineData("2024-12-31", "homeownership.ground_lease.record_lease_fee_notice.sent_date_before_lease_start")]
        public void GivenInvalidSentDate_WhenRecorded_ThenThrowsBusinessRuleViolation(string sentOn, string expectedCode)
        {
            var sut = _testBed.Lease();

            var act = () => sut.RecordLeaseFeeNotice(
                LeaseFeeNoticeId.Generate(), LeaseFeeNoticeLevel.First, DateOnly.Parse(sentOn, CultureInfo.InvariantCulture),
                NoticeDelivery.FirstClassMail, DefaultCurePeriod.Default, Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Fact]
        public void GivenNothingOwedOnTheSentDate_WhenRecorded_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.Lease().Decide(l => _testBed.Payment(l, 180m, new DateOnly(2025, 1, 2))).Aggregate;

            var act = () => sut.RecordLeaseFeeNotice(
                LeaseFeeNoticeId.Generate(), LeaseFeeNoticeLevel.First, new DateOnly(2025, 3, 12),
                NoticeDelivery.FirstClassMail, DefaultCurePeriod.Default, Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.record_lease_fee_notice.nothing_owed");
        }

        [Fact]
        public void GivenBalanceDue_WhenRecorded_ThenKeepsWhatWasOwedOnTheSentDate()
        {
            var sut = _testBed.Lease().Decide(l => _testBed.Payment(l, 60m, new DateOnly(2025, 1, 2))).Aggregate;

            var (noticed, _) = sut.Decide(l => l.RecordLeaseFeeNotice(
                LeaseFeeNoticeId.Generate(), LeaseFeeNoticeLevel.First, new DateOnly(2025, 2, 12),
                NoticeDelivery.CertifiedMail, DefaultCurePeriod.Default, Today, _testBed.StaffUser));

            noticed.LeaseFeeNotices.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                Level = LeaseFeeNoticeLevel.First,
                SentOn = new DateOnly(2025, 2, 12),
                Delivery = NoticeDelivery.CertifiedMail,
                BalanceDue = Money.Create(60m),
                OldestUnpaidDueDate = new DateOnly(2025, 2, 1),
            });
        }
    }

    public class TheRecordLeaseFeeNoticeMethodForANoticeOfDefault
    {
        private readonly TestBed _testBed = new();

        // Nothing paid: January to March, $180, owed on March 15.
        private static readonly DateOnly Today = new(2025, 3, 15);

        private LeaseFeeNoticeRecorded Default(GroundLease lease, DateOnly sentOn, NoticeDelivery delivery) =>
            lease.RecordLeaseFeeNotice(
                LeaseFeeNoticeId.Generate(), LeaseFeeNoticeLevel.Default, sentOn, delivery, DefaultCurePeriod.Default,
                Today, _testBed.StaffUser);

        [Theory]
        [InlineData("FirstClassMail")]
        [InlineData("Email")]
        public void GivenDeliveryOutsideTheNoticeClause_WhenRecorded_ThenThrowsBusinessRuleViolation(string delivery)
        {
            var sut = _testBed.Lease();

            var act = () => Default(sut, Today, NoticeDelivery.FromDisplayName(delivery));

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.record_lease_fee_notice.default_notice_delivery_not_allowed");
        }

        [Fact]
        public void GivenCertifiedMail_WhenRecorded_ThenKeepsTheCurePeriodAndTheDeadlineRunsFromTheSentDate()
        {
            var (noticed, _) = _testBed.Lease().Decide(l => Default(l, new DateOnly(2025, 3, 3), NoticeDelivery.CertifiedMail));

            noticed.LeaseFeeNotices.Should().ContainSingle().Which.CurePeriod.Should().Be(DefaultCurePeriod.Default);
            noticed.DefaultCureAsOf(Today).Should().BeEquivalentTo(new
            {
                AmountOwed = Money.Create(180m),
                CureBy = new DateOnly(2025, 4, 2),
                Status = DefaultCureStatus.Curing,
            });
        }

        [Fact]
        public void GivenEarlierNoticeOfDefaultStillCuring_WhenRecorded_ThenThrowsBusinessRuleViolation()
        {
            var (sut, _) = _testBed.Lease().Decide(l => Default(l, new DateOnly(2025, 3, 3), NoticeDelivery.HandDelivered));

            var act = () => Default(sut, Today, NoticeDelivery.CertifiedMail);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.record_lease_fee_notice.default_notice_already_curing");
        }

        [Fact]
        public void GivenLateNotice_WhenRecorded_ThenItHasNoCurePeriod()
        {
            var (noticed, _) = _testBed.Lease().Decide(l => l.RecordLeaseFeeNotice(
                LeaseFeeNoticeId.Generate(), LeaseFeeNoticeLevel.Second, Today, NoticeDelivery.Email,
                DefaultCurePeriod.Default, Today, _testBed.StaffUser));

            noticed.LeaseFeeNotices.Should().ContainSingle().Which.CurePeriod.Should().BeNull();
            noticed.DefaultCureAsOf(Today).Should().BeNull();
        }
    }

    public class TheWaiveLeaseFeeInterestMethod
    {
        private readonly TestBed _testBed = new();

        // January's $60 unpaid on Feb 1 is 31 days late: $1.86 interest owed.
        private static readonly DateOnly Today = new(2025, 2, 1);

        [Theory]
        [InlineData(1, " ", "homeownership.ground_lease.waive_lease_fee_interest.reason_required")]
        [InlineData(0, "Hardship", "homeownership.ground_lease.waive_lease_fee_interest.amount_non_positive")]
        [InlineData(1.87, "Hardship", "homeownership.ground_lease.waive_lease_fee_interest.amount_above_interest_owed")]
        public void GivenInvalidWaiver_WhenWaived_ThenThrowsBusinessRuleViolation(double amount, string reason, string expectedCode)
        {
            var sut = _testBed.Lease(fees: _testBed.FeesWithInterest);

            var act = () => sut.WaiveLeaseFeeInterest(
                LeaseFeeInterestWaiverId.Generate(), Money.Create((decimal)amount), reason, Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Fact]
        public void GivenAllTheInterestOwed_WhenWaived_ThenKeepsTheWaiverWithItsTrimmedReason()
        {
            var sut = _testBed.Lease(fees: _testBed.FeesWithInterest);

            var (waived, _) = sut.Decide(l => l.WaiveLeaseFeeInterest(
                LeaseFeeInterestWaiverId.Generate(), Money.Create(1.86m), "  Job loss  ", Today, _testBed.StaffUser));

            waived.LeaseFeeInterestWaivers.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                WaivedOn = Today,
                Amount = Money.Create(1.86m),
                Reason = "Job loss",
            });
        }
    }

    public class TheRecordPermittedMortgageMethod
    {
        private readonly TestBed _testBed = new();

        private static readonly DateOnly Today = new(2025, 6, 1);

        [Theory]
        [InlineData(" ", 150_000, "2025-03-01", "homeownership.ground_lease.record_permitted_mortgage.lender_required")]
        [InlineData("Bank", 0, "2025-03-01", "homeownership.ground_lease.record_permitted_mortgage.principal_non_positive")]
        [InlineData("Bank", 150_000, "2025-06-02", "homeownership.ground_lease.record_permitted_mortgage.permitted_on_in_future")]
        [InlineData("Bank", 150_000, "2024-12-31", "homeownership.ground_lease.record_permitted_mortgage.permitted_on_before_lease_start")]
        public void GivenInvalidMortgage_WhenRecorded_ThenThrowsBusinessRuleViolation(
            string lender, int principal, string permittedOn, string expectedCode)
        {
            var sut = _testBed.Lease();

            var act = () => sut.RecordPermittedMortgage(
                PermittedMortgageId.Generate(), lender, null, Money.Create(principal),
                DateOnly.Parse(permittedOn, CultureInfo.InvariantCulture), Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Fact]
        public void GivenValidMortgage_WhenRecorded_ThenTheLeaseListsItAsRecordedWithTrimmedText()
        {
            var mortgageId = PermittedMortgageId.Generate();

            var (sut, _) = _testBed.Lease().Decide(l => l.RecordPermittedMortgage(
                mortgageId, "  Columbia Credit Union ", "  PO Box 12, Salem OR  ", Money.Create(150_000m),
                TestBed.LeaseStart, Today, _testBed.StaffUser));

            sut.PermittedMortgages.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                Id = mortgageId,
                Lender = "Columbia Credit Union",
                NoticeAddress = "PO Box 12, Salem OR",
                OriginalPrincipal = Money.Create(150_000m),
                PermittedOn = TestBed.LeaseStart,
                Source = MortgagePermissionSource.Recorded,
            });
        }

        [Fact]
        public void GivenBlankNoticeAddress_WhenRecorded_ThenItHasNone()
        {
            var recorded = _testBed.Lease().RecordPermittedMortgage(
                PermittedMortgageId.Generate(), "Bank", "  ", Money.Create(150_000m), TestBed.LeaseStart, Today, _testBed.StaffUser);

            recorded.NoticeAddress.Should().BeNull();
        }
    }

    public class TheEditPermittedMortgageMethod
    {
        private readonly TestBed _testBed = new();

        private readonly PermittedMortgageId _mortgageId = PermittedMortgageId.Generate();

        private GroundLease LeaseWithMortgage() => _testBed.Lease()
            .Decide(l => l.RecordPermittedMortgage(
                _mortgageId, "Bank", null, Money.Create(150_000m), TestBed.LeaseStart, TestBed.LeaseStart, _testBed.StaffUser))
            .Aggregate;

        [Fact]
        public void GivenNewLenderAndAddress_WhenEdited_ThenTheMortgageHasThem()
        {
            var (sut, _) = LeaseWithMortgage().Decide(l => l.EditPermittedMortgage(
                _mortgageId, " Mr. Cooper ", "8950 Cypress Waters Blvd, Coppell TX", _testBed.StaffUser));

            sut.PermittedMortgages.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                Lender = "Mr. Cooper",
                NoticeAddress = "8950 Cypress Waters Blvd, Coppell TX",
                OriginalPrincipal = Money.Create(150_000m),
            });
        }

        [Fact]
        public void GivenSameLenderAndAddress_WhenEdited_ThenNothingToDo()
        {
            var edited = LeaseWithMortgage().EditPermittedMortgage(_mortgageId, "Bank", " ", _testBed.StaffUser);

            edited.Should().BeNull();
        }

        [Fact]
        public void GivenBlankLender_WhenEdited_ThenThrowsBusinessRuleViolation()
        {
            var act = () => LeaseWithMortgage().EditPermittedMortgage(_mortgageId, " ", null, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.edit_permitted_mortgage.lender_required");
        }

        [Fact]
        public void GivenMortgageNotOnTheLease_WhenEdited_ThenThrowsBusinessRuleViolation()
        {
            var act = () => LeaseWithMortgage().EditPermittedMortgage(PermittedMortgageId.Generate(), "Bank", null, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.edit_permitted_mortgage.mortgage_not_found");
        }

        [Fact]
        public void GivenPaidOffMortgage_WhenEdited_ThenThrowsBusinessRuleViolation()
        {
            var sut = LeaseWithMortgage()
                .Decide(l => l.MarkPermittedMortgagePaidOff(_mortgageId, new DateOnly(2025, 5, 1), new DateOnly(2025, 6, 1), _testBed.StaffUser))
                .Aggregate;

            var act = () => sut.EditPermittedMortgage(_mortgageId, "Other Bank", null, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.edit_permitted_mortgage.mortgage_paid_off");
        }
    }

    public class TheMarkPermittedMortgagePaidOffMethod
    {
        private readonly TestBed _testBed = new();

        private readonly PermittedMortgageId _mortgageId = PermittedMortgageId.Generate();

        private static readonly DateOnly PermittedOn = new(2025, 3, 1);

        private static readonly DateOnly Today = new(2025, 6, 1);

        private GroundLease LeaseWithMortgage() => _testBed.Lease()
            .Decide(l => l.RecordPermittedMortgage(
                _mortgageId, "Bank", null, Money.Create(150_000m), PermittedOn, Today, _testBed.StaffUser))
            .Aggregate;

        [Fact]
        public void GivenCurrentMortgage_WhenMarkedPaidOff_ThenItKeepsTheDay()
        {
            var (sut, _) = LeaseWithMortgage().Decide(l => l.MarkPermittedMortgagePaidOff(_mortgageId, PermittedOn, Today, _testBed.StaffUser));

            sut.PermittedMortgages.Should().ContainSingle().Which.Should().BeEquivalentTo(new { PaidOffOn = PermittedOn, IsPaidOff = true });
        }

        [Theory]
        [InlineData("2025-06-02", "homeownership.ground_lease.mark_permitted_mortgage_paid_off.paid_off_on_in_future")]
        [InlineData("2025-02-28", "homeownership.ground_lease.mark_permitted_mortgage_paid_off.paid_off_on_before_permitted")]
        public void GivenInvalidDay_WhenMarkedPaidOff_ThenThrowsBusinessRuleViolation(string paidOffOn, string expectedCode)
        {
            var act = () => LeaseWithMortgage().MarkPermittedMortgagePaidOff(
                _mortgageId, DateOnly.Parse(paidOffOn, CultureInfo.InvariantCulture), Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Fact]
        public void GivenAlreadyPaidOff_WhenMarkedPaidOff_ThenThrowsBusinessRuleViolation()
        {
            var sut = LeaseWithMortgage().Decide(l => l.MarkPermittedMortgagePaidOff(_mortgageId, Today, Today, _testBed.StaffUser)).Aggregate;

            var act = () => sut.MarkPermittedMortgagePaidOff(_mortgageId, Today, Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.mark_permitted_mortgage_paid_off.already_paid_off");
        }

        [Fact]
        public void GivenMortgageNotOnTheLease_WhenMarkedPaidOff_ThenThrowsBusinessRuleViolation()
        {
            var act = () => LeaseWithMortgage().MarkPermittedMortgagePaidOff(PermittedMortgageId.Generate(), Today, Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.mark_permitted_mortgage_paid_off.mortgage_not_found");
        }
    }

    public class ThePermitRequestedMortgageMethod
    {
        private readonly TestBed _testBed = new();

        private readonly PermittedMortgageId _purchaseLoanId = PermittedMortgageId.Generate();

        private static readonly DateOnly ApprovedOn = new(2026, 5, 20);

        private static readonly DateOnly ClosedOn = new(2026, 6, 1);

        private GroundLease LeaseWithPurchaseLoan() => _testBed.Lease()
            .Decide(l => l.RecordPermittedMortgage(
                _purchaseLoanId, "First Bank", null, Money.Create(150_000m), TestBed.LeaseStart, ClosedOn, _testBed.StaffUser))
            .Aggregate;

        private MortgageRequest ApprovedRefinance(GroundLease lease)
        {
            var requestId = MortgageRequestId.Generate();
            var events = MortgageRequest.Receive(
                requestId, lease, null, new DateOnly(2026, 5, 1),
                MortgageLoanTerms.Create("Columbia Bank", MortgagePurpose.Refinance, Money.Create(140_000m), Money.Create(140_000m), 5.5m, MortgageRateType.Fixed, 30),
                ClosedOn, null, LoanAppraisal.Create(Money.Create(330_000m), new DateOnly(2026, 5, 1), null), null, ClosedOn, _testBed.StaffUser);

            return EventReplay.Replay<MortgageRequest>([.. events])
                .Decide(r => r.Approve(ApprovedOn, null, ClosedOn, _testBed.StaffUser)).Aggregate;
        }

        [Fact]
        public void GivenApprovedRefinance_WhenPermitted_ThenAddsTheLoanFromItsApprovalAndPaysOffTheOldOne()
        {
            var lease = LeaseWithPurchaseLoan();
            var newId = PermittedMortgageId.Generate();

            var (sut, _) = lease.Decide(l => l.PermitRequestedMortgage(
                newId, ApprovedRefinance(lease), ClosedOn, [_purchaseLoanId], "  PO Box 9, Portland OR  ", ClosedOn, _testBed.StaffUser));

            sut.PermittedMortgages.Should().BeEquivalentTo(new object[]
            {
                new { Id = _purchaseLoanId, PaidOffOn = (DateOnly?)ClosedOn },
                new
                {
                    Id = newId, Lender = "Columbia Bank", NoticeAddress = "PO Box 9, Portland OR", OriginalPrincipal = Money.Create(140_000m), PermittedOn = ApprovedOn,
                    Source = MortgagePermissionSource.Request, PaidOffOn = (DateOnly?)null,
                },
            });
        }

        [Fact]
        public void GivenPaidOffMortgageNotOnTheLease_WhenPermitted_ThenThrowsBusinessRuleViolation()
        {
            var lease = LeaseWithPurchaseLoan();

            var act = () => lease.PermitRequestedMortgage(
                PermittedMortgageId.Generate(), ApprovedRefinance(lease), ClosedOn, [PermittedMortgageId.Generate()], null, ClosedOn, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.mark_permitted_mortgage_paid_off.mortgage_not_found");
        }
    }

    public class TheLeaseFeeNoticeDueAsOfMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenActiveLeaseBehindPastTheFirstThreshold_WhenAsked_ThenTheFirstIsDueUntilRecorded()
        {
            var sut = _testBed.Lease().Decide(l => _testBed.Payment(l, 60m, new DateOnly(2025, 1, 2))).Aggregate;
            var today = new DateOnly(2025, 2, 12);

            sut.LeaseFeeNoticeDueAsOf(today, LeaseFeeNoticeSchedule.Default).Should().Be(LeaseFeeNoticeLevel.First);

            var noticed = sut.Decide(l => l.RecordLeaseFeeNotice(
                LeaseFeeNoticeId.Generate(), LeaseFeeNoticeLevel.First, today, NoticeDelivery.FirstClassMail, DefaultCurePeriod.Default, today, _testBed.StaffUser)).Aggregate;

            noticed.LeaseFeeNoticeDueAsOf(today, LeaseFeeNoticeSchedule.Default).Should().BeNull();
        }

        [Fact]
        public void GivenEndedLeaseWithArrears_WhenAsked_ThenNoNoticeIsDue()
        {
            var sut = _testBed.Lease().Decide(l => l.Terminate("Sold", new DateOnly(2025, 2, 15), _testBed.StaffUser)).Aggregate;

            sut.LeaseFeeNoticeDueAsOf(new DateOnly(2025, 6, 1), LeaseFeeNoticeSchedule.Default).Should().BeNull();
        }
    }
}
