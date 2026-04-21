using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows;

/// <summary>Strongly-typed identifier for a purchase workflow.</summary>
/// <param name="value">The string value of the identifier.</param>
public sealed class PurchaseWorkflowId(string value)
    : EntityId<PurchaseWorkflowId>(value, Prefix)
{
    private const string Prefix = "purchase";
}
