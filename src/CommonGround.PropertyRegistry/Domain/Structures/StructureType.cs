using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.Structures;

/// <summary>Physical type of a CLT structure.</summary>
public sealed class StructureType : Enumeration<StructureType>
{
    /// <summary>Single-family detached home.</summary>
    public static readonly StructureType SingleFamily = new(1, "SingleFamily");

    /// <summary>Condominium unit in a multi-unit building.</summary>
    public static readonly StructureType CondoUnit = new(2, "CondoUnit");

    /// <summary>Duplex — two attached units.</summary>
    public static readonly StructureType Duplex = new(3, "Duplex");

    /// <summary>Townhouse / row house.</summary>
    public static readonly StructureType Townhouse = new(4, "Townhouse");

    private StructureType(int value, string displayName) : base(value, displayName) { }
}
