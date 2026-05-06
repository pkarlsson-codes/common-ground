using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for development project API endpoints.</summary>
public interface IDevelopmentProjectsApiClient
{
    /// <summary>
    /// Gets a paginated list of development projects.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="search">Optional free-text search.</param>
    /// <param name="landParcelId">Optional land parcel ID filter.</param>
    /// <param name="homeIds">Optional home ID filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of development projects.</returns>
    Task<PaginatedResponse<DevelopmentProjectListItemResponse>> GetDevelopmentProjectsAsync(
        int pageNumber, int pageSize, string? search, string? landParcelId,
        IReadOnlyCollection<string>? homeIds, CancellationToken cancellationToken);

    /// <summary>Initiates a new development project for a land parcel.</summary>
    /// <param name="request">The initiation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the newly created development project.</returns>
    Task<InitiateDevelopmentProjectResponse> InitiateDevelopmentProjectAsync(
        InitiateDevelopmentProjectRequest request, CancellationToken cancellationToken);
}
