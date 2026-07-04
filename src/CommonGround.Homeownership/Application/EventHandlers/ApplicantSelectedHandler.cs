using CommonGround.ApplicantSelection.Contracts.Events;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>Reacts to applicant selection by initiating a purchase workflow.</summary>
public static class ApplicantSelectedHandler
{
    private static readonly string[] TerminalPhaseNames =
    [
        WorkflowPhase.Closed.DisplayName,
        WorkflowPhase.Failed.DisplayName,
        WorkflowPhase.Withdrawn.DisplayName,
    ];

    /// <summary>Creates a purchase workflow when an applicant is selected for a home, unless an open workflow already exists for the application.</summary>
    public static async Task Handle(
        ApplicantSelected @event,
        IDocumentSession session,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var existingOpenWorkflow = await session.Query<PurchaseWorkflowView>()
            .Where(v => v.ApplicationId == @event.ApplicationId && !TerminalPhaseNames.Contains(v.Phase))
            .FirstOrDefaultAsync(ct).ConfigureAwait(false);

        if (existingOpenWorkflow is not null)
            return;

        var workflow = PurchaseWorkflow.Initiate(
            GroundLeaseTemplateId.Create(@event.GroundLeaseTemplateId),
            @event.LandParcelId,
            @event.HomeId,
            new Homeowner(@event.ApplicantFullName, @event.ApplicantEmail, @event.ApplicantPhone),
            @event.PurchasePrice,
            UserId.Create(@event.SelectedBy),
            @event.ApplicationId);

        session.CreateAggregate<PurchaseWorkflow, PurchaseWorkflowId>(workflow);
    }
}