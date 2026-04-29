using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.LeasableUnits;

/// <summary>Strongly-typed identifier for a LeasableUnit.</summary>
/// <remarks>Initializes a new instance of <see cref="LeasableUnitId"/>.</remarks>
public sealed class LeasableUnitId(string value)
: EntityId<LeasableUnitId>(value, Prefix)
{
    private const string Prefix = "unit";
}
