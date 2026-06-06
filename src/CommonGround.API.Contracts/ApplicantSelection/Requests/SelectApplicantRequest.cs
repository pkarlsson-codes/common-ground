namespace CommonGround.API.Contracts.ApplicantSelection.Requests;

/// <summary>Request to select an applicant for a home.</summary>
public sealed record SelectApplicantRequest(
    string HomeId,
    string LandParcelId,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    string SelectedDate);