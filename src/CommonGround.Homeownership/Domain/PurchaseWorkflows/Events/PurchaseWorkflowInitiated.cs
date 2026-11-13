using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>
/// Raised when a new purchase workflow is initiated. For a purchase started from a home selection,
/// <paramref name="DeclaredAnnualIncome"/> and <paramref name="DeclaredHouseholdSize"/> are what the household declared,
/// <paramref name="IncomeLimitPercentOfAmi"/> the home's limit the selection judged by (null when the home had none),
/// and <paramref name="IncomeLimitsEffectiveFrom"/> and <paramref name="IncomeLimitLevels"/> the schedule in effect
/// when the selection started (null when none was). All are null for a purchase not started from a selection.
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
    IReadOnlyList<IncomeLimitLevelEntry>? IncomeLimitLevels) : DomainEvent(AggregateId);
