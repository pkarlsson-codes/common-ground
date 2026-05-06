using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that buyer eligibility has been verified.</summary>
public sealed record RecordEligibilityVerified(
    PurchaseWorkflowId WorkflowId,
    DateOnly VerifiedDate,
    string? Notes,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordEligibilityVerified"/>.</summary>
public static class RecordEligibilityVerifiedHandler
{
    /// <summary>Loads the workflow, records eligibility verification, and persists.</summary>
    public static async Task Handle(
        RecordEligibilityVerified command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events.FetchForWriting<PurchaseWorkflow>(command.WorkflowId.Value, cancellationToken).ConfigureAwait(false);
        var workflow = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(PurchaseWorkflow), command.WorkflowId.Value);

        workflow.RecordEligibilityVerified(command.VerifiedDate, command.Notes, command.RecordedBy);

        workflow.ValidateInvariants();
        stream.AppendMany(workflow.UncommittedEvents.ToArray());
        workflow.UncommittedEvents.MarkAsCommitted();
    }
}
