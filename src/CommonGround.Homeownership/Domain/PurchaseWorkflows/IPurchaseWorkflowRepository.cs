using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows;

/// <summary>Repository interface for purchase workflow aggregates.</summary>
public interface IPurchaseWorkflowRepository : IRepository<PurchaseWorkflow, PurchaseWorkflowId>;
