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
        int pageNumber, int pageSize, string? search, string? status, CancellationToken cancellationToken)
    {
        var qs = new QueryStringBuilder()
            .Add("pageNumber", pageNumber)
            .Add("pageSize", pageSize)
            .AddIfNotWhiteSpace("search", search)
            .AddIfNotWhiteSpace("status", status)
            .Build();

        return await GetAsync<PaginatedResponse<GroundLeaseTemplateListItemResponse>>(
            $"api/ground-lease-templates{qs}", cancellationToken).ConfigureAwait(false);
    }
}
