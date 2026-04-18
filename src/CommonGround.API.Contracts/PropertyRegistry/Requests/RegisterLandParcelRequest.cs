namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Request to register a new land parcel.</summary>
public sealed record RegisterLandParcelRequest(
    string LegalParcelNumber,
    string Address,
    decimal AreaAcres,
    DateOnly AcquisitionDate,
    string AcquisitionMethod,
    string? CovenantTerms);
