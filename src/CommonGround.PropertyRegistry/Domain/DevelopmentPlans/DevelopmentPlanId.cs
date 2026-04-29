using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.DevelopmentPlans;

/// <summary>Strongly-typed identifier for a DevelopmentPlan.</summary>
/// <remarks>Initializes a new instance of <see cref="DevelopmentPlanId"/>.</remarks>
public sealed class DevelopmentPlanId(string value)
    : EntityId<DevelopmentPlanId>(value, Prefix)
{
    private const string Prefix = "plan";
}
