using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that a purchase workflow is stalled.</summary>
public sealed record StallWorkflow(
    PurchaseWorkflowId WorkflowId,
    string StalledReason,
    DateOnly StalledDate,
    DateOnly ResolutionTargetDate,
    UserId RecordedBy);

/// <summary>Handles <see cref="StallWorkflow"/>.</summary>
public static class StallWorkflowHandler
{
    /// <summary>Loads the workflow, marks it stalled, and persists.</summary>
    public static async Task Handle(
        StallWorkflow command,
        IPurchaseWorkflowRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workflow = await repository.GetByIdAsync(command.WorkflowId, cancellationToken).ConfigureAwait(false)
            ?? throw new CommonGround.SharedKernel.Domain.Exceptions.AggregateNotFoundException(command.WorkflowId.Value);
        workflow.StallWorkflow(command.StalledReason, command.StalledDate, command.ResolutionTargetDate, command.RecordedBy);

        repository.Stage(workflow);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
