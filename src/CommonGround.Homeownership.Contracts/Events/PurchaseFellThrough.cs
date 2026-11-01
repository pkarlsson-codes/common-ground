using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Contracts.Events;

/// <summary>
/// Integration event published by the Homeownership BC when a purchase fails or the buyer withdraws.
/// <paramref name="ApplicationId"/> and <paramref name="HouseholdId"/> are set when the purchase was started by
/// selecting an applicant for the home. Consumed by PropertyRegistry to make the home available again, and by
/// HomeBuyers to return the household to the waiting list and start a new selection for the home.
/// </summary>
public sealed record PurchaseFellThrough(
    string PurchaseWorkflowId,
    string HomeId,
    string? ApplicationId,
    string? HouseholdId,
    string Reason,
    DateOnly FellThroughDate,
    string RecordedBy) : IntegrationEvent;
