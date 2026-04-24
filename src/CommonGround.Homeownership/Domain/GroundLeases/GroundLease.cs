using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.PropertyRegistry.Domain.Buildings;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.Homeownership.Domain.GroundLeases;

/// <summary>
/// A ground lease between the CLT and a homeowner.
/// The CLT retains land ownership; the homeowner leases the land and owns the structure.
/// </summary>
public sealed class GroundLease : AggregateRoot<GroundLeaseId>
{
#pragma warning disable CS8618
    private GroundLease() { }
#pragma warning restore CS8618

    /// <summary>Gets the ID of the land parcel this lease is for.</summary>
    public LandParcelId LandParcelId { get; private set; }

    /// <summary>Gets the ID of the building on this leased parcel.</summary>
    public BuildingId BuildingId { get; private set; }

    /// <summary>Gets the start date of the lease.</summary>
    public DateOnly LeaseStartDate { get; private set; }

    /// <summary>Gets the term of the lease in years.</summary>
    public int TermYears { get; private set; }

    /// <summary>Gets the covenant terms for this lease.</summary>
    public string CovenantTerms { get; private set; }

    /// <summary>Gets the resale policy terms for this lease.</summary>
    public ResalePolicyTerms ResalePolicyTerms { get; private set; }

    /// <summary>Gets the current homeowner, if any.</summary>
    public Homeowner? CurrentHomeowner { get; private set; }

    /// <summary>Gets the status of this ground lease.</summary>
    public GroundLeaseStatus Status { get; private set; }

    /// <summary>Gets when this lease was created.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Gets the user who created this lease.</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>Creates a new ground lease.</summary>
    public static GroundLease Create(
        LandParcelId landParcelId,
        BuildingId buildingId,
        DateOnly leaseStartDate,
        int termYears,
        string covenantTerms,
        ResalePolicyTerms resalePolicyTerms,
        UserId createdBy)
    {
        ArgumentNullException.ThrowIfNull(landParcelId);
        ArgumentNullException.ThrowIfNull(buildingId);
        ArgumentNullException.ThrowIfNull(resalePolicyTerms);
        ArgumentNullException.ThrowIfNull(createdBy);

        if (termYears <= 0)
            throw new BusinessRuleViolationException("Term years must be positive.");

        if (string.IsNullOrWhiteSpace(covenantTerms))
            throw new BusinessRuleViolationException("Covenant terms must be specified.");

        var lease = new GroundLease();
        lease.ApplyChange(new GroundLeaseCreated(
            GroundLeaseId.Generate().Value,
            landParcelId.Value,
            buildingId.Value,
            leaseStartDate,
            termYears,
            covenantTerms,
            resalePolicyTerms.FormulaKind,
            resalePolicyTerms.AppreciationShare,
            resalePolicyTerms.IndexType,
            resalePolicyTerms.BaselineIndexValue,
            resalePolicyTerms.AnnualPercentage,
            resalePolicyTerms.ImprovementsCap,
            createdBy.Value,
            DateTimeOffset.UtcNow));

        return lease;
    }

    /// <summary>Onboards the first homeowner to this lease.</summary>
    /// <param name="homeowner">The homeowner to onboard.</param>
    /// <param name="onboardedBy">The user recording this action.</param>
    public void OnboardHomeowner(Homeowner homeowner, UserId onboardedBy)
    {
        ArgumentNullException.ThrowIfNull(homeowner);
        ArgumentNullException.ThrowIfNull(onboardedBy);

        if (CurrentHomeowner is not null)
        {
            throw new BusinessRuleViolationException(
                "A homeowner is already onboarded. Use ChangeHomeowner instead.");
        }

        ApplyChange(new HomeownerOnboarded(
            Id.Value,
            homeowner.FullName,
            homeowner.Email,
            homeowner.Phone,
            onboardedBy.Value,
            DateTimeOffset.UtcNow));
    }

    /// <summary>Changes the homeowner on this lease.</summary>
    /// <param name="newHomeowner">The new homeowner.</param>
    /// <param name="changedBy">The user recording this action.</param>
    public void ChangeHomeowner(Homeowner newHomeowner, UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(newHomeowner);
        ArgumentNullException.ThrowIfNull(changedBy);

        if (CurrentHomeowner is null)
        {
            throw new BusinessRuleViolationException(
                "No homeowner is currently onboarded. Use OnboardHomeowner first.");
        }

        ApplyChange(new HomeownerChanged(
            Id.Value,
            CurrentHomeowner.FullName,
            CurrentHomeowner.Email,
            CurrentHomeowner.Phone,
            newHomeowner.FullName,
            newHomeowner.Email,
            newHomeowner.Phone,
            changedBy.Value,
            DateTimeOffset.UtcNow));
    }

    /// <summary>Terminates this ground lease.</summary>
    /// <param name="reason">The reason for termination.</param>
    /// <param name="terminatedDate">The date of termination.</param>
    /// <param name="terminatedBy">The user recording this action.</param>
    public void Terminate(string reason, DateOnly terminatedDate, UserId terminatedBy)
    {
        ArgumentNullException.ThrowIfNull(terminatedBy);

        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleViolationException("Termination reason is required.");

        if (Status != GroundLeaseStatus.Active)
            throw new BusinessRuleViolationException("Only Active leases can be terminated.");

        ApplyChange(new GroundLeaseTerminated(
            Id.Value,
            reason,
            terminatedDate,
            terminatedBy.Value,
            DateTimeOffset.UtcNow));
    }

    /// <summary>Records a stewardship check-in visit.</summary>
    /// <param name="checkInDate">Date of the check-in.</param>
    /// <param name="notes">Notes from the visit.</param>
    /// <param name="recordedBy">The user recording this action.</param>
    public void RecordStewardshipCheckIn(DateOnly checkInDate, string notes, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);

        if (string.IsNullOrWhiteSpace(notes))
        {
            throw new BusinessRuleViolationException("Check-in notes are required.");
        }

        ApplyChange(new StewardshipCheckInRecorded(
            Id.Value,
            checkInDate,
            notes,
            recordedBy.Value,
            DateTimeOffset.UtcNow));
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case GroundLeaseCreated e: Apply(e); break;
            case HomeownerOnboarded e: Apply(e); break;
            case HomeownerChanged e: Apply(e); break;
            case StewardshipCheckInRecorded: break; // No state mutation needed
            case GroundLeaseTerminated e: Apply(e); break;
            default: throw new InvalidOperationException(
                $"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(GroundLeaseCreated e)
    {
        Id = GroundLeaseId.Create(e.AggregateId);
        LandParcelId = LandParcelId.Create(e.LandParcelId);
        BuildingId = BuildingId.Create(e.BuildingId);
        LeaseStartDate = e.LeaseStartDate;
        TermYears = e.TermYears;
        CovenantTerms = e.CovenantTerms;
        ResalePolicyTerms = e.FormulaKind switch
        {
            FormulaKind.AppraisalBased => ResalePolicyTerms.AppraisalBased(
                e.AppreciationShare!.Value, e.ImprovementsCap),
            FormulaKind.IndexBased => ResalePolicyTerms.IndexBased(
                e.IndexType!, e.BaselineIndexValue!.Value, e.ImprovementsCap),
            FormulaKind.FixedPercentage => ResalePolicyTerms.FixedPercentage(
                e.AnnualPercentage!.Value, e.ImprovementsCap),
            _ => throw new InvalidOperationException($"Unknown formula kind: {e.FormulaKind}")
        };
        Status = GroundLeaseStatus.Active;
        CreatedBy = UserId.Create(e.RegisteredBy);
        CreatedAt = e.OccurredAt;
    }

    private void Apply(HomeownerOnboarded e)
    {
        CurrentHomeowner = new Homeowner(e.FullName, e.Email, e.Phone);
    }

    private void Apply(HomeownerChanged e)
    {
        CurrentHomeowner = new Homeowner(e.NewFullName, e.NewEmail, e.NewPhone);
    }

    private void Apply(GroundLeaseTerminated _)
    {
        Status = GroundLeaseStatus.Inactive;
    }

}
