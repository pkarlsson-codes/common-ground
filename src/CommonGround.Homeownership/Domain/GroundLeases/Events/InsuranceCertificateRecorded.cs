using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when staff record a certificate of the homeowner's insurance on a lease (Art 9.4).</summary>
/// <param name="PolicyStart">The first day of the policy period.</param>
/// <param name="PolicyEnd">The last day of the policy period.</param>
/// <param name="ReceivedOn">The day the trust received the certificate.</param>
/// <param name="Notes">Notes, such as flood cover, or null for none.</param>
/// <param name="DocumentId">The certificate file in Documents, or null when none was added.</param>
public sealed record InsuranceCertificateRecorded(
    string AggregateId,
    string CertificateId,
    string Insurer,
    string PolicyNumber,
    DateOnly PolicyStart,
    DateOnly PolicyEnd,
    Money DwellingCoverage,
    Money LiabilityPerOccurrence,
    bool TrustNamedAdditionalInsured,
    DateOnly ReceivedOn,
    string? Notes,
    string? DocumentId,
    string RecordedBy) : DomainEvent(AggregateId);
