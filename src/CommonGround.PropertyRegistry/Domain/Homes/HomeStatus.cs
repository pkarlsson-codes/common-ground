using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.Homes;

/// <summary>Occupancy lifecycle status of a dwelling unit.</summary>
public sealed class HomeStatus : Enumeration<HomeStatus>
{
    /// <summary>Unit is vacant and available for a new homeowner.</summary>
    public static readonly HomeStatus Available = new(1, "Available");

    /// <summary>Unit has an executed ground lease but move-in is not yet confirmed.</summary>
    public static readonly HomeStatus PendingOccupancy = new(2, "PendingOccupancy");

    /// <summary>Unit is occupied by a homeowner under an active ground lease.</summary>
    public static readonly HomeStatus Occupied = new(3, "Occupied");

    /// <summary>Unit is being offered for resale through the CLT resale process.</summary>
    public static readonly HomeStatus Resale = new(4, "Resale");

    private HomeStatus(int value, string displayName) : base(value, displayName) { }
}
