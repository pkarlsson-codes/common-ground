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

    /// <summary>Gets the ID of the user who registered this parcel.</summary>
    public UserId RegisteredBy { get; private set; }

    /// <summary>Gets whether the parcel is decommissioned.</summary>
    public bool IsDecommissioned { get; private set; }

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

    /// <summary>Lists the parcel for placement.</summary>
    public void ListForPlacement(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (IsDecommissioned)
            throw new BusinessRuleViolationException("Cannot list a decommissioned parcel for placement.");

        ApplyChange(new ParcelListedForPlacement(Id.Value, changedBy.Value));
    }

    /// <summary>Withdraws the parcel for redevelopment.</summary>
    public void WithdrawForRedevelopment(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (IsDecommissioned)
            throw new BusinessRuleViolationException("Cannot withdraw a decommissioned parcel.");

        ApplyChange(new ParcelWithdrawnForRedevelopment(Id.Value, changedBy.Value));
    }

    /// <summary>Decommissions the parcel.</summary>
    public void Decommission(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (IsDecommissioned)
            return;

        ApplyChange(new ParcelDecommissioned(Id.Value, changedBy.Value));
    }

    /// <summary>Recommissions a decommissioned parcel. Idempotent if already active.</summary>
    public void Recommission(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (!IsDecommissioned)
            return;

        ApplyChange(new ParcelRecommissioned(Id.Value, changedBy.Value));
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case LandParcelRegistered e: Apply(e); break;
            case ParcelListedForPlacement: break;
            case ParcelWithdrawnForRedevelopment: break;
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
        RegisteredBy = UserId.Create(e.RegisteredBy);
    }

    private void Apply(ParcelDecommissioned _)
    {
        IsDecommissioned = true;
    }

    private void Apply(ParcelRecommissioned _)
    {
        IsDecommissioned = false;
    }
}
