using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that a purchase has been closed.</summary>
public sealed record RecordPurchaseClosed(
    PurchaseWorkflowId WorkflowId,
    DateOnly ActualClosingDate,
    string? Notes,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordPurchaseClosed"/>.</summary>
public static class RecordPurchaseClosedHandler
{
    /// <summary>Handles the command.</summary>
    public static async Task Handle(
        RecordPurchaseClosed command,
        IPurchaseWorkflowRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workflow = await repository.GetByIdAsync(command.WorkflowId, cancellationToken).ConfigureAwait(false)
            ?? throw new CommonGround.SharedKernel.Domain.Exceptions.AggregateNotFoundException(command.WorkflowId.Value);
        workflow.RecordPurchaseClosed(command.ActualClosingDate, command.Notes, command.RecordedBy);

        repository.Stage(workflow);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
