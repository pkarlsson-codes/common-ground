using CommonGround.API.Clients;
using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.Web.Gateway.Controllers;

/// <summary>Gateway proxy for home endpoints.</summary>
/// <param name="client">Typed client for the PropertyRegistry API.</param>
[ApiController]
[Route("api/v1/homes")]
public sealed class HomesController(IHomesApiClient client) : ControllerBase
{
    /// <summary>Registers a new home.</summary>
    /// <param name="request">The home registration details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>201 Created with the new home ID.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RegisterHomeResponse), 201)]
    [EndpointName("registerHome")]
    public async Task<IActionResult> RegisterHome(
        [FromBody] RegisterHomeRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.RegisterHomeAsync(request, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetHome), new { homeId = result.HomeId }, result);
    }

    /// <summary>Gets a home by ID.</summary>
    /// <param name="homeId">The ID of the home to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with the home, or 404 if not found.</returns>
    [HttpGet("{homeId}")]
    [ProducesResponseType(typeof(HomeResponse), 200)]
    [ProducesResponseType(404)]
    [EndpointName("getHome")]
    public async Task<IActionResult> GetHome(string homeId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        var result = await client
            .GetHomeAsync(homeId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a paginated list of homes.</summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="landParcelId">Optional land parcel ID filter.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with a paginated list of homes.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<HomeListItemResponse>), 200)]
    [EndpointName("getHomes")]
    public async Task<IActionResult> GetHomes(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? landParcelId = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await client
            .GetHomesAsync(
                page, pageSize, landParcelId,
                status, cancellationToken);
        return Ok(result);
    }

    /// <summary>Records an assessment for a home.</summary>
    /// <param name="homeId">The ID of the home to assess.</param>
    /// <param name="request">The assessment details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>201 Created, or 404 if not found.</returns>
    [HttpPost("{homeId}/assessments")]
    [ProducesResponseType(201)]
    [ProducesResponseType(404)]
    [EndpointName("recordHomeAssessment")]
    public async Task<IActionResult> RecordHomeAssessment(
        string homeId,
        [FromBody] RecordHomeAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordHomeAssessmentAsync(
            homeId, request, cancellationToken);
        return Created();
    }

    /// <summary>Gets a paginated list of assessments for a home.</summary>
    /// <param name="homeId">The ID of the home.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with a paginated list of assessments, or 404 if not found.</returns>
    [HttpGet("{homeId}/assessments")]
    [ProducesResponseType(typeof(PaginatedResponse<HomeAssessmentItemResponse>), 200)]
    [ProducesResponseType(404)]
    [EndpointName("getHomeAssessments")]
    public async Task<IActionResult> GetHomeAssessments(
        string homeId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        var result = await client.GetHomeAssessmentsAsync(homeId, page, pageSize, cancellationToken);
        return Ok(result);
    }
}
