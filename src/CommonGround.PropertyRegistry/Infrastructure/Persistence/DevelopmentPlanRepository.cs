using CommonGround.PropertyRegistry.Domain.DevelopmentPlans;
using CommonGround.SharedKernel.Persistence;
using Marten;
using Microsoft.Extensions.Logging;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence;

/// <summary>Marten-backed repository for <see cref="DevelopmentPlan"/> aggregates.</summary>
internal sealed class DevelopmentPlanRepository(
    IDocumentSession session,
    ILogger<DevelopmentPlanRepository> logger)
    : MartenRepository<DevelopmentPlan, DevelopmentPlanId>(session, logger),
    IDevelopmentPlanRepository
{
}
