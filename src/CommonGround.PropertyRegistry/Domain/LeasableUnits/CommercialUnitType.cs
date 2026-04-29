using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.LeasableUnits;

/// <summary>Physical type of a CLT commercial unit.</summary>
public sealed class CommercialUnitType : Enumeration<CommercialUnitType>
{
    /// <summary>Retail commercial space.</summary>
    public static readonly CommercialUnitType Retail = new(1, "Retail");

    private CommercialUnitType(int value, string displayName) : base(value, displayName) { }
}
