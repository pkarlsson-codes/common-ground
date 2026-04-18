using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.Structures;

/// <summary>Strongly-typed identifier for a Structure.</summary>
/// <remarks>Initializes a new instance of <see cref="StructureId"/>.</remarks>
public sealed class StructureId(string value)
: EntityId<StructureId>(value, Prefix)
{
    private const string Prefix = "structure";
}
