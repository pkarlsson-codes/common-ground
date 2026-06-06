using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;

namespace CommonGround.API.Clients;

/// <summary>Typed HTTP client for the Homeownership ground lease endpoints.</summary>
public interface IGroundLeasesApiClient
{
    /// <summary>Creates a new ground lease.</summary>
    /// <param name="request">The ground lease creation details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The creation response containing the new ground lease ID.</returns>
    Task<CreateGroundLeaseResponse> CreateGroundLeaseAsync(CreateGroundLeaseRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a ground lease by its ID.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The ground lease details.</returns>
    Task<GroundLeaseResponse> GetGroundLeaseAsync(string groundLeaseId, CancellationToken cancellationToken);

    /// <summary>Gets a paginated list of ground leases.</summary>
    /// <param name="querySpec">The query parameters for pagination and filtering.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated list of ground lease summaries.</returns>
    Task<PaginatedResponse<GroundLeaseListItemResponse>> GetGroundLeasesAsync(GetGroundLeasesQuerySpec querySpec, CancellationToken cancellationToken);

    /// <summary>Records a stewardship check-in for a ground lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The check-in details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordStewardshipCheckInAsync(string groundLeaseId, RecordStewardshipCheckInRequest request, CancellationToken cancellationToken);

    /// <summary>Calculates a resale price estimate for a ground lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The calculation inputs.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The estimated maximum resale price.</returns>
    Task<ResaleEstimateResponse> CalculateResaleEstimateAsync(string groundLeaseId, CalculateResaleEstimateRequest request, CancellationToken cancellationToken);
}
