using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases;

public class GroundLeaseTests
{
    private const string ParcelId = "land-parcel-1234";
    private const string HomeId = "home-5678";
    private static readonly UserId StaffUser = UserId.Generate();
    private static readonly PurchaseWorkflowId PurchaseWorkflowId = PurchaseWorkflowId.Generate();

    private static PurchaseWorkflow CreatePurchaseWorkflow()
    {
        return PurchaseWorkflow.Initiate(
            GroundLeaseTemplateId.Generate(),
            ParcelId,
            HomeId,
            new Homeowner("Buyer Name", "buyer@example.com"),
            250000m,
            StaffUser);
    }

    private static GroundLease CreateLease()
    {
        return GroundLease.Create(
            ParcelId,
            HomeId,
            new DateOnly(2025, 1, 1),
            99,
            "Standard affordability covenant",
            ResalePolicyTerms.AppraisalBased(0.25m),
            StaffUser);
    }

    private static GroundLease CreateLease(ResalePolicyTerms terms)
    {
        return GroundLease.Create(
            ParcelId,
            HomeId,
            new DateOnly(2025, 1, 1),
            99,
            "Standard affordability covenant",
            terms,
            StaffUser);
    }

    [Fact]
    public void Create_ValidInputs_CreatesActiveLeaseWithNoHomeowner()
    {
        var lease = CreateLease();

        lease.LandParcelId.Should().Be(ParcelId);
        lease.UnitId.Should().Be(HomeId);
        lease.LeaseStartDate.Should().Be(new DateOnly(2025, 1, 1));
        lease.TermYears.Should().Be(99);
        lease.CovenantTerms.Should().Be("Standard affordability covenant");
        lease.Status.Should().Be(GroundLeaseStatus.Active);
        lease.CurrentHomeowner.Should().BeNull();
        lease.CreatedBy.Should().Be(StaffUser);
    }

    [Fact]
    public void Create_ZeroTermYears_Throws()
    {
        var act = () => GroundLease.Create(
            ParcelId, HomeId, new DateOnly(2025, 1, 1),
            0, "Terms", ResalePolicyTerms.AppraisalBased(0.25m), StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_EmptyCovenantTerms_Throws()
    {
        var act = () => GroundLease.Create(
            ParcelId, HomeId, new DateOnly(2025, 1, 1),
            99, "", ResalePolicyTerms.AppraisalBased(0.25m), StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_EmitsGroundLeaseCreatedEvent()
    {
        var lease = CreateLease();

        lease.UncommittedEvents.Should().HaveCount(1);
        lease.UncommittedEvents.First().Should().BeOfType<GroundLeaseCreated>();
    }

    [Fact]
    public void Create_ResalePolicyTerms_AreHydrated()
    {
        var lease = CreateLease();

        lease.ResalePolicyTerms.FormulaKind.Should().Be(FormulaKind.AppraisalBased);
        lease.ResalePolicyTerms.AppreciationShare.Should().Be(0.25m);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_AppraisalTerms_RoundTripsImprovementsCap(bool withCap)
    {
        var terms = ResalePolicyTerms.AppraisalBased(0.25m, withCap ? 12_000m : null);
        var lease = CreateLease(terms);

        lease.ResalePolicyTerms.FormulaKind.Should().Be(FormulaKind.AppraisalBased);
        lease.ResalePolicyTerms.AppreciationShare.Should().Be(0.25m);
        lease.ResalePolicyTerms.ImprovementsCap.Should().Be(withCap ? 12_000m : null);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_IndexTerms_RoundTripsImprovementsCap(bool withCap)
    {
        var terms = ResalePolicyTerms.IndexBased("CPI", 100m, withCap ? 18_500m : null);
        var lease = CreateLease(terms);

        lease.ResalePolicyTerms.FormulaKind.Should().Be(FormulaKind.IndexBased);
        lease.ResalePolicyTerms.IndexType.Should().Be("CPI");
        lease.ResalePolicyTerms.BaselineIndexValue.Should().Be(100m);
        lease.ResalePolicyTerms.ImprovementsCap.Should().Be(withCap ? 18_500m : null);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_FixedTerms_RoundTripsImprovementsCap(bool withCap)
    {
        var terms = ResalePolicyTerms.FixedPercentage(0.03m, withCap ? 9_900m : null);
        var lease = CreateLease(terms);

        lease.ResalePolicyTerms.FormulaKind.Should().Be(FormulaKind.FixedPercentage);
        lease.ResalePolicyTerms.AnnualPercentage.Should().Be(0.03m);
        lease.ResalePolicyTerms.ImprovementsCap.Should().Be(withCap ? 9_900m : null);
    }

    [Fact]
    public void RecordStewardshipCheckIn_ValidNotes_EmitsEvent()
    {
        var lease = CreateLease();
        var checkInDate = new DateOnly(2025, 6, 15);

        lease.RecordStewardshipCheckIn(checkInDate, "Property in good condition", StaffUser);

        var checkInEvent = lease.UncommittedEvents.OfType<StewardshipCheckInRecorded>().Single();
        checkInEvent.CheckInDate.Should().Be(checkInDate);
        checkInEvent.Notes.Should().Be("Property in good condition");
    }

    [Fact]
    public void RecordStewardshipCheckIn_EmptyNotes_Throws()
    {
        var lease = CreateLease();

        var act = () => lease.RecordStewardshipCheckIn(
            new DateOnly(2025, 6, 15), "", StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void CreateFromTemplate_ActiveTemplate_CreatesLease()
    {
        var template = GroundLeaseTemplate.Create(
            "Default 99-Year",
            99,
            "Template covenant terms",
            ResalePolicyTerms.IndexBased("CPI", 100m, 7500m),
            StaffUser);

        var lease = GroundLease.CreateFromPurchase(
            template,
            CreatePurchaseWorkflow(),
            new DateOnly(2026, 1, 1),
            StaffUser);

        lease.TemplateId.Should().Be(template.Id);
        lease.TermYears.Should().Be(99);
        lease.CovenantTerms.Should().Be("Template covenant terms");
        lease.ResalePolicyTerms.FormulaKind.Should().Be(FormulaKind.IndexBased);
        lease.ResalePolicyTerms.IndexType.Should().Be("CPI");
        lease.CurrentHomeowner.Should().NotBeNull();
        lease.CurrentHomeowner.FullName.Should().Be("Buyer Name");
        lease.CurrentHomeowner.Email.Should().Be("buyer@example.com");
    }

    [Fact]
    public void CreateFromTemplate_InactiveTemplate_Throws()
    {
        var template = GroundLeaseTemplate.Create(
            "Inactive Template",
            50,
            "Covenants",
            ResalePolicyTerms.AppraisalBased(0.25m),
            StaffUser);
        template.Deactivate(StaffUser);

        var act = () => GroundLease.CreateFromPurchase(
            template,
            CreatePurchaseWorkflow(),
            new DateOnly(2026, 1, 1),
            StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void CreateFromTemplate_EmitsGroundLeaseCreatedFromTemplateEvent()
    {
        var template = GroundLeaseTemplate.Create(
            "Template A",
            40,
            "Template covenants",
            ResalePolicyTerms.FixedPercentage(0.02m, 12000m),
            StaffUser);

        var lease = GroundLease.CreateFromPurchase(
            template,
            CreatePurchaseWorkflow(),
            new DateOnly(2026, 4, 1),
            StaffUser);

        lease.UncommittedEvents.Should().HaveCount(1);
        var evt = lease.UncommittedEvents.Single()
            .Should().BeOfType<GroundLeaseCreatedFromPurchase>().Subject;

        evt.GroundLeaseTemplateId.Should().Be(template.Id.Value);
        evt.TermYears.Should().Be(40);
        evt.CovenantTerms.Should().Be("Template covenants");
        evt.FormulaKind.Should().Be(FormulaKind.FixedPercentage);
        evt.AnnualPercentage.Should().Be(0.02m);
        evt.ImprovementsCap.Should().Be(12000m);
    }

    [Fact]
    public void Create_DirectPath_LeavesTemplateIdNull()
    {
        var lease = CreateLease();

        lease.TemplateId.Should().BeNull();
    }

    [Fact]
    public void CreateFromTemplate_AppraisalTerms_RoundTrips()
    {
        var template = GroundLeaseTemplate.Create(
            "Appraisal",
            99,
            "Appraisal covenants",
            ResalePolicyTerms.AppraisalBased(0.33m, 15000m),
            StaffUser);

        var lease = GroundLease.CreateFromPurchase(
            template,
            CreatePurchaseWorkflow(),
            new DateOnly(2026, 2, 1),
            StaffUser);

        lease.ResalePolicyTerms.FormulaKind.Should().Be(FormulaKind.AppraisalBased);
        lease.ResalePolicyTerms.AppreciationShare.Should().Be(0.33m);
        lease.ResalePolicyTerms.ImprovementsCap.Should().Be(15000m);
    }

    [Fact]
    public void CreateFromTemplate_IndexTerms_RoundTrips()
    {
        var template = GroundLeaseTemplate.Create(
            "Index",
            60,
            "Index covenants",
            ResalePolicyTerms.IndexBased("CPI", 210m, null),
            StaffUser);

        var lease = GroundLease.CreateFromPurchase(
            template,
            CreatePurchaseWorkflow(),
            new DateOnly(2026, 2, 1),
            StaffUser);

        lease.ResalePolicyTerms.FormulaKind.Should().Be(FormulaKind.IndexBased);
        lease.ResalePolicyTerms.IndexType.Should().Be("CPI");
        lease.ResalePolicyTerms.BaselineIndexValue.Should().Be(210m);
        lease.ResalePolicyTerms.ImprovementsCap.Should().BeNull();
    }

    [Fact]
    public void CreateFromTemplate_FixedTerms_RoundTrips()
    {
        var template = GroundLeaseTemplate.Create(
            "Fixed",
            30,
            "Fixed covenants",
            ResalePolicyTerms.FixedPercentage(0.015m, 5000m),
            StaffUser);

        var lease = GroundLease.CreateFromPurchase(
            template,
            CreatePurchaseWorkflow(),
            new DateOnly(2026, 2, 1),
            StaffUser);

        lease.ResalePolicyTerms.FormulaKind.Should().Be(FormulaKind.FixedPercentage);
        lease.ResalePolicyTerms.AnnualPercentage.Should().Be(0.015m);
        lease.ResalePolicyTerms.ImprovementsCap.Should().Be(5000m);
    }
}
