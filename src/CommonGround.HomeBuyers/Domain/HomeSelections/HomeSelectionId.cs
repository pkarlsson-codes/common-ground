using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.HomeSelections;

/// <summary>Strongly-typed identifier for a <see cref="HomeSelection"/>.</summary>
/// <remarks>Initializes a new instance of <see cref="HomeSelectionId"/>.</remarks>
public sealed class HomeSelectionId(string value)
: EntityId<HomeSelectionId>(value, Prefix)
{
    private const string Prefix = "selection";
}
