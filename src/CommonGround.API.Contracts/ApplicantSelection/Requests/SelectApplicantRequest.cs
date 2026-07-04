namespace CommonGround.API.Contracts.ApplicantSelection.Requests;

/// <summary>Request to select an applicant for a home. LandParcelId is resolved server-side from the home.</summary>
public sealed record SelectApplicantRequest(
    string HomeId,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    string SelectedDate);