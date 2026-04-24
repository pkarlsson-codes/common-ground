using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that a home inspection has been completed.</summary>
public sealed record RecordInspectionCompleted(
    PurchaseWorkflowId WorkflowId,
    DateOnly InspectionDate,
    string InspectorName,
    string? Notes,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordInspectionCompleted"/>.</summary>
public static class RecordInspectionCompletedHandler
{
    /// <summary>Loads the workflow, records inspection completion, and persists.</summary>
    public static async Task Handle(
        RecordInspectionCompleted command,
        IPurchaseWorkflowRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workflow = await repository.GetByIdAsync(command.WorkflowId, cancellationToken).ConfigureAwait(false)
            ?? throw new CommonGround.SharedKernel.Domain.Exceptions.AggregateNotFoundException(command.WorkflowId.Value);
        workflow.RecordInspectionCompleted(command.InspectionDate, command.InspectorName, command.Notes, command.RecordedBy);

        repository.Stage(workflow);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
