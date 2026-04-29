using CommonGround.PropertyRegistry.Domain.DevelopmentPlans.Events;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.PropertyRegistry.Domain.DevelopmentPlans;

/// <summary>
/// A development plan associated with a CLT land parcel.
/// Tracks the intent and status of development activity on a parcel.
/// </summary>
public sealed class DevelopmentPlan : AggregateRoot<DevelopmentPlanId>
{
#pragma warning disable CS8618
    private DevelopmentPlan() { }
#pragma warning restore CS8618

    /// <summary>Gets the ID of the land parcel this plan is for.</summary>
    public LandParcelId LandParcelId { get; private set; }

    /// <summary>Gets the description of the development plan.</summary>
    public string Description { get; private set; }

    /// <summary>Gets the current status of this plan.</summary>
    public PlanStatus PlanStatus { get; private set; }

    /// <summary>Initiates a new development plan for a land parcel.</summary>
    public static DevelopmentPlan Initiate(
        LandParcelId landParcelId,
        string description,
        UserId initiatedBy)
    {
        ArgumentNullException.ThrowIfNull(landParcelId);
        ArgumentNullException.ThrowIfNull(initiatedBy);

        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleViolationException("Development plan description must be specified.");

        var plan = new DevelopmentPlan();

        plan.ApplyChange(new DevelopmentPlanInitiated(
            DevelopmentPlanId.Generate().Value,
            landParcelId.Value,
            description,
            initiatedBy.Value));

        return plan;
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case DevelopmentPlanInitiated e: Apply(e); break;
            default: throw new InvalidOperationException($"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(DevelopmentPlanInitiated e)
    {
        Id = DevelopmentPlanId.Create(e.AggregateId);
        LandParcelId = LandParcelId.Create(e.LandParcelId);
        Description = e.Description;
        PlanStatus = PlanStatus.Initiated;
    }
}
