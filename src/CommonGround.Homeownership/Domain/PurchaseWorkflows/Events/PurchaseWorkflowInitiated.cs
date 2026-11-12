using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>
/// Raised when a new purchase workflow is initiated. For a purchase started from a home selection,
/// <paramref name="DeclaredAnnualIncome"/> and <paramref name="DeclaredHouseholdSize"/> are what the household declared,
/// and <paramref name="IncomeLimitPercentOfAmi"/>, <paramref name="IncomeLimitsEffectiveFrom"/> and
/// <paramref name="IncomeLimits"/> (1 to 8 people) the limit the selection judged by. All are null otherwise; the
/// limit fields are also null when the home had no limit.
/// </summary>
public sealed record PurchaseWorkflowInitiated(
    string AggregateId,
    string GroundLeaseTemplateId,
    string LandParcelId,
    string HomeId,
    string BuyerFullName,
    string BuyerEmail,
    string? BuyerPhone,
    decimal PurchasePrice,
    string InitiatedBy,
    string? ApplicationId,
    string? HouseholdId,
    Money? DeclaredAnnualIncome,
    int? DeclaredHouseholdSize,
    int? IncomeLimitPercentOfAmi,
    DateOnly? IncomeLimitsEffectiveFrom,
    IReadOnlyList<Money>? IncomeLimits) : DomainEvent(AggregateId);
