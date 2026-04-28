using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.LandParcels;

/// <summary>Status of a land parcel in the CLT portfolio.</summary>
public sealed class ParcelStatus : Enumeration<ParcelStatus>
{
    /// <summary>Parcel has no active structure or occupant; ready for development.</summary>
    public static readonly ParcelStatus Available = new(1, "Available");

    /// <summary>Parcel is owned by the CLT but has no active structure or occupant; not yet ready for development.</summary>
    public static readonly ParcelStatus Predevelopment = new(2, "Predevelopment");

    /// <summary>Parcel is actively under construction or renovation.</summary>
    public static readonly ParcelStatus UnderConstruction = new(3, "UnderConstruction");

    /// <summary>Parcel has an active structure but is not yet occupied.</summary>
    public static readonly ParcelStatus ReadyForOccupancy = new(4, "ReadyForOccupancy");

    /// <summary>Parcel has an active ground lease with a homeowner.</summary>
    public static readonly ParcelStatus Occupied = new(5, "Occupied");

    /// <summary>Parcel is in transition between statuses, e.g. in the process of being sold or leased.</summary>
    public static readonly ParcelStatus Transitioning = new(6, "Transitioning");

    /// <summary>Parcel is permanently removed from the CLT portfolio, e.g. sold to another entity or repurposed for non-residential use.</summary>
    public static readonly ParcelStatus Decommissioned = new(7, "Decommissioned");

    private ParcelStatus(int value, string displayName) : base(value, displayName) { }
}
