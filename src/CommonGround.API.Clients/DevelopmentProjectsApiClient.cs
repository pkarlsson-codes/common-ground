using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
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
        PaginationQuerySpec<GetDevelopmentProjectsFilter> query, CancellationToken cancellationToken)
    {
        var qs = query?.ToQueryString() ?? string.Empty;
        return await GetAsync<PaginatedResponse<DevelopmentProjectListItemResponse>>($"api/development-projects{qs}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<InitiateDevelopmentProjectResponse> InitiateDevelopmentProjectAsync(
        InitiateDevelopmentProjectRequest request, CancellationToken cancellationToken)
    {
        return await PostAsync<InitiateDevelopmentProjectRequest, InitiateDevelopmentProjectResponse>(
            "api/development-projects",
            request,
            cancellationToken).ConfigureAwait(false);
    }
}
