using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
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

        var workflow = await session
            .LoadAggregateAsync<PurchaseWorkflow>(
                command.WorkflowId,
                cancellationToken)
            .ConfigureAwait(false);

        workflow.RecordEligibilityVerified(command.VerifiedDate, command.Notes, command.RecordedBy);

        session.UpdateAggregate(workflow);
    }
}
