using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that a purchase has failed.</summary>
public sealed record FailPurchase(
    PurchaseWorkflowId WorkflowId,
    string FailureReason,
    DateOnly FailedDate,
    UserId RecordedBy);

/// <summary>Handles <see cref="FailPurchase"/>.</summary>
public static class FailPurchaseHandler
{
    /// <summary>Loads the workflow, records the failure, and persists.</summary>
    public static async Task Handle(
        FailPurchase command,
        IPurchaseWorkflowRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workflow = await repository.GetByIdAsync(command.WorkflowId, cancellationToken).ConfigureAwait(false)
            ?? throw new CommonGround.SharedKernel.Domain.Exceptions.AggregateNotFoundException(command.WorkflowId.Value);
        workflow.FailPurchase(command.FailureReason, command.FailedDate, command.RecordedBy);

        repository.Stage(workflow);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
