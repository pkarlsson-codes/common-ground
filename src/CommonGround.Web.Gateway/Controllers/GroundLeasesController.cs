using CommonGround.API.Clients;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.Web.Gateway.Controllers;

/// <summary>Gateway proxy for ground lease endpoints.</summary>
/// <param name="client">Typed client for the Homeownership API.</param>
[ApiController]
[Route("api/v1/ground-leases")]
public sealed class GroundLeasesController(IGroundLeasesApiClient client) : ControllerBase
{
    /// <summary>Creates a new ground lease.</summary>
    /// <param name="request">The ground lease creation details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>201 Created with the new ground lease ID.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateGroundLeaseResponse), 201)]
    [EndpointName("createGroundLease")]
    public async Task<IActionResult> CreateGroundLease(
        [FromBody] CreateGroundLeaseRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.CreateGroundLeaseAsync(request, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetGroundLease), new { groundLeaseId = result.GroundLeaseId }, result);
    }

    /// <summary>Gets a ground lease by ID.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with the ground lease, or 404 if not found.</returns>
    [HttpGet("{groundLeaseId}")]
    [ProducesResponseType(typeof(GroundLeaseResponse), 200)]
    [ProducesResponseType(404)]
    [EndpointName("getGroundLease")]
    public async Task<IActionResult> GetGroundLease(string groundLeaseId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        var result = await client.GetGroundLeaseAsync(groundLeaseId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Gets a paginated list of ground leases.</summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="landParcelId">Optional land parcel ID filter.</param>
    /// <param name="buildingId">Optional building ID filter.</param>
    /// <param name="search">Optional free-text search across homeowner name and email.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with a paginated list of ground leases.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<GroundLeaseListItemResponse>), 200)]
    [EndpointName("getGroundLeases")]
    public async Task<IActionResult> GetGroundLeases(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? landParcelId = null,
        [FromQuery] string? buildingId = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetGroundLeasesAsync(page, pageSize, status, landParcelId, buildingId, search, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Onboards the first homeowner to a ground lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The homeowner details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("{groundLeaseId}/homeowner")]
    [ProducesResponseType(204)]
    [EndpointName("onboardHomeowner")]
    public async Task<IActionResult> OnboardHomeowner(
        string groundLeaseId,
        [FromBody] OnboardHomeownerRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.OnboardHomeownerAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Changes the homeowner on a ground lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The new homeowner details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPatch("{groundLeaseId}/homeowner")]
    [ProducesResponseType(204)]
    [EndpointName("changeHomeowner")]
    public async Task<IActionResult> ChangeHomeowner(
        string groundLeaseId,
        [FromBody] ChangeHomeownerRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.ChangeHomeownerAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Records a stewardship check-in for a ground lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The check-in details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("{groundLeaseId}/stewardship-check-ins")]
    [ProducesResponseType(204)]
    [EndpointName("recordStewardshipCheckIn")]
    public async Task<IActionResult> RecordStewardshipCheckIn(
        string groundLeaseId,
        [FromBody] RecordStewardshipCheckInRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordStewardshipCheckInAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Calculates a resale price estimate for a ground lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The calculation inputs.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with the estimated maximum resale price.</returns>
    [HttpPost("{groundLeaseId}/resale-estimate")]
    [ProducesResponseType(typeof(ResaleEstimateResponse), 200)]
    [EndpointName("calculateResaleEstimate")]
    public async Task<IActionResult> CalculateResaleEstimate(
        string groundLeaseId,
        [FromBody] CalculateResaleEstimateRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.CalculateResaleEstimateAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
