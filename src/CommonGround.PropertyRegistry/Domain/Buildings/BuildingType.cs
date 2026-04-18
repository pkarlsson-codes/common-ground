using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.Buildings;

/// <summary>Physical type of a CLT building.</summary>
public sealed class BuildingType : Enumeration<BuildingType>
{
    /// <summary>Single-family detached home.</summary>
    public static readonly BuildingType SingleFamily = new(1, "SingleFamily");

    /// <summary>Condominium unit in a multi-unit building.</summary>
    public static readonly BuildingType CondoUnit = new(2, "CondoUnit");

    /// <summary>Duplex — two attached units.</summary>
    public static readonly BuildingType Duplex = new(3, "Duplex");

    /// <summary>Townhouse / row house.</summary>
    public static readonly BuildingType Townhouse = new(4, "Townhouse");

    private BuildingType(int value, string displayName) : base(value, displayName) { }
}
