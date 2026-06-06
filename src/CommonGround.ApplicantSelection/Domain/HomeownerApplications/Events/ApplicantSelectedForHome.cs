using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.ApplicantSelection.Domain.HomeownerApplications.Events;

/// <summary>Applicant selected for home event.</summary>
public sealed record ApplicantSelectedForHome(
    string AggregateId,
    string ApplicantFullName,
    string ApplicantEmail,
    string? ApplicantPhone,
    string HomeId,
    string LandParcelId,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    DateOnly SelectedDate,
    string SelectedBy) : DomainEvent(AggregateId);
