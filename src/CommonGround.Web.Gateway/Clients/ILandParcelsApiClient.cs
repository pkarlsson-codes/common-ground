using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;

namespace CommonGround.Web.Gateway.Clients;

/// <summary>Typed HTTP client for the PropertyRegistry land parcel endpoints.</summary>
public interface ILandParcelsApiClient
{
    /// <summary>Registers a new land parcel and returns its ID.</summary>
    /// <param name="request">The land parcel registration details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The ID of the newly registered land parcel.</returns>
    Task<string> RegisterLandParcelAsync(RegisterLandParcelRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a land parcel by its ID.</summary>
    /// <param name="landParcelId">The ID of the land parcel to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The land parcel details.</returns>
    Task<LandParcelResponse> GetLandParcelAsync(string landParcelId, CancellationToken cancellationToken);

    /// <summary>Gets a paginated list of land parcels.</summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated list of land parcel summaries.</returns>
    Task<PaginatedResponse<LandParcelListItemResponse>> GetLandParcelsAsync(int page, int pageSize, string? status, CancellationToken cancellationToken);

    /// <summary>Updates the status of a land parcel.</summary>
    /// <param name="landParcelId">The ID of the land parcel to update.</param>
    /// <param name="request">The new status details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateLandParcelStatusAsync(string landParcelId, UpdateLandParcelStatusRequest request, CancellationToken cancellationToken);
}
