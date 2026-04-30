using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>
/// HTTP client for development project API endpoints.
/// All requests are automatically decorated with an internal JWT by <c>InternalGatewayAuthHandler</c>.
/// </summary>
public sealed class DevelopmentProjectsApiClient(
    HttpClient httpClient,
    ILogger<DevelopmentProjectsApiClient> logger) : BaseApiClient(httpClient, logger), IDevelopmentProjectsApiClient
{
    /// <inheritdoc/>
    public async Task<PaginatedResponse<DevelopmentProjectListItemResponse>> GetDevelopmentProjectsAsync(
        int page, int pageSize, string? search, CancellationToken cancellationToken)
    {
        var qs = $"?page.number={page}&page.size={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            qs += $"&filter.search={Uri.EscapeDataString(search)}";
        }
        
        return await GetAsync<PaginatedResponse<DevelopmentProjectListItemResponse>>($"api/development-projects{qs}", cancellationToken).ConfigureAwait(false);
    }
}
