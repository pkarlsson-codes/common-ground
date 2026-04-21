using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.PropertyRegistry.Domain.Buildings;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases;

public class GroundLeaseTests
{
    private static readonly UserId StaffUser = UserId.Generate();
    private static readonly LandParcelId ParcelId = LandParcelId.Generate();
    private static readonly BuildingId TestBuildingId = BuildingId.Generate();

    private static GroundLease CreateLease()
    {
        return GroundLease.Create(
            ParcelId,
            TestBuildingId,
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
            TestBuildingId,
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
        lease.BuildingId.Should().Be(TestBuildingId);
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
            ParcelId, TestBuildingId, new DateOnly(2025, 1, 1),
            0, "Terms", ResalePolicyTerms.AppraisalBased(0.25m), StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_EmptyCovenantTerms_Throws()
    {
        var act = () => GroundLease.Create(
            ParcelId, TestBuildingId, new DateOnly(2025, 1, 1),
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
    public void OnboardHomeowner_NoExistingHomeowner_SetsHomeowner()
    {
        var lease = CreateLease();
        var homeowner = new Homeowner("Jane Doe", "jane@example.com", "+12025551234");

        lease.OnboardHomeowner(homeowner, StaffUser);

        lease.CurrentHomeowner.Should().NotBeNull();
        lease.CurrentHomeowner!.FullName.Should().Be("Jane Doe");
        lease.CurrentHomeowner.Email.Should().Be("jane@example.com");
        lease.CurrentHomeowner.Phone.Should().Be("+12025551234");
    }

    [Fact]
    public void OnboardHomeowner_AlreadyOnboarded_Throws()
    {
        var lease = CreateLease();
        lease.OnboardHomeowner(new Homeowner("Jane Doe", "jane@example.com"), StaffUser);

        var act = () => lease.OnboardHomeowner(
            new Homeowner("Other Person", "other@example.com"), StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void ChangeHomeowner_WithExisting_UpdatesHomeowner()
    {
        var lease = CreateLease();
        lease.OnboardHomeowner(new Homeowner("Jane Doe", "jane@example.com"), StaffUser);

        var newOwner = new Homeowner("John Smith", "john@example.com", "+12025559999");
        lease.ChangeHomeowner(newOwner, StaffUser);

        lease.CurrentHomeowner!.FullName.Should().Be("John Smith");
        lease.CurrentHomeowner.Email.Should().Be("john@example.com");
    }

    [Fact]
    public void ChangeHomeowner_NoExisting_Throws()
    {
        var lease = CreateLease();

        var act = () => lease.ChangeHomeowner(
            new Homeowner("John Smith", "john@example.com"), StaffUser);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void ChangeHomeowner_EmitsEventWithPreviousData()
    {
        var lease = CreateLease();
        lease.OnboardHomeowner(new Homeowner("Jane Doe", "jane@example.com", "+12025551234"), StaffUser);

        lease.ChangeHomeowner(new Homeowner("John Smith", "john@example.com"), StaffUser);

        var changeEvent = lease.UncommittedEvents.OfType<HomeownerChanged>().Single();
        changeEvent.PreviousFullName.Should().Be("Jane Doe");
        changeEvent.PreviousEmail.Should().Be("jane@example.com");
        changeEvent.PreviousPhone.Should().Be("+12025551234");
        changeEvent.NewFullName.Should().Be("John Smith");
        changeEvent.NewEmail.Should().Be("john@example.com");
        changeEvent.NewPhone.Should().BeNull();
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
}
