using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.LandParcels;

/// <summary>Status of a land parcel in the CLT portfolio.</summary>
public sealed class ParcelStatus : Enumeration<ParcelStatus>
{
    /// <summary>Parcel has no active structure or occupant; ready for development.</summary>
    public static readonly ParcelStatus Available = new(1, "Available");

    /// <summary>Parcel has an active ground lease with a homeowner.</summary>
    public static readonly ParcelStatus Occupied = new(2, "Occupied");

    /// <summary>Parcel is actively under construction or renovation.</summary>
    public static readonly ParcelStatus UnderDevelopment = new(3, "UnderDevelopment");

    private ParcelStatus(int value, string displayName) : base(value, displayName) { }
}
