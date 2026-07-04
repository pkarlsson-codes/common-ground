using CommonGround.ApplicantSelection.Contracts.Events;
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
    /// <summary>Creates a purchase workflow when an applicant is selected for a home.</summary>
    public static void Handle(
        ApplicantSelected @event,
        IDocumentSession session)
    {
        ArgumentNullException.ThrowIfNull(@event);

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