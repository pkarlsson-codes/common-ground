using CommonGround.PropertyRegistry.Application.Errors;
using CommonGround.PropertyRegistry.Domain.LandParcels.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels;

/// <summary>
/// Land owned permanently by the CLT. Never sold.
/// Structures may be built on it; homeowners lease the land but do not own it.
/// </summary>
public sealed class LandParcel : AggregateRoot<LandParcelId>
{
#pragma warning disable CS8618 // State is set by Create from the stream's first event.
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
    public static LandParcelRegistered Register(
        LandParcelId id,
        string legalParcelNumber,
        string address,
        decimal areaAcres,
        DateOnly acquisitionDate,
        AcquisitionMethod acquisitionMethod,
        string? covenantTerms,
        UserId registeredBy)
    {
        ArgumentNullException.ThrowIfNull(acquisitionMethod);

        if (string.IsNullOrWhiteSpace(legalParcelNumber))
            throw BusinessRuleViolation.LandParcel.Register.LegalParcelNumberRequired();

        if (string.IsNullOrWhiteSpace(address))
            throw BusinessRuleViolation.LandParcel.Register.AddressRequired();

        if (areaAcres <= 0)
            throw BusinessRuleViolation.LandParcel.Register.AreaNonPositive(areaAcres);

        return new LandParcelRegistered(
            id.Value,
            legalParcelNumber,
            address,
            areaAcres,
            acquisitionDate,
            acquisitionMethod,
            covenantTerms,
            registeredBy.Value);
    }

    /// <summary>Lists the parcel for placement.</summary>
    public ParcelListedForPlacement ListForPlacement(UserId changedBy)
    {
        if (IsDecommissioned)
            throw BusinessRuleViolation.LandParcel.ListForPlacement.ParcelDecommissioned();

        return new ParcelListedForPlacement(Id.Value, changedBy.Value);
    }

    /// <summary>Withdraws the parcel for redevelopment.</summary>
    public ParcelWithdrawnForRedevelopment WithdrawForRedevelopment(UserId changedBy)
    {
        if (IsDecommissioned)
            throw BusinessRuleViolation.LandParcel.WithdrawForRedevelopment.ParcelDecommissioned();

        return new ParcelWithdrawnForRedevelopment(Id.Value, changedBy.Value);
    }

    /// <summary>Decommissions the parcel.</summary>
    public ParcelDecommissioned? Decommission(UserId changedBy)
    {
        if (IsDecommissioned)
            return null;

        return new ParcelDecommissioned(Id.Value, changedBy.Value);
    }

    /// <summary>Recommissions a decommissioned parcel. Idempotent if already active.</summary>
    public ParcelRecommissioned? Recommission(UserId changedBy)
    {
        if (!IsDecommissioned)
            return null;

        return new ParcelRecommissioned(Id.Value, changedBy.Value);
    }

    /// <summary>Starts a land parcel from its first event.</summary>
    public static LandParcel Create(LandParcelRegistered e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var landParcel = new LandParcel();
        landParcel.Apply(e);
        return landParcel;
    }

    private void Apply(LandParcelRegistered e)
    {
        Id = LandParcelId.Create(e.AggregateId);
        LegalParcelNumber = e.LegalParcelNumber;
        Address = e.Address;
        AreaAcres = e.AreaAcres;
        AcquisitionDate = e.AcquisitionDate;
        AcquisitionMethod = e.AcquisitionMethod;
        CovenantTerms = e.CovenantTerms;
        RegisteredBy = UserId.Create(e.RegisteredBy);
    }

    /// <summary>Applies <see cref="ParcelDecommissioned"/>.</summary>
    public void Apply(ParcelDecommissioned _)
    {
        IsDecommissioned = true;
    }

    /// <summary>Applies <see cref="ParcelRecommissioned"/>.</summary>
    public void Apply(ParcelRecommissioned _)
    {
        IsDecommissioned = false;
    }
}
