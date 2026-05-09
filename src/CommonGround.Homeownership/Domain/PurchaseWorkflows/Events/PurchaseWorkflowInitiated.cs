using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>Raised when a new purchase workflow is initiated.</summary>
public sealed record PurchaseWorkflowInitiated(
    string AggregateId,
    string GroundLeaseTemplateId,
    string LandParcelId,
    string HomeId,
    string BuyerFullName,
    string BuyerEmail,
    string? BuyerPhone,
    decimal PurchasePrice,
    string InitiatedBy) : DomainEvent(AggregateId);
