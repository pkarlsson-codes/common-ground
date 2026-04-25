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
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="landParcelId">Optional land parcel ID filter.</param>
    /// <param name="buildingId">Optional building ID filter.</param>
    /// <param name="search">Optional free-text search across homeowner name and email.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated list of ground lease summaries.</returns>
    Task<PaginatedResponse<GroundLeaseListItemResponse>> GetGroundLeasesAsync(int page, int pageSize, string? status, string? landParcelId, string? buildingId, string? search, CancellationToken cancellationToken);

    /// <summary>Onboards the first homeowner to a ground lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The homeowner details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnboardHomeownerAsync(string groundLeaseId, OnboardHomeownerRequest request, CancellationToken cancellationToken);

    /// <summary>Changes the homeowner on a ground lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The new homeowner details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ChangeHomeownerAsync(string groundLeaseId, ChangeHomeownerRequest request, CancellationToken cancellationToken);

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
