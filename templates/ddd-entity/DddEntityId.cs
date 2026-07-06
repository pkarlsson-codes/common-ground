using CommonGround.SharedKernel.Domain;

namespace DddNamespace;

/// <summary>Strongly-typed identifier for a <see cref="DddEntity"/>.</summary>
/// <remarks>Initializes a new instance of <see cref="DddEntityId"/>.</remarks>
public sealed class DddEntityId(string value)
: EntityId<DddEntityId>(value, Prefix)
{
    private const string Prefix = "dddprefix";
}
