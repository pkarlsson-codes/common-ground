using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

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
        IPurchaseWorkflowRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workflow = await repository.GetByIdAsync(command.WorkflowId, cancellationToken).ConfigureAwait(false)
            ?? throw new CommonGround.SharedKernel.Domain.Exceptions.AggregateNotFoundException(command.WorkflowId.Value);
        workflow.RecordEligibilityVerified(command.VerifiedDate, command.Notes, command.RecordedBy);

        repository.Stage(workflow);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
