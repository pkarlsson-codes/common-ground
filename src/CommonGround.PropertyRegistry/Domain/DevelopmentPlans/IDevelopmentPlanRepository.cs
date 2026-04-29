using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Domain.DevelopmentPlans;

/// <summary>Repository for <see cref="DevelopmentPlan"/> aggregates.</summary>
public interface IDevelopmentPlanRepository : IRepository<DevelopmentPlan, DevelopmentPlanId>
{
}
