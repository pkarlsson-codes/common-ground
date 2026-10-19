using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.Households.Events;

/// <summary>Housing counseling recorded event.</summary>
public sealed record HousingCounselingRecorded(
    string AggregateId,
    string CertificationNumber,
    DateOnly CompletedDate,
    string CounselorOrganization,
    string RecordedBy) : DomainEvent(AggregateId);