using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.Households.Events;

public sealed record HousingCounselingRecorded(
    string AggregateId,
    string CertificationNumber,
    DateOnly CompletedDate,
    string CounselorOrganization,
    string RecordedBy) : DomainEvent(AggregateId);