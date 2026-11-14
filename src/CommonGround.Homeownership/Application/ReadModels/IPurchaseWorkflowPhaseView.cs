namespace CommonGround.Homeownership.Application.ReadModels;

/// <summary>A purchase workflow read model that stores the workflow's phase.</summary>
public interface IPurchaseWorkflowPhaseView
{
    /// <summary>The <see cref="Domain.PurchaseWorkflows.WorkflowPhase"/> display name.</summary>
    string Phase { get; }
}
