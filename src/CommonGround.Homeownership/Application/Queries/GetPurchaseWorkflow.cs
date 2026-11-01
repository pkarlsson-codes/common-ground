using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Returns a single purchase workflow by ID.</summary>
public sealed record GetPurchaseWorkflow(PurchaseWorkflowId WorkflowId);

/// <summary>A purchase workflow and the label of the home being purchased.</summary>
public sealed record PurchaseWorkflowDetails(PurchaseWorkflowView Workflow, string? HomeLabel);

/// <summary>Handles <see cref="GetPurchaseWorkflow"/>.</summary>
public static class GetPurchaseWorkflowHandler
{
    /// <summary>Loads the purchase workflow view by ID, throwing if not found.</summary>
    public static async Task<PurchaseWorkflowDetails> Handle(
        GetPurchaseWorkflow request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var view = await session.LoadAsync<PurchaseWorkflowView>(request.WorkflowId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.WorkflowId.Value);

        var label = await session.LoadAsync<HomeLabelView>(view.HomeId, cancellationToken).ConfigureAwait(false);

        return new PurchaseWorkflowDetails(view, label?.Label);
    }
}
