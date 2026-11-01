using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Contracts.Events;

/// <summary>
/// Integration event published by the Homeownership BC when a purchase has closed and the ground lease it creates exists.
/// <paramref name="ApplicationId"/> and <paramref name="HouseholdId"/> are set when the purchase was started by
/// selecting an applicant for the home. Consumed by PropertyRegistry to mark the home leased, and by HomeBuyers
/// to record that the household bought its home.
/// </summary>
public sealed record PurchaseClosed(
    string PurchaseWorkflowId,
    string HomeId,
    string? ApplicationId,
    string? HouseholdId,
    DateOnly ClosingDate,
    string RecordedBy) : IntegrationEvent;
