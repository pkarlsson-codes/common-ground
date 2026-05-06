using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

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
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events.FetchForWriting<PurchaseWorkflow>(command.WorkflowId.Value, cancellationToken).ConfigureAwait(false);
        var workflow = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(PurchaseWorkflow), command.WorkflowId.Value);

        workflow.WithdrawPurchase(command.WithdrawnDate, command.Reason, command.RecordedBy);

        workflow.ValidateInvariants();
        stream.AppendMany(workflow.UncommittedEvents.ToArray());
        workflow.UncommittedEvents.MarkAsCommitted();
    }
}
