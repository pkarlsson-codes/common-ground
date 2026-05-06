using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

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
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events.FetchForWriting<PurchaseWorkflow>(command.WorkflowId.Value, cancellationToken).ConfigureAwait(false);
        var workflow = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(PurchaseWorkflow), command.WorkflowId.Value);

        workflow.StallWorkflow(command.StalledReason, command.StalledDate, command.ResolutionTargetDate, command.RecordedBy);

        workflow.ValidateInvariants();
        stream.AppendMany(workflow.UncommittedEvents.ToArray());
        workflow.UncommittedEvents.MarkAsCommitted();
    }
}
