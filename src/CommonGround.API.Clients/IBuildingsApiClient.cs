using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;

namespace CommonGround.API.Clients;

/// <summary>Typed HTTP client for the PropertyRegistry building endpoints.</summary>
public interface IBuildingsApiClient
{
    /// <summary>Registers a new building and returns its ID.</summary>
    /// <param name="request">The building registration details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The registration response containing the new building ID.</returns>
    Task<RegisterBuildingResponse> RegisterBuildingAsync(RegisterBuildingRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a building by its ID.</summary>
    /// <param name="buildingId">The ID of the building to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The building details.</returns>
    Task<BuildingResponse> GetBuildingAsync(string buildingId, CancellationToken cancellationToken);

    /// <summary>Gets a paginated list of buildings.</summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="landParcelId">Optional land parcel ID filter.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated list of building summaries.</returns>
    Task<PaginatedResponse<BuildingListItemResponse>> GetBuildingsAsync(int page, int pageSize, string? landParcelId, string? status, CancellationToken cancellationToken);

    /// <summary>Records an assessment for a building.</summary>
    /// <param name="buildingId">The ID of the building to assess.</param>
    /// <param name="request">The assessment details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordBuildingAssessmentAsync(string buildingId, RecordBuildingAssessmentRequest request, CancellationToken cancellationToken);

    /// <summary>Updates the status of a building.</summary>
    /// <param name="buildingId">The ID of the building to update.</param>
    /// <param name="request">The new status details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateBuildingStatusAsync(string buildingId, UpdateBuildingStatusRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a paginated list of assessments for a building, newest first.</summary>
    /// <param name="buildingId">The ID of the building.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated list of assessment items.</returns>
    Task<PaginatedResponse<BuildingAssessmentItemResponse>> GetBuildingAssessmentsAsync(string buildingId, int page, int pageSize, CancellationToken cancellationToken);
}
