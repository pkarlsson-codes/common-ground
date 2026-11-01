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
    IHttpClientFactory httpClientFactory,
    ILogger<DevelopmentProjectsApiClient> logger) : BaseApiClient(httpClientFactory.CreateClient(nameof(DevelopmentProjectsApiClient)), logger), IDevelopmentProjectsApiClient
{
    /// <inheritdoc/>
    public async Task<PaginatedResponse<DevelopmentProjectListItemResponse>> GetDevelopmentProjectsAsync(
        GetDevelopmentProjectsQuerySpec querySpec, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(querySpec);

        return await GetAsync<PaginatedResponse<DevelopmentProjectListItemResponse>>(
            $"api/development-projects{querySpec.ToQueryString()}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<DevelopmentProjectResponse> GetDevelopmentProjectAsync(string projectId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);
        return await GetAsync<DevelopmentProjectResponse>($"api/development-projects/{projectId}", cancellationToken).ConfigureAwait(false);
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
