using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.Homes;

/// <summary>Physical type of a CLT home.</summary>
public sealed class HomeType : Enumeration<HomeType>
{
    /// <summary>Single-family detached home.</summary>
    public static readonly HomeType SingleFamily = new(1, "SingleFamily");

    /// <summary>Condominium unit in a multi-unit building.</summary>
    public static readonly HomeType CondoUnit = new(2, "CondoUnit");

    /// <summary>Duplex — two attached units.</summary>
    public static readonly HomeType Duplex = new(3, "Duplex");

    /// <summary>Townhouse / row house.</summary>
    public static readonly HomeType Townhouse = new(4, "Townhouse");

    private HomeType(int value, string displayName) : base(value, displayName) { }
}
