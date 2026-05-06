using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

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
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events.FetchForWriting<PurchaseWorkflow>(command.WorkflowId.Value, cancellationToken).ConfigureAwait(false);
        var workflow = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(PurchaseWorkflow), command.WorkflowId.Value);

        workflow.RecordPurchaseClosed(command.ActualClosingDate, command.Notes, command.RecordedBy);

        workflow.ValidateInvariants();
        stream.AppendMany(workflow.UncommittedEvents.ToArray());
        workflow.UncommittedEvents.MarkAsCommitted();
    }
}
