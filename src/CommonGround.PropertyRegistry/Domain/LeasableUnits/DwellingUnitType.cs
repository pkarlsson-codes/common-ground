using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.LeasableUnits;

/// <summary>Physical type of a CLT dwelling unit.</summary>
public sealed class DwellingUnitType : Enumeration<DwellingUnitType>
{
    /// <summary>Single-family detached home.</summary>
    public static readonly DwellingUnitType SingleFamily = new(1, "SingleFamily");

    /// <summary>Condominium unit in a multi-unit building.</summary>
    public static readonly DwellingUnitType CondoUnit = new(2, "CondoUnit");

    /// <summary>Duplex — two attached units.</summary>
    public static readonly DwellingUnitType Duplex = new(3, "Duplex");

    /// <summary>Townhouse / row house.</summary>
    public static readonly DwellingUnitType Townhouse = new(4, "Townhouse");

    private DwellingUnitType(int value, string displayName) : base(value, displayName) { }
}
