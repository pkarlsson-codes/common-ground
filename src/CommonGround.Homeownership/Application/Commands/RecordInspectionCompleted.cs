using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

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
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workflow = await session
            .LoadAggregateAsync<PurchaseWorkflow>(
                command.WorkflowId,
                cancellationToken)
            .ConfigureAwait(false);

        workflow.RecordInspectionCompleted(
            command.InspectionDate,
            command.InspectorName,
            command.Notes,
            command.RecordedBy);

        session.UpdateAggregate(workflow);
    }
}
