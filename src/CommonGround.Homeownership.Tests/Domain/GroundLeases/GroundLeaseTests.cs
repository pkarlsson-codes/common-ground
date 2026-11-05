using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates.Events;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
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

        public GroundLeaseCreated Established(ResalePolicyTerms? terms = null) =>
            GroundLease.Establish(
                LeaseId, ParcelId, HomeId, Homeowner, new DateOnly(2025, 1, 1), 99, "Standard affordability covenant",
                terms ?? ResalePolicyTerms.AppraisalBased(0.25m), HomeCommitments.None, StaffUser);

        public GroundLease Lease(ResalePolicyTerms? terms = null) => EventReplay.Replay<GroundLease>(Established(terms));

        public GroundLeaseTemplate ActiveTemplate(string covenantTerms, int termYears, ResalePolicyTerms terms) =>
            EventReplay.Replay<GroundLeaseTemplate>(
                GroundLeaseTemplate.Define(GroundLeaseTemplateId.Generate(), "Template", termYears, covenantTerms, terms, StaffUser));

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
            GroundLease.CreateFromPurchase(LeaseId, template, Workflow(), new DateOnly(2026, 1, 1), StaffUser);
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
                Status = GroundLeaseStatus.Active,
                CurrentHomeowner = _testBed.Homeowner,
                TemplateId = (GroundLeaseTemplateId?)null,
                PurchaseWorkflowId = (PurchaseWorkflowId?)null,
                CreatedBy = _testBed.StaffUser,
            });
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
                99, "Terms", ResalePolicyTerms.AppraisalBased(0.25m),
                HomeCommitments.Create(activeLeaseId, openPurchaseWorkflowId, openHomeSelectionId), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be(expectedCode);
        }

        [Theory]
        [InlineData(0, "Terms")]
        [InlineData(99, "")]
        public void GivenInvalidInputs_WhenEstablished_ThenThrowsBusinessRuleViolation(int termYears, string covenantTerms)
        {
            var act = () => GroundLease.Establish(
                _testBed.LeaseId, TestBed.ParcelId, TestBed.HomeId, _testBed.Homeowner, new DateOnly(2025, 1, 1),
                termYears, covenantTerms, ResalePolicyTerms.AppraisalBased(0.25m), HomeCommitments.None, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>();
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

        public static TheoryData<ResalePolicyTerms> TemplateTerms() => new()
        {
            ResalePolicyTerms.AppraisalBased(0.33m, 15000m),
            ResalePolicyTerms.IndexBased("CPI", 210m, null),
            ResalePolicyTerms.FixedPercentage(0.015m, 5000m),
        };
    }
}
