namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>Response after recording an insurance certificate.</summary>
/// <param name="CertificateId">The new certificate's ID.</param>
public sealed record RecordInsuranceCertificateResponse(string CertificateId);
