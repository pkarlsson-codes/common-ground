using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.DevelopmentProjects;

/// <summary>Strongly-typed identifier for a DevelopmentProject.</summary>
/// <remarks>Initializes a new instance of <see cref="DevelopmentProjectId"/>.</remarks>
public sealed class DevelopmentProjectId(string value)
    : EntityId<DevelopmentProjectId>(value, Prefix)
{
    private const string Prefix = "project";
}
