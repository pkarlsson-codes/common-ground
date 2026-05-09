using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when a new ground lease is created from a purchase workflow closing.</summary>
public sealed record GroundLeaseCreatedFromPurchase(
    string AggregateId,
    string PurchaseWorkflowId,
    string GroundLeaseTemplateId,
    string LandParcelId,
    string HomeId,
    DateOnly LeaseStartDate,
    int TermYears,
    string CovenantTerms,
    FormulaKind FormulaKind,
    decimal? AppreciationShare,
    string? IndexType,
    decimal? BaselineIndexValue,
    decimal? AnnualPercentage,
    decimal? ImprovementsCap,
    string RegisteredBy,
    string BuyerFullName,
    string BuyerEmail,
    string? BuyerPhone) : DomainEvent(AggregateId);
