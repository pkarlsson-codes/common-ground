using CommonGround.SharedKernel.Domain;

namespace DddNamespace.DddPluralFolder;

/// <summary>Strongly-typed identifier for a <see cref="DddAggregate"/>.</summary>
/// <remarks>Initializes a new instance of <see cref="DddAggregateId"/>.</remarks>
public sealed class DddAggregateId(string value)
: EntityId<DddAggregateId>(value, Prefix)
{
    private const string Prefix = "dddprefix";
}
