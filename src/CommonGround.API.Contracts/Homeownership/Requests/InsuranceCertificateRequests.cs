using System.Text.Json.Serialization;

namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record a certificate of the homeowner's insurance on a lease (Art 9.4).</summary>
/// <param name="Insurer">The insurer.</param>
/// <param name="PolicyNumber">The policy number.</param>
/// <param name="PolicyStart">The first day of the policy period.</param>
/// <param name="PolicyEnd">The last day of the policy period; after the start.</param>
/// <param name="DwellingCoverage">The dwelling coverage; zero or more.</param>
/// <param name="LiabilityPerOccurrence">The liability coverage per occurrence; zero or more.</param>
/// <param name="TrustNamedAdditionalInsured">Whether the trust is named as additional insured.</param>
/// <param name="ReceivedOn">The day the trust received the certificate; not in the future.</param>
/// <param name="Notes">Notes, such as flood cover, or null for none.</param>
/// <param name="DocumentId">The certificate file already uploaded to Documents, or null when none was added.</param>
public sealed record RecordInsuranceCertificateRequest(
    string Insurer,
    string PolicyNumber,
    [property: JsonRequired] DateOnly PolicyStart,
    [property: JsonRequired] DateOnly PolicyEnd,
    [property: JsonRequired] decimal DwellingCoverage,
    [property: JsonRequired] decimal LiabilityPerOccurrence,
    [property: JsonRequired] bool TrustNamedAdditionalInsured,
    [property: JsonRequired] DateOnly ReceivedOn,
    [property: JsonRequired] string? Notes,
    [property: JsonRequired] string? DocumentId);

/// <summary>Request to remove an insurance certificate recorded in error.</summary>
/// <param name="Reason">Why it is removed.</param>
public sealed record RemoveInsuranceCertificateRequest(string Reason);
