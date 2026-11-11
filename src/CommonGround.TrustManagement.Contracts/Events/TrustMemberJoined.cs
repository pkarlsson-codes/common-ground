using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.TrustManagement.Contracts.Events;

/// <summary>Integration event published when a user joins a trust with an invitation; Identity assigns the membership.</summary>
public sealed record TrustMemberJoined(
    string TrustId,
    string UserId) : IntegrationEvent;
