using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.LandParcels;

/// <summary>How the CLT acquired a land parcel.</summary>
public sealed class AcquisitionMethod : Enumeration<AcquisitionMethod>
{
    /// <summary>Land was purchased.</summary>
    public static readonly AcquisitionMethod Purchase = new(1, "Purchase");

    /// <summary>Land was donated to the CLT.</summary>
    public static readonly AcquisitionMethod Donation = new(2, "Donation");

    /// <summary>Land was transferred from another entity (e.g., municipality).</summary>
    public static readonly AcquisitionMethod Transfer = new(3, "Transfer");

    private AcquisitionMethod(int value, string displayName) : base(value, displayName) { }
}
