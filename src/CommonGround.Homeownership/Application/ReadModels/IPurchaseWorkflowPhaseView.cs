namespace CommonGround.Homeownership.Application.ReadModels;

public interface IPurchaseWorkflowPhaseView
{
    /// <summary>The <see cref="Domain.PurchaseWorkflows.WorkflowPhase"/> display name.</summary>
    string Phase { get; }
}
