using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Domain.DevelopmentProjects;

/// <summary>Repository for <see cref="DevelopmentProject"/> aggregates.</summary>
public interface IDevelopmentProjectRepository : IRepository<DevelopmentProject, DevelopmentProjectId>
{
}
