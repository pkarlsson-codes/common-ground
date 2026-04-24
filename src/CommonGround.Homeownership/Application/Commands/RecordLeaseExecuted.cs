using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that the ground lease has been signed by both parties.</summary>
public sealed record RecordLeaseExecuted(
    PurchaseWorkflowId WorkflowId,
    DateOnly SignedDate,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordLeaseExecuted"/>.</summary>
public static class RecordLeaseExecutedHandler
{
    /// <summary>Loads the workflow, records lease execution, and persists.</summary>
    public static async Task Handle(
        RecordLeaseExecuted command,
        IPurchaseWorkflowRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workflow = await repository.GetByIdAsync(command.WorkflowId, cancellationToken).ConfigureAwait(false)
            ?? throw new CommonGround.SharedKernel.Domain.Exceptions.AggregateNotFoundException(command.WorkflowId.Value);
        workflow.RecordLeaseExecuted(command.SignedDate, command.RecordedBy);

        repository.Stage(workflow);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
