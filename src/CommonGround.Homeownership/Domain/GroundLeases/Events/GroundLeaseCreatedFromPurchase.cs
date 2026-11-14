using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>
/// Raised when a new ground lease is created from a purchase workflow closing. The base price is the purchase price;
/// the original appraised value is the purchase's appraisal, if it recorded one. The purchase loan, from the
/// purchase's financing step, is the lease's first permitted mortgage (Model Lease Art 8.2); its fields are null
/// only for a purchase that closed without financing.
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
    Money LandUseFee,
    Money RepairReserveFee,
    decimal LateInterestRate,
    int InterestWaiverDays,
    int MaxMortgageDebtPercent,
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
    string? BuyerPhone,
    string? PurchaseMortgageId,
    string? PurchaseLender,
    Money? PurchaseLoanPrincipal,
    Money? RequiredLiabilityPerOccurrence) : DomainEvent(AggregateId);
