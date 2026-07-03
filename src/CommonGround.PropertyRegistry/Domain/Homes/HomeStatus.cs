using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.Homes;

/// <summary>Occupancy lifecycle status of a home.</summary>
public sealed class HomeStatus : Enumeration<HomeStatus>
{
    /// <summary>Home is vacant and available for a new homeowner.</summary>
    public static readonly HomeStatus Available = new(1, "Available");

    /// <summary>Home has an executed ground lease but move-in is not yet confirmed.</summary>
    public static readonly HomeStatus PendingOccupancy = new(2, "PendingOccupancy");

    /// <summary>Home is occupied by a homeowner under an active ground lease.</summary>
    public static readonly HomeStatus Occupied = new(3, "Occupied");

    /// <summary>Home is being offered for resale through the CLT resale process.</summary>
    public static readonly HomeStatus Resale = new(4, "Resale");

    /// <summary>Home has a pending sale and is under contract to a new homeowner.</summary>
    public static readonly HomeStatus UnderContract = new(5, "UnderContract");

    private HomeStatus(int value, string displayName) : base(value, displayName) { }
}
