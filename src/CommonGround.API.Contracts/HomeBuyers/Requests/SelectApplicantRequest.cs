namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Request to select an applicant for a home. LandParcelId is resolved server-side from the home.</summary>
public sealed record SelectApplicantRequest(
    string HomeId,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    DateOnly SelectedDate);