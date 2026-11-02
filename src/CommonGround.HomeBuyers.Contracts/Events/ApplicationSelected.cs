using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Contracts.Events;

/// <summary>
/// Integration event published when an applicant is selected for a specific home.
/// Consumed by Homeownership to initiate a PurchaseWorkflow, and by PropertyRegistry to show the home under contract.
/// </summary>
public sealed record ApplicationSelected(
    string ApplicationId,
    string HouseholdId,
    string ApplicantFullName,
    string ApplicantEmail,
    string? ApplicantPhone,
    string HomeSelectionId,
    string HomeId,
    string LandParcelId,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    DateOnly SelectedDate,
    string SelectedBy) : IntegrationEvent;