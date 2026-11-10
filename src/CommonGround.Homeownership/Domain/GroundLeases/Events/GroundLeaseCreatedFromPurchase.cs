using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>
/// Raised when a new ground lease is created from a purchase workflow closing. The base price is the purchase price;
/// the original appraised value is the purchase's appraisal, if it recorded one.
/// </summary>
public sealed record GroundLeaseCreatedFromPurchase(
    string AggregateId,
    string PurchaseWorkflowId,
    string GroundLeaseTemplateId,
    string LandParcelId,
    string HomeId,
    DateOnly LeaseStartDate,
    int TermYears,
    int OptionWindowDays,
    decimal LandUseFee,
    decimal RepairReserveFee,
    decimal BasePrice,
    decimal? OriginalAppraisedValue,
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
