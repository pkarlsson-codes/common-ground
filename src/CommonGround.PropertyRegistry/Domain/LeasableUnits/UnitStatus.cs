using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.LeasableUnits;

/// <summary>Occupancy lifecycle status of a dwelling unit.</summary>
public sealed class UnitStatus : Enumeration<UnitStatus>
{
    /// <summary>Unit is vacant and available for a new homeowner.</summary>
    public static readonly UnitStatus Available = new(1, "Available");

    /// <summary>Unit has an executed ground lease but move-in is not yet confirmed.</summary>
    public static readonly UnitStatus PendingOccupancy = new(2, "PendingOccupancy");

    /// <summary>Unit is occupied by a homeowner under an active ground lease.</summary>
    public static readonly UnitStatus Occupied = new(3, "Occupied");

    /// <summary>Unit is being offered for resale through the CLT resale process.</summary>
    public static readonly UnitStatus Resale = new(4, "Resale");

    private UnitStatus(int value, string displayName) : base(value, displayName) { }
}
