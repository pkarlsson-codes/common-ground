using CommonGround.API.Clients;
using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.Web.Gateway.Controllers;

/// <summary>Gateway proxy for land parcel endpoints.</summary>
/// <param name="client">Typed client for the PropertyRegistry API.</param>
[ApiController]
[Route("api/v1/land-parcels")]
public sealed class LandParcelsController(ILandParcelsApiClient client) : ControllerBase
{
    /// <summary>Registers a new land parcel.</summary>
    /// <param name="request">The land parcel registration details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>201 Created with the new land parcel ID.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RegisterLandParcelResponse), 201)]
    [EndpointName("registerLandParcel")]
    public async Task<IActionResult> RegisterLandParcel(
        [FromBody] RegisterLandParcelRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.RegisterLandParcelAsync(request, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetLandParcel), new { landParcelId = result.ParcelId }, result);
    }

    /// <summary>Gets a land parcel by ID.</summary>
    /// <param name="landParcelId">The ID of the land parcel to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with the land parcel, or 404 if not found.</returns>
    [HttpGet("{landParcelId}")]
    [ProducesResponseType(typeof(LandParcelResponse), 200)]
    [ProducesResponseType(404)]
    [EndpointName("getLandParcel")]
    public async Task<IActionResult> GetLandParcel(string landParcelId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        var result = await client.GetLandParcelAsync(landParcelId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Gets a paginated list of land parcels.</summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="search">Optional free-text search across address and legal parcel number.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with a paginated list of land parcels.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<LandParcelResponse>), 200)]
    [EndpointName("getLandParcels")]
    public async Task<IActionResult> GetLandParcels(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetLandParcelsAsync(page, pageSize, status, search, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Starts predevelopment for a land parcel.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content, or 404 if not found.</returns>
    [HttpPost("{landParcelId}/predevelopment/start")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EndpointName("startLandParcelPredevelopment")]
    public async Task<IActionResult> StartParcelPredevelopment(
        string landParcelId,
        [FromBody] StartParcelPredevelopmentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        ArgumentNullException.ThrowIfNull(request);
        await client.StartParcelPredevelopmentAsync(landParcelId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Starts construction for a land parcel.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content, or 404 if not found.</returns>
    [HttpPost("{landParcelId}/construction/start")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EndpointName("startLandParcelConstruction")]
    public async Task<IActionResult> StartParcelConstruction(
        string landParcelId,
        [FromBody] StartParcelConstructionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        ArgumentNullException.ThrowIfNull(request);
        await client.StartParcelConstructionAsync(landParcelId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Marks a land parcel ready for occupancy.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content, or 404 if not found.</returns>
    [HttpPost("{landParcelId}/ready-for-occupancy/mark")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EndpointName("markLandParcelReadyForOccupancy")]
    public async Task<IActionResult> MarkParcelReadyForOccupancy(
        string landParcelId,
        [FromBody] MarkParcelReadyForOccupancyRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        ArgumentNullException.ThrowIfNull(request);
        await client.MarkParcelReadyForOccupancyAsync(landParcelId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Decommissions a land parcel.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content, or 404 if not found.</returns>
    [HttpPost("{landParcelId}/decommission")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EndpointName("decommissionLandParcel")]
    public async Task<IActionResult> DecommissionParcel(
        string landParcelId,
        [FromBody] DecommissionParcelRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        ArgumentNullException.ThrowIfNull(request);
        await client.DecommissionParcelAsync(landParcelId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Recommissions a land parcel.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content, or 404 if not found.</returns>
    [HttpPost("{landParcelId}/recommission")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EndpointName("recommissionLandParcel")]
    public async Task<IActionResult> RecommissionParcel(
        string landParcelId,
        [FromBody] RecommissionParcelRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecommissionParcelAsync(landParcelId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Marks a land parcel occupied.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content, or 404 if not found.</returns>
    [HttpPost("{landParcelId}/occupied/mark")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EndpointName("markLandParcelOccupied")]
    public async Task<IActionResult> MarkParcelOccupied(
        string landParcelId,
        [FromBody] MarkParcelOccupiedRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        ArgumentNullException.ThrowIfNull(request);
        await client.MarkParcelOccupiedAsync(landParcelId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Enters transitioning state for a land parcel.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content, or 404 if not found.</returns>
    [HttpPost("{landParcelId}/transitioning/enter")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EndpointName("enterLandParcelTransitioning")]
    public async Task<IActionResult> EnterParcelTransitioning(
        string landParcelId,
        [FromBody] EnterParcelTransitioningRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        ArgumentNullException.ThrowIfNull(request);
        await client.EnterParcelTransitioningAsync(landParcelId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Returns a land parcel to available status.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content, or 404 if not found.</returns>
    [HttpPost("{landParcelId}/available/return")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EndpointName("returnLandParcelToAvailable")]
    public async Task<IActionResult> ReturnParcelToAvailable(
        string landParcelId,
        [FromBody] ReturnParcelToAvailableRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        ArgumentNullException.ThrowIfNull(request);
        await client.ReturnParcelToAvailableAsync(landParcelId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Legacy status mutation endpoint retained for backward compatibility. Prefer explicit transition endpoints.</summary>
    /// <param name="landParcelId">The ID of the land parcel to update.</param>
    /// <param name="request">The new status details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content, or 404 if not found.</returns>
    [Obsolete("Use explicit land parcel transition endpoints.")]
    [HttpPatch("{landParcelId}/status")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EndpointName("updateLandParcelStatus")]
    public async Task<IActionResult> UpdateLandParcelStatus(
        string landParcelId,
        [FromBody] UpdateLandParcelStatusRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        ArgumentNullException.ThrowIfNull(request);
        await client.UpdateLandParcelStatusAsync(landParcelId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
