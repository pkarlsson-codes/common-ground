using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

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
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workflow = await session
            .LoadAggregateAsync<PurchaseWorkflow>(
                command.WorkflowId,
                cancellationToken)
            .ConfigureAwait(false);

        workflow.FailPurchase(command.FailureReason, command.FailedDate, command.RecordedBy);

        session.UpdateAggregate(workflow);
    }
}
