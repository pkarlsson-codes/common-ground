using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

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
    public static TitleCleared? Handle(RecordTitleCleared command, [WriteAggregate(nameof(RecordTitleCleared.WorkflowId))] PurchaseWorkflow workflow)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(workflow);

        return workflow.RecordTitleCleared(command.ClearedDate, command.TitleCompanyName, command.RecordedBy);
    }
}
