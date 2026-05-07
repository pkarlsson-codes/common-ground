using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Contracts.Events;

/// <summary>Integration event published by the Homeownership BC when a ground lease is terminated.</summary>
public sealed record GroundLeaseTerminated(
    string GroundLeaseId,
    string Reason,
    DateOnly TerminatedDate,
    string TerminatedBy) : IntegrationEvent;
