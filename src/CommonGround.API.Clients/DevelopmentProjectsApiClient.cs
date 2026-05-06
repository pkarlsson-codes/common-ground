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
        int pageNumber, int pageSize, string? search, string? landParcelId,
        IReadOnlyCollection<string>? homeIds, CancellationToken cancellationToken)
    {
        var query = new List<string>(capacity: 5)
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}",
        };
        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add($"search={Uri.EscapeDataString(search)}");
        }

        if (!string.IsNullOrWhiteSpace(landParcelId))
        {
            query.Add($"landParcelId={Uri.EscapeDataString(landParcelId)}");
        }

        if (homeIds is { Count: > 0 })
        {
            query.AddRange(homeIds.Select(id => $"homeIds={Uri.EscapeDataString(id)}"));
        }

        return await GetAsync<PaginatedResponse<DevelopmentProjectListItemResponse>>(
            $"api/development-projects?{string.Join('&', query)}", cancellationToken).ConfigureAwait(false);
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
