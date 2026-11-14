using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records the buyer's signed disclosure acknowledgment on a purchase (Art 1).</summary>
/// <param name="WorkflowId">The purchase.</param>
/// <param name="AcknowledgedDate">The date the buyer signed.</param>
/// <param name="DocumentId">The signed acknowledgment in Documents.</param>
/// <param name="Notes">Optional notes.</param>
/// <param name="RecordedBy">The staff user recording it.</param>
public sealed record RecordDisclosureAcknowledged(
    PurchaseWorkflowId WorkflowId,
    DateOnly AcknowledgedDate,
    string DocumentId,
    string? Notes,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordDisclosureAcknowledged"/>.</summary>
public static class RecordDisclosureAcknowledgedHandler
{
    /// <summary>Fetches the workflow for writing and the trust's today.</summary>
    public static async Task<(IEventStream<PurchaseWorkflow> Workflow, DateOnly Today)> LoadAsync(
        RecordDisclosureAcknowledged command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var workflow = await session.Events
            .FetchForWriting<PurchaseWorkflow>(command.WorkflowId.Value, cancellationToken)
            .ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return (workflow, today);
    }

    /// <summary>Records the acknowledgment on the workflow.</summary>
    public static void Handle(RecordDisclosureAcknowledged command, IEventStream<PurchaseWorkflow> workflow, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(workflow);

        workflow.Decide(w => w.RecordDisclosureAcknowledged(
            command.AcknowledgedDate, command.DocumentId, command.Notes, today, command.RecordedBy));
    }
}
