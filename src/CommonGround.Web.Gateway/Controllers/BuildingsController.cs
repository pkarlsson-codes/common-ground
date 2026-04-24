using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.Web.Gateway.Clients;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.Web.Gateway.Controllers;

/// <summary>Gateway proxy for building endpoints.</summary>
/// <param name="client">Typed client for the PropertyRegistry API.</param>
[ApiController]
[Route("api/v1/buildings")]
public sealed class BuildingsController(IBuildingsApiClient client) : ControllerBase
{
    /// <summary>Registers a new building.</summary>
    /// <param name="request">The building registration details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>201 Created with the new building ID.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RegisterBuildingResponse), 201)]
    [EndpointName("registerBuilding")]
    public async Task<IActionResult> RegisterBuilding(
        [FromBody] RegisterBuildingRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.RegisterBuildingAsync(request, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetBuilding), new { buildingId = result.BuildingId }, result);
    }

    /// <summary>Gets a building by ID.</summary>
    /// <param name="buildingId">The ID of the building to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with the building, or 404 if not found.</returns>
    [HttpGet("{buildingId}")]
    [ProducesResponseType(typeof(BuildingResponse), 200)]
    [ProducesResponseType(404)]
    [EndpointName("getBuilding")]
    public async Task<IActionResult> GetBuilding(string buildingId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildingId);
        var result = await client
            .GetBuildingAsync(buildingId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a paginated list of buildings.</summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="landParcelId">Optional land parcel ID filter.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with a paginated list of buildings.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<BuildingListItemResponse>), 200)]
    [EndpointName("getBuildings")]
    public async Task<IActionResult> GetBuildings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? landParcelId = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await client
            .GetBuildingsAsync(
                page, pageSize, landParcelId,
                status, cancellationToken);
        return Ok(result);
    }

    /// <summary>Records an assessment for a building.</summary>
    /// <param name="buildingId">The ID of the building to assess.</param>
    /// <param name="request">The assessment details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>201 Created, or 404 if not found.</returns>
    [HttpPost("{buildingId}/assessments")]
    [ProducesResponseType(201)]
    [ProducesResponseType(404)]
    [EndpointName("recordBuildingAssessment")]
    public async Task<IActionResult> RecordBuildingAssessment(
        string buildingId,
        [FromBody] RecordBuildingAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildingId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordBuildingAssessmentAsync(
            buildingId, request, cancellationToken);
        return Created();
    }

    /// <summary>Updates a building's status.</summary>
    /// <param name="buildingId">The ID of the building to update.</param>
    /// <param name="request">The new status details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content, or 404 if not found.</returns>
    [HttpPatch("{buildingId}/status")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EndpointName("updateBuildingStatus")]
    public async Task<IActionResult> UpdateBuildingStatus(
        string buildingId,
        [FromBody] UpdateBuildingStatusRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildingId);
        ArgumentNullException.ThrowIfNull(request);
        await client.UpdateBuildingStatusAsync(buildingId, request, cancellationToken);
        return NoContent();
    }
}
