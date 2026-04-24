using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that a buyer has withdrawn from a purchase.</summary>
public sealed record WithdrawPurchase(
    PurchaseWorkflowId WorkflowId,
    DateOnly WithdrawnDate,
    string? Reason,
    UserId RecordedBy);

/// <summary>Handles <see cref="WithdrawPurchase"/>.</summary>
public static class WithdrawPurchaseHandler
{
    /// <summary>Loads the workflow, records buyer withdrawal, and persists.</summary>
    public static async Task Handle(
        WithdrawPurchase command,
        IPurchaseWorkflowRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workflow = await repository.GetByIdAsync(command.WorkflowId, cancellationToken).ConfigureAwait(false)
            ?? throw new CommonGround.SharedKernel.Domain.Exceptions.AggregateNotFoundException(command.WorkflowId.Value);
        workflow.WithdrawPurchase(command.WithdrawnDate, command.Reason, command.RecordedBy);

        repository.Stage(workflow);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
