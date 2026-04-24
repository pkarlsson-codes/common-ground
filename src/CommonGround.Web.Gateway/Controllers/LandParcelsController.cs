using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.Web.Gateway.Clients;
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
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with a paginated list of land parcels.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<LandParcelResponse>), 200)]
    [EndpointName("getLandParcels")]
    public async Task<IActionResult> GetLandParcels(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetLandParcelsAsync(page, pageSize, status, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Updates a land parcel's status.</summary>
    /// <param name="landParcelId">The ID of the land parcel to update.</param>
    /// <param name="request">The new status details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content, or 404 if not found.</returns>
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
