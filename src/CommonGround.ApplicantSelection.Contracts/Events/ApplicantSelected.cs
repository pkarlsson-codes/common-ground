using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.ApplicantSelection.Contracts.Events;

/// <summary>
/// Integration event published when an applicant is selected for a specific home.
/// Consumed by Homeownership to initiate a PurchaseWorkflow.
/// </summary>
public sealed record ApplicantSelected(
    string ApplicationId,
    string ApplicantFullName,
    string ApplicantEmail,
    string? ApplicantPhone,
    string HomeId,
    string LandParcelId,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    DateOnly SelectedDate,
    string SelectedBy) : IntegrationEvent;