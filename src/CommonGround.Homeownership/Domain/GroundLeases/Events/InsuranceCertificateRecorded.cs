using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when staff record a certificate of the homeowner's insurance on a lease (Art 9.4).</summary>
/// <param name="AggregateId">The ground lease.</param>
/// <param name="CertificateId">The certificate.</param>
/// <param name="Insurer">The insurer.</param>
/// <param name="PolicyNumber">The policy number.</param>
/// <param name="PolicyStart">The first day of the policy period.</param>
/// <param name="PolicyEnd">The last day of the policy period.</param>
/// <param name="DwellingCoverage">The dwelling coverage.</param>
/// <param name="LiabilityPerOccurrence">The liability coverage per occurrence.</param>
/// <param name="TrustNamedAdditionalInsured">Whether the trust is named as additional insured.</param>
/// <param name="ReceivedOn">The day the trust received the certificate.</param>
/// <param name="Notes">Notes, such as flood cover, or null for none.</param>
/// <param name="DocumentId">The certificate file in Documents, or null when none was added.</param>
/// <param name="RecordedBy">The staff user who recorded it.</param>
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
