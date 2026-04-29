using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.DevelopmentPlans;

/// <summary>Status of a development plan.</summary>
public sealed class PlanStatus : Enumeration<PlanStatus>
{
    /// <summary>Plan has been initiated but not yet activated.</summary>
    public static readonly PlanStatus Initiated = new(1, "Initiated");

    /// <summary>Plan is actively being executed.</summary>
    public static readonly PlanStatus Active = new(2, "Active");

    /// <summary>Plan has been completed.</summary>
    public static readonly PlanStatus Completed = new(3, "Completed");

    /// <summary>Plan has been abandoned.</summary>
    public static readonly PlanStatus Abandoned = new(4, "Abandoned");

    private PlanStatus(int value, string displayName) : base(value, displayName) { }
}
