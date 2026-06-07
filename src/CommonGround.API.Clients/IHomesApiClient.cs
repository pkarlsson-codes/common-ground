using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;

namespace CommonGround.API.Clients;

/// <summary>Typed HTTP client for the PropertyRegistry home endpoints.</summary>
public interface IHomesApiClient
{
    /// <summary>Registers a new home and returns its ID.</summary>
    /// <param name="request">The home registration details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The registration response containing the new home ID.</returns>
    Task<RegisterHomeResponse> RegisterHomeAsync(RegisterHomeRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a home by its ID.</summary>
    /// <param name="homeId">The ID of the home to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The home details.</returns>
    Task<HomeResponse> GetHomeAsync(string homeId, CancellationToken cancellationToken);

    /// <summary>Gets a paginated list of homes.</summary>
    /// <param name="querySpec">The query specification containing pagination and filter parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated list of home summaries.</returns>
    Task<PaginatedResponse<HomeListItemResponse>> GetHomesAsync(GetHomesQuerySpec querySpec, CancellationToken cancellationToken);

    /// <summary>Records an assessment for a home.</summary>
    /// <param name="homeId">The ID of the home to assess.</param>
    /// <param name="request">The assessment details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordHomeAssessmentAsync(string homeId, RecordHomeAssessmentRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a paginated list of assessments for a home, newest first.</summary>
    /// <param name="homeId">The ID of the home.</param>
    /// <param name="querySpec">The query specification containing pagination parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated list of assessment items.</returns>
    Task<PaginatedResponse<HomeAssessmentItemResponse>> GetHomeAssessmentsAsync(string homeId, GetHomeAssessmentsQuerySpec querySpec, CancellationToken cancellationToken);
}
