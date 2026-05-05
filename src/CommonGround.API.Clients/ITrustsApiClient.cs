using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for the trust management API endpoints.</summary>
public interface ITrustsApiClient
{
    /// <summary>Creates a new trust and returns the response.</summary>
    /// <param name="request">The request containing the trust name.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The response containing the stable trust ID.</returns>
    Task<CreateTrustResponse> CreateTrustAsync(CreateTrustRequest request, CancellationToken ct = default);
}
