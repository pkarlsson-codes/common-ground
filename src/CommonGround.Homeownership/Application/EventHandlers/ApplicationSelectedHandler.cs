using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Specifications;
using Marten;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>Reacts to a household being chosen for a home by initiating a purchase workflow.</summary>
public static class ApplicationSelectedHandler
{

    /// <summary>Finds the application's purchase workflow that is still open, if any.</summary>
    public static Task<PurchaseWorkflowView?> LoadAsync(ApplicationSelected @event, IQuerySession session, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(session);

        return session.Query<PurchaseWorkflowView>()
            .Where(v => v.ApplicationId == @event.ApplicationId)
            .Matching(PurchaseWorkflowInProgress<PurchaseWorkflowView>.Instance)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Creates a purchase workflow when an applicant is selected for a home, unless an open workflow already
    /// exists for the application.
    /// </summary>
    public static IMartenOp Handle(ApplicationSelected @event, PurchaseWorkflowView? existingOpenWorkflow)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (existingOpenWorkflow is not null)
            return MartenOps.Nothing();

        var workflowId = PurchaseWorkflowId.Generate();
        var initiated = PurchaseWorkflow.Initiate(
            workflowId,
            GroundLeaseTemplateId.Create(@event.GroundLeaseTemplateId),
            @event.LandParcelId,
            @event.HomeId,
            Homeowner.From(@event.ApplicantFullName, @event.ApplicantEmail, @event.ApplicantPhone),
            @event.PurchasePrice,
            UserId.Create(@event.SelectedBy),
            @event.ApplicationId,
            @event.HouseholdId,
            Money.Create(@event.AnnualIncome),
            @event.HouseholdSize,
            @event.IncomeLimitPercentOfAmi,
            @event.IncomeLimitsEffectiveFrom is { } effectiveFrom && @event.IncomeLimitLevels is { } levels
                ? IncomeLimitTable.Create(
                    effectiveFrom,
                    [.. levels.Select(level => IncomeLimitLevel.Create(level.PercentOfAmi, [.. level.Limits.Select(Money.Create)]))])
                : null);

        return MartenOps.StartStream<PurchaseWorkflow>(workflowId.Value, initiated);
    }
}
