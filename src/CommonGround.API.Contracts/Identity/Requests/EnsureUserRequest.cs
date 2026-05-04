namespace CommonGround.API.Contracts.Identity.Requests;

/// <summary>Request to idempotently provision a CommonGround-owned user record for a Keycloak subject.</summary>
public sealed record EnsureUserRequest(
    string KeycloakSubject,
    string? Email,
    string? DisplayName,
    string TrustId);
