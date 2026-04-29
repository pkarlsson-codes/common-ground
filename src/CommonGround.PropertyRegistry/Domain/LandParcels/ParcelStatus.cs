using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.LandParcels;

/// <summary>Status of a land parcel in the CLT portfolio.</summary>
public sealed class ParcelStatus : Enumeration<ParcelStatus>
{
    /// <summary>Parcel has no active structure or occupant; ready for development.</summary>
    public static readonly ParcelStatus Registered = new(1, nameof(Registered));

    /// <summary>Parcel is owned by the CLT but has no active structure or occupant; not yet ready for development.</summary>
    public static readonly ParcelStatus Predevelopment = new(2, nameof(Predevelopment));

    /// <summary>Parcel is actively under construction or renovation.</summary>
    public static readonly ParcelStatus UnderConstruction = new(3, nameof(UnderConstruction));

    /// <summary>Parcel has an active structure but is not yet occupied.</summary>
    public static readonly ParcelStatus Available = new(4, nameof(Available));

    /// <summary>Parcel has an active ground lease with a homeowner.</summary>
    public static readonly ParcelStatus Occupied = new(5, nameof(Occupied));

    /// <summary>Parcel is in transition between statuses, e.g. in the process of being sold or leased.</summary>
    public static readonly ParcelStatus Resale = new(6, nameof(Resale));

    /// <summary>Parcel is pending occupancy after construction resale.</summary>
    public static readonly ParcelStatus PendingOccupancy = new(7, nameof(PendingOccupancy));

    /// <summary>Parcel is permanently removed from the CLT portfolio, e.g. sold to another entity or repurposed for non-residential use.</summary>
    public static readonly ParcelStatus Decommissioned = new(8, nameof(Decommissioned));

    private ParcelStatus(int value, string displayName) : base(value, displayName) { }
}
