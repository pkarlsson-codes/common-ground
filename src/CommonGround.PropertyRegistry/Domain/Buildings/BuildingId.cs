using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.Buildings;

/// <summary>Strongly-typed identifier for a Building.</summary>
/// <remarks>Initializes a new instance of <see cref="BuildingId"/>.</remarks>
public sealed class BuildingId(string value)
: EntityId<BuildingId>(value, Prefix)
{
    private const string Prefix = "building";
}
