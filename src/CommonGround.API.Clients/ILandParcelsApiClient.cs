using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;

namespace CommonGround.API.Clients;

/// <summary>Typed HTTP client for the PropertyRegistry land parcel endpoints.</summary>
public interface ILandParcelsApiClient
{
    /// <summary>Registers a new land parcel and returns its ID.</summary>
    /// <param name="request">The land parcel registration details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The registration response containing the new land parcel ID.</returns>
    Task<RegisterLandParcelResponse> RegisterLandParcelAsync(RegisterLandParcelRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a land parcel by its ID.</summary>
    /// <param name="landParcelId">The ID of the land parcel to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The land parcel details.</returns>
    Task<LandParcelResponse> GetLandParcelAsync(string landParcelId, CancellationToken cancellationToken);

    /// <summary>Gets a paginated list of land parcels.</summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="search">Optional free-text search across address and legal parcel number.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated list of land parcel summaries.</returns>
    Task<PaginatedResponse<LandParcelListItemResponse>> GetLandParcelsAsync(int page, int pageSize, string? status, string? search, CancellationToken cancellationToken);

    /// <summary>Starts predevelopment for a land parcel.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StartParcelPredevelopmentAsync(string landParcelId, StartParcelPredevelopmentRequest request, CancellationToken cancellationToken);

    /// <summary>Starts construction for a land parcel.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StartParcelConstructionAsync(string landParcelId, StartParcelConstructionRequest request, CancellationToken cancellationToken);

    /// <summary>Marks a land parcel ready for occupancy.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MarkParcelReadyForOccupancyAsync(string landParcelId, MarkParcelReadyForOccupancyRequest request, CancellationToken cancellationToken);

    /// <summary>Decommissions a land parcel.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DecommissionParcelAsync(string landParcelId, DecommissionParcelRequest request, CancellationToken cancellationToken);

    /// <summary>Recommissions a land parcel.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecommissionParcelAsync(string landParcelId, RecommissionParcelRequest request, CancellationToken cancellationToken);

    /// <summary>Marks a land parcel occupied.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MarkParcelOccupiedAsync(string landParcelId, MarkParcelOccupiedRequest request, CancellationToken cancellationToken);

    /// <summary>Enters transitioning state for a land parcel.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EnterParcelTransitioningAsync(string landParcelId, EnterParcelTransitioningRequest request, CancellationToken cancellationToken);

    /// <summary>Returns a land parcel to available status.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ReturnParcelToAvailableAsync(string landParcelId, ReturnParcelToAvailableRequest request, CancellationToken cancellationToken);

    /// <summary>Updates the status of a land parcel.</summary>
    /// <param name="landParcelId">The ID of the land parcel to update.</param>
    /// <param name="request">The new status details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Obsolete("Use explicit land parcel transition endpoints.")]
    Task UpdateLandParcelStatusAsync(string landParcelId, UpdateLandParcelStatusRequest request, CancellationToken cancellationToken);
}
