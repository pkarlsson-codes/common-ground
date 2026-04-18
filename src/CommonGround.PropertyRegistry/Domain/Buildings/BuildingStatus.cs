using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.Buildings;

/// <summary>Occupancy and condition status of a building.</summary>
public sealed class BuildingStatus : Enumeration<BuildingStatus>
{
    /// <summary>Building is vacant and available for a new homeowner.</summary>
    public static readonly BuildingStatus Available = new(1, "Available");

    /// <summary>Building has an active ground lease.</summary>
    public static readonly BuildingStatus Occupied = new(2, "Occupied");

    /// <summary>Building is undergoing renovation and not available.</summary>
    public static readonly BuildingStatus UnderRenovation = new(3, "UnderRenovation");

    private BuildingStatus(int value, string displayName) : base(value, displayName) { }
}