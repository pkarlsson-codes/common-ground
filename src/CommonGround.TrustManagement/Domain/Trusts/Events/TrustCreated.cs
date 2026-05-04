using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.TrustManagement.Domain.Trusts.Events;

/// <summary>Raised when a new trust is created.</summary>
/// <param name="AggregateId">The unique identifier of the trust.</param>
/// <param name="Name">The name of the trust.</param>
public sealed record TrustCreated(
    string AggregateId,
    string Name)
: DomainEvent(AggregateId);