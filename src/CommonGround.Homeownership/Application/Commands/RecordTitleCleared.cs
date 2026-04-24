using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

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
        IPurchaseWorkflowRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workflow = await repository.GetByIdAsync(command.WorkflowId, cancellationToken).ConfigureAwait(false)
            ?? throw new CommonGround.SharedKernel.Domain.Exceptions.AggregateNotFoundException(command.WorkflowId.Value);
        workflow.RecordTitleCleared(command.ClearedDate, command.TitleCompanyName, command.RecordedBy);

        repository.Stage(workflow);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
