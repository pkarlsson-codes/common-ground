using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.Structures;

/// <summary>Occupancy and condition status of a structure.</summary>
public sealed class StructureStatus : Enumeration<StructureStatus>
{
    /// <summary>Structure is vacant and available for a new homeowner.</summary>
    public static readonly StructureStatus Available = new(1, "Available");

    /// <summary>Structure has an active ground lease.</summary>
    public static readonly StructureStatus Occupied = new(2, "Occupied");

    /// <summary>Structure is undergoing renovation and not available.</summary>
    public static readonly StructureStatus UnderRenovation = new(3, "UnderRenovation");

    private StructureStatus(int value, string displayName) : base(value, displayName) { }
}