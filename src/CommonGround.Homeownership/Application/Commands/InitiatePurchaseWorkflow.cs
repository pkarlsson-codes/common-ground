using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Initiates a new purchase workflow for a ground lease.</summary>
public sealed record InitiatePurchaseWorkflow(
    GroundLeaseId GroundLeaseId,
    Homeowner Buyer,
    decimal PurchasePrice,
    UserId InitiatedBy);

/// <summary>Handles <see cref="InitiatePurchaseWorkflow"/>.</summary>
public static class InitiatePurchaseWorkflowHandler
{
    /// <summary>Initiates a new purchase workflow and returns its ID.</summary>
    public static async Task<PurchaseWorkflowId> Handle(
        InitiatePurchaseWorkflow command,
        IPurchaseWorkflowRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workflow = PurchaseWorkflow.Initiate(
            command.GroundLeaseId,
            command.Buyer,
            command.PurchasePrice,
            command.InitiatedBy);

        repository.Stage(workflow);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return workflow.Id;
    }
}
