using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.Homes;

/// <summary>Strongly-typed identifier for a <see cref="Home"/>.</summary>
/// <remarks>Initializes a new instance of <see cref="HomeId"/>.</remarks>
public sealed class HomeId(string value)
: EntityId<HomeId>(value, Prefix)
{
    private const string Prefix = "home";
}
