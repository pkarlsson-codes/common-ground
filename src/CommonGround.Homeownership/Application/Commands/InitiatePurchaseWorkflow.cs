using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Initiates a new purchase workflow for a ground lease.</summary>
public sealed record InitiatePurchaseWorkflow(
    PurchaseWorkflowId WorkflowId,
    GroundLeaseTemplateId GroundLeaseTemplateId,
    string LandParcelId,
    string HomeId,
    Homeowner Buyer,
    decimal PurchasePrice,
    UserId InitiatedBy);

/// <summary>Handles <see cref="InitiatePurchaseWorkflow"/>.</summary>
public static class InitiatePurchaseWorkflowHandler
{
    /// <summary>Starts the new stream.</summary>
    public static IStartStream Handle(InitiatePurchaseWorkflow command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var started = PurchaseWorkflow.Initiate(
            command.WorkflowId,
            command.GroundLeaseTemplateId,
            command.LandParcelId,
            command.HomeId,
            command.Buyer,
            command.PurchasePrice,
            command.InitiatedBy);

        return MartenOps.StartStream<PurchaseWorkflow>(command.WorkflowId.Value, started);
    }
}
