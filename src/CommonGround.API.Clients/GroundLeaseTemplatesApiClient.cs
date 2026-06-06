using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>
/// HTTP client for ground lease template endpoints.
/// All requests are automatically decorated with an internal JWT by <c>InternalGatewayAuthHandler</c>.
/// </summary>
public sealed class GroundLeaseTemplatesApiClient(
    HttpClient httpClient,
    ILogger<GroundLeaseTemplatesApiClient> logger) : BaseApiClient(httpClient, logger), IGroundLeaseTemplatesApiClient
{
    /// <inheritdoc/>
    public async Task<PaginatedResponse<GroundLeaseTemplateListItemResponse>> GetGroundLeaseTemplatesAsync(
        GetGroundLeaseTemplatesQuerySpec querySpec, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(querySpec);

        return await GetAsync<PaginatedResponse<GroundLeaseTemplateListItemResponse>>(
            $"api/ground-lease-templates{querySpec.ToQueryString()}", cancellationToken).ConfigureAwait(false);
    }
}
