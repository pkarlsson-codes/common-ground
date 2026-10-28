using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for development project API endpoints.</summary>
public interface IDevelopmentProjectsApiClient
{
    /// <summary>Gets a paginated list of development projects.</summary>
    /// <param name="querySpec">The query specification containing pagination and filter parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of development projects.</returns>
    Task<PaginatedResponse<DevelopmentProjectListItemResponse>> GetDevelopmentProjectsAsync(
        GetDevelopmentProjectsQuerySpec querySpec, CancellationToken cancellationToken);

    /// <summary>Gets a single development project.</summary>
    /// <param name="projectId">The ID of the project.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The development project.</returns>
    Task<DevelopmentProjectResponse> GetDevelopmentProjectAsync(string projectId, CancellationToken cancellationToken);

    /// <summary>Initiates a new development project for a land parcel.</summary>
    /// <param name="request">The initiation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the newly created development project.</returns>
    Task<InitiateDevelopmentProjectResponse> InitiateDevelopmentProjectAsync(
        InitiateDevelopmentProjectRequest request, CancellationToken cancellationToken);
}
