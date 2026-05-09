using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

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
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workflow = await session
            .LoadAggregateAsync<PurchaseWorkflow>(
                command.WorkflowId,
                cancellationToken)
            .ConfigureAwait(false);

        workflow.RecordLeaseExecuted(command.SignedDate, command.RecordedBy);

        session.UpdateAggregate(workflow);
    }
}
