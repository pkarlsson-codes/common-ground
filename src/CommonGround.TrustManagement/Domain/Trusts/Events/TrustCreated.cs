using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.TrustManagement.Domain.Trusts.Events;

/// <summary>Raised when a new trust is created. <c>TimeZone</c> is an IANA time zone id.</summary>
public sealed record TrustCreated(
    string AggregateId,
    string Name,
    string TimeZone)
: DomainEvent(AggregateId);