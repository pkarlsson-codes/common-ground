using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.PropertyRegistry.Application.Commands;
using CommonGround.PropertyRegistry.Application.Queries;
using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Buildings;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CommonGround.API.PropertyRegistry;

/// <summary>API controller for building management.</summary>
/// <param name="bus">The Wolverine message bus.</param>
[ApiController]
[Route("api/buildings")]
public sealed class BuildingsController(IMessageBus bus) : BaseApiController
{
    /// <summary>
    /// Registers a new building in the CLT portfolio.
    /// </summary>
    /// <param name="request">The request containing building details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the newly registered building.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<string>), 201)]
    [EndpointName("registerBuilding")]
    public async Task<IActionResult> RegisterBuilding(
        [FromBody] RegisterBuildingRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = HttpContext.GetUserId();

        var command = new RegisterBuilding(
            LandParcelId.Create(request.LandParcelId),
            BuildingType.FromDisplayName(request.BuildingType),
            request.Bedrooms,
            request.Bathrooms,
            request.SquareFootage,
            request.YearBuilt,
            request.InitialAssessedValue,
            userId);

        var id = await bus.InvokeAsync<BuildingId>(command, cancellationToken);

        return CreatedAtAction(nameof(GetBuilding), new { buildingId = id.Value }, id.Value);
    }

    /// <summary>
    /// Retrieves detailed information about a specific building by its ID.
    /// </summary>
    /// <param name="buildingId">The ID of the building to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The detailed information of the building.</returns>
    [HttpGet("{buildingId}")]
    [ProducesResponseType(typeof(ApiResponse<BuildingResponse>), 200)]
    [ProducesResponseType(404)]
    [EndpointName("getBuilding")]
    public async Task<IActionResult> GetBuilding(
        string buildingId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildingId);

        var view = await bus.InvokeAsync<BuildingView>(
            new GetBuilding(BuildingId.Create(buildingId)),
            cancellationToken);

        var response = new BuildingResponse(view.Id, view.LandParcelId,
            view.BuildingType, view.Bedrooms, view.Bathrooms, view.SquareFootage,
            view.YearBuilt, view.AssessedValue, view.LastAssessedDate,
            view.ConditionNotes, view.Status, view.RegisteredAt);

        return Ok(response);
    }

    /// <summary>Gets a paginated list of buildings.</summary>
    /// <param name="request">The pagination and filter parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of buildings.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<BuildingListItemResponse>>), 200)]
    [EndpointName("getBuildings")]
    public async Task<IActionResult> GetBuildings(
        [FromQuery] PaginationQuerySpec<GetBuildingsFilter> request,
        CancellationToken cancellationToken = default)
    {
        var landParcelFilter = request.Filter?.LandParcelIds?
            .Select(LandParcelId.Create).ToList().AsReadOnly();
        var statusFilter = request.Filter?.Statuses?
            .Select(BuildingStatus.FromDisplayName).ToList().AsReadOnly();
        var query = new GetBuildings(request.Page, landParcelFilter, statusFilter);

        var result = await bus
            .InvokeAsync<PaginatedResponse<BuildingListItemView>>(
                query, cancellationToken);

        var response = new PaginatedResponse<BuildingListItemResponse>
        {
            Items = [..result.Items.Select(v => new BuildingListItemResponse(
                v.Id, v.LandParcelId, v.BuildingType, v.Bedrooms, v.Bathrooms, v.SquareFootage, v.AssessedValue, v.Status))],
            TotalCount = result.TotalCount,
            PageSize = result.PageSize,
            PageNumber = result.PageNumber,
        };

        return Ok(response);
    }

    /// <summary>
    /// Records a new assessment for a building, including the assessed value and any relevant notes.
    /// </summary>
    /// <param name="buildingId">The ID of the building to assess.</param>
    /// <param name="request">The request containing the assessment details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the newly created building assessment.</returns>
    [HttpPost("{buildingId}/assessments")]
    [ProducesResponseType(201)]
    [EndpointName("recordBuildingAssessment")]
    public async Task<IActionResult> RecordBuildingAssessment(
        string buildingId,
        [FromBody] RecordBuildingAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildingId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = HttpContext.GetUserId();

        var command = new RecordBuildingAssessment(
            BuildingId.Create(buildingId),
            request.AssessedValue,
            request.AssessmentDate,
            request.ConditionNotes,
            userId);

        await bus.InvokeAsync(command, cancellationToken);

        return Created();
    }

    /// <summary>Updates a building's status.</summary>
    /// <param name="buildingId">The ID of the building to update.</param>
    /// <param name="request">The request containing the new status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if the update is successful.</returns>
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

        var userId = HttpContext.GetUserId();
        var newStatus = BuildingStatus.FromDisplayName(request.NewStatus);

        await bus.InvokeAsync(
            new UpdateBuildingStatus(BuildingId.Create(buildingId), newStatus, userId),
            cancellationToken).ConfigureAwait(false);

        return NoContent();
    }
}