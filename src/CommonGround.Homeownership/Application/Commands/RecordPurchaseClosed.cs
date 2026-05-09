using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
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

        var workflow = await session
            .LoadAggregateAsync<PurchaseWorkflow>(
                command.WorkflowId,
                cancellationToken)
            .ConfigureAwait(false);

        workflow.RecordPurchaseClosed(command.ActualClosingDate, command.Notes, command.RecordedBy);

        session.UpdateAggregate(workflow);
    }
}
