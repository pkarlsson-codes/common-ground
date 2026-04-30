using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for development project API endpoints.</summary>
public interface IDevelopmentProjectsApiClient
{
    /// <summary>
    /// Gets a paginated list of development projects.
    /// </summary>
    /// <param name="query">Pagination and filter parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of development projects.</returns>
    Task<PaginatedResponse<DevelopmentProjectListItemResponse>> GetDevelopmentProjectsAsync(
        PaginationQuerySpec<GetDevelopmentProjectsFilter> query, CancellationToken cancellationToken);

    /// <summary>Initiates a new development project for a land parcel.</summary>
    /// <param name="request">The initiation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the newly created development project.</returns>
    Task<InitiateDevelopmentProjectResponse> InitiateDevelopmentProjectAsync(
        InitiateDevelopmentProjectRequest request, CancellationToken cancellationToken);
}
