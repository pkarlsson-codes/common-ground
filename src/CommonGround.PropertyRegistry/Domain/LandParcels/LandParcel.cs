using CommonGround.PropertyRegistry.Domain.LandParcels.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.PropertyRegistry.Domain.LandParcels;

/// <summary>
/// Land owned permanently by the CLT. Never sold.
/// Structures may be built on it; homeowners lease the land but do not own it.
/// </summary>
public sealed class LandParcel : AggregateRoot<LandParcelId>
{
#pragma warning disable CS8618
    private LandParcel() { }
#pragma warning restore CS8618

    /// <summary>Gets the legal parcel number (APN or legal description).</summary>
    public string LegalParcelNumber { get; private set; }

    /// <summary>Gets the street address of the parcel.</summary>
    public string Address { get; private set; }

    /// <summary>Gets the area in acres.</summary>
    public decimal AreaAcres { get; private set; }

    /// <summary>Gets the date the CLT acquired this parcel.</summary>
    public DateOnly AcquisitionDate { get; private set; }

    /// <summary>Gets the method by which the CLT acquired this parcel.</summary>
    public AcquisitionMethod AcquisitionMethod { get; private set; }

    /// <summary>Gets optional free-text description of the affordability covenant terms.</summary>
    public string? CovenantTerms { get; private set; }

    /// <summary>Gets the current status of the parcel.</summary>
    public ParcelStatus Status { get; private set; }

    /// <summary>Gets the ID of the user who registered this parcel.</summary>
    public UserId RegisteredBy { get; private set; }

    /// <summary>Registers a new land parcel in the CLT portfolio.</summary>
    public static LandParcel Register(
        string legalParcelNumber,
        string address,
        decimal areaAcres,
        DateOnly acquisitionDate,
        AcquisitionMethod acquisitionMethod,
        string? covenantTerms,
        UserId registeredBy)
    {
        ArgumentNullException.ThrowIfNull(acquisitionMethod);
        ArgumentNullException.ThrowIfNull(registeredBy);

        if (string.IsNullOrWhiteSpace(legalParcelNumber))
            throw new BusinessRuleViolationException("Legal parcel number must be specified.");

        if (string.IsNullOrWhiteSpace(address))
            throw new BusinessRuleViolationException("Address must be specified.");

        if (areaAcres <= 0)
            throw new BusinessRuleViolationException("Area must be greater than zero.");

        var parcel = new LandParcel();

        parcel.ApplyChange(new LandParcelRegistered(
            LandParcelId.Generate().Value,
            legalParcelNumber,
            address,
            areaAcres,
            acquisitionDate,
            acquisitionMethod.DisplayName,
            covenantTerms,
            registeredBy.Value));

        return parcel;
    }

    /// <summary>Transitions a parcel from Available or Transitioning to Predevelopment.</summary>
    public void StartPredevelopment(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (Status == ParcelStatus.Predevelopment)
            return;

        EnsureTransitionAllowed(ParcelStatus.Predevelopment, ParcelStatus.Available, ParcelStatus.Transitioning);
        ApplyChange(new ParcelEnteredPredevelopment(Id.Value, changedBy.Value));
    }

    /// <summary>Transitions a parcel from Predevelopment to UnderConstruction.</summary>
    public void StartConstruction(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (Status == ParcelStatus.UnderConstruction)
            return;

        EnsureTransitionAllowed(ParcelStatus.UnderConstruction, ParcelStatus.Predevelopment);
        ApplyChange(new ParcelConstructionStarted(Id.Value, changedBy.Value));
    }

    /// <summary>Transitions a parcel from UnderConstruction or Transitioning to ReadyForOccupancy.</summary>
    public void MarkReadyForOccupancy(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (Status == ParcelStatus.ReadyForOccupancy)
            return;

        EnsureTransitionAllowed(ParcelStatus.ReadyForOccupancy, ParcelStatus.UnderConstruction, ParcelStatus.Transitioning);
        ApplyChange(new ParcelMarkedReadyForOccupancy(Id.Value, changedBy.Value));
    }

    /// <summary>Transitions a parcel from ReadyForOccupancy to Occupied.</summary>
    public void MarkOccupied(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (Status == ParcelStatus.Occupied)
            return;

        EnsureTransitionAllowed(ParcelStatus.Occupied, ParcelStatus.ReadyForOccupancy);
        ApplyChange(new ParcelOccupied(Id.Value, changedBy.Value));
    }

    /// <summary>Transitions a parcel from Occupied to Transitioning.</summary>
    public void EnterTransitioning(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (Status == ParcelStatus.Transitioning)
            return;

        EnsureTransitionAllowed(ParcelStatus.Transitioning, ParcelStatus.Occupied);
        ApplyChange(new ParcelEnteredTransitioning(Id.Value, changedBy.Value));
    }

    /// <summary>Transitions a parcel from Transitioning to Available.</summary>
    public void ReturnToAvailable(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (Status == ParcelStatus.Available)
            return;

        EnsureTransitionAllowed(ParcelStatus.Available, ParcelStatus.Transitioning);
        ApplyChange(new ParcelReturnedToAvailable(Id.Value, changedBy.Value));
    }

    /// <summary>Transitions a parcel to Decommissioned from Available, Predevelopment, or Transitioning.</summary>
    public void Decommission(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (Status == ParcelStatus.Decommissioned)
            return;

        EnsureTransitionAllowed(ParcelStatus.Decommissioned, ParcelStatus.Available, ParcelStatus.Predevelopment, ParcelStatus.Transitioning);
        ApplyChange(new ParcelDecommissioned(Id.Value, changedBy.Value));
    }

    /// <summary>Transitions a parcel from Decommissioned to Available.</summary>
    public void Recommission(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (Status == ParcelStatus.Available)
            return;

        EnsureTransitionAllowed(ParcelStatus.Available, ParcelStatus.Decommissioned);
        ApplyChange(new ParcelRecommissioned(Id.Value, changedBy.Value));
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case LandParcelRegistered e: Apply(e); break;
            case ParcelEnteredPredevelopment e: Apply(e); break;
            case ParcelConstructionStarted e: Apply(e); break;
            case ParcelMarkedReadyForOccupancy e: Apply(e); break;
            case ParcelOccupied e: Apply(e); break;
            case ParcelEnteredTransitioning e: Apply(e); break;
            case ParcelReturnedToAvailable e: Apply(e); break;
            case ParcelDecommissioned e: Apply(e); break;
            case ParcelRecommissioned e: Apply(e); break;
            default: throw new InvalidOperationException($"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(LandParcelRegistered e)
    {
        Id = LandParcelId.Create(e.AggregateId);
        LegalParcelNumber = e.LegalParcelNumber;
        Address = e.Address;
        AreaAcres = e.AreaAcres;
        AcquisitionDate = e.AcquisitionDate;
        AcquisitionMethod = AcquisitionMethod.FromDisplayName(e.AcquisitionMethod);
        CovenantTerms = e.CovenantTerms;
        Status = ParcelStatus.Available;
        RegisteredBy = UserId.Create(e.RegisteredBy);
    }

    private void Apply(ParcelEnteredPredevelopment e)
    {
        Status = ParcelStatus.Predevelopment;
    }

    private void Apply(ParcelConstructionStarted e)
    {
        Status = ParcelStatus.UnderConstruction;
    }

    private void Apply(ParcelMarkedReadyForOccupancy e)
    {
        Status = ParcelStatus.ReadyForOccupancy;
    }

    private void Apply(ParcelOccupied e)
    {
        Status = ParcelStatus.Occupied;
    }

    private void Apply(ParcelEnteredTransitioning e)
    {
        Status = ParcelStatus.Transitioning;
    }

    private void Apply(ParcelReturnedToAvailable e)
    {
        Status = ParcelStatus.Available;
    }

    private void Apply(ParcelDecommissioned e)
    {
        Status = ParcelStatus.Decommissioned;
    }

    private void Apply(ParcelRecommissioned e)
    {
        Status = ParcelStatus.Available;
    }

    private void EnsureTransitionAllowed(ParcelStatus targetStatus, params ParcelStatus[] allowedFrom)
    {
        foreach (var fromStatus in allowedFrom)
        {
            if (Status == fromStatus)
                return;
        }

        throw new BusinessRuleViolationException(
            $"Cannot transition parcel from '{Status.DisplayName}' to '{targetStatus.DisplayName}'.");
    }
}