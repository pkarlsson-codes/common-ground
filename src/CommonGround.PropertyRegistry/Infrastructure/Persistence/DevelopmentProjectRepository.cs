using CommonGround.PropertyRegistry.Domain.DevelopmentProjects;
using CommonGround.SharedKernel.Persistence;
using Marten;
using Microsoft.Extensions.Logging;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence;

/// <summary>Marten-backed repository for <see cref="DevelopmentProject"/> aggregates.</summary>
internal sealed class DevelopmentProjectRepository(
    IDocumentSession session,
    ILogger<DevelopmentProjectRepository> logger)
    : MartenRepository<DevelopmentProject, DevelopmentProjectId>(session, logger),
    IDevelopmentProjectRepository
{
}
