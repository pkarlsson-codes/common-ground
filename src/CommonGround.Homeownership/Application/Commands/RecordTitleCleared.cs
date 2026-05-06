using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that the title has been cleared.</summary>
public sealed record RecordTitleCleared(
    PurchaseWorkflowId WorkflowId,
    DateOnly ClearedDate,
    string? TitleCompanyName,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordTitleCleared"/>.</summary>
public static class RecordTitleClearedHandler
{
    /// <summary>Loads the workflow, records title clearance, and persists.</summary>
    public static async Task Handle(
        RecordTitleCleared command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events.FetchForWriting<PurchaseWorkflow>(command.WorkflowId.Value, cancellationToken).ConfigureAwait(false);
        var workflow = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(PurchaseWorkflow), command.WorkflowId.Value);

        workflow.RecordTitleCleared(command.ClearedDate, command.TitleCompanyName, command.RecordedBy);

        workflow.ValidateInvariants();
        stream.AppendMany(workflow.UncommittedEvents.ToArray());
        workflow.UncommittedEvents.MarkAsCommitted();
    }
}
