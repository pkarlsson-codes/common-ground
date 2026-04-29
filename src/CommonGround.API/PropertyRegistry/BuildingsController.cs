using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.PropertyRegistry.Application.Commands;
using CommonGround.PropertyRegistry.Application.Queries;
using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.PropertyRegistry.Domain.LeasableUnits;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CommonGround.API.PropertyRegistry;

/// <summary>API controller for dwelling unit management.</summary>
/// <param name="bus">The Wolverine message bus.</param>
[ApiController]
[Route("api/buildings")]
public sealed class BuildingsController(IMessageBus bus) : ControllerBase
{
    /// <summary>
    /// Registers a new dwelling unit in the CLT portfolio.
    /// </summary>
    /// <param name="request">The request containing unit details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the newly registered unit.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RegisterBuildingResponse), 201)]
    [EndpointName("registerBuilding")]
    public async Task<IActionResult> RegisterBuilding(
        [FromBody] RegisterBuildingRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = HttpContext.GetUserId();

        var command = new RegisterDwellingUnit(
            LandParcelId.Create(request.LandParcelId),
            DwellingUnitType.FromDisplayName(request.BuildingType),
            request.Bedrooms,
            request.Bathrooms,
            request.SquareFootage,
            request.YearBuilt,
            request.InitialAssessedValue,
            userId);

        var id = await bus.InvokeAsync<LeasableUnitId>(command, cancellationToken);

        return CreatedAtAction(nameof(GetBuilding), new { buildingId = id.Value }, new RegisterBuildingResponse(id.Value));
    }

    /// <summary>
    /// Retrieves detailed information about a specific dwelling unit by its ID.
    /// </summary>
    /// <param name="buildingId">The ID of the unit to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The detailed information of the unit.</returns>
    [HttpGet("{buildingId}")]
    [ProducesResponseType(typeof(BuildingResponse), 200)]
    [ProducesResponseType(404)]
    [EndpointName("getBuilding")]
    public async Task<IActionResult> GetBuilding(
        string buildingId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildingId);

        var view = await bus.InvokeAsync<DwellingUnitView>(
            new GetDwellingUnit(LeasableUnitId.Create(buildingId)),
            cancellationToken);

        var response = new BuildingResponse(view.Id, view.LandParcelId,
            view.UnitType, view.Bedrooms, view.Bathrooms, view.SquareFootage,
            view.YearBuilt, view.AssessedValue, view.LastAssessedDate,
            view.ConditionNotes, view.Status, view.RegisteredAt);

        return Ok(response);
    }

    /// <summary>Gets a paginated list of dwelling units.</summary>
    /// <param name="request">The pagination and filter parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of dwelling units.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<BuildingListItemResponse>), 200)]
    [EndpointName("getBuildings")]
    public async Task<IActionResult> GetBuildings(
        [FromQuery] PaginationQuerySpec<GetBuildingsFilter> request,
        CancellationToken cancellationToken = default)
    {
        var landParcelFilter = request.Filter?.LandParcelIds?
            .Select(LandParcelId.Create).ToList().AsReadOnly();
        var statusFilter = request.Filter?.Statuses?
            .Select(UnitStatus.FromDisplayName).ToList().AsReadOnly();
        var query = new GetDwellingUnits(request.Page, landParcelFilter, statusFilter);

        var result = await bus
            .InvokeAsync<PaginatedResponse<DwellingUnitListItemView>>(
                query, cancellationToken);

        var response = result.MapItems(v => new BuildingListItemResponse(
            v.Id, v.LandParcelId, v.UnitType, v.Bedrooms, v.Bathrooms, v.SquareFootage, v.AssessedValue, v.Status));

        return Ok(response);
    }

    /// <summary>
    /// Records a new assessment for a dwelling unit, including the assessed value and any relevant notes.
    /// </summary>
    /// <param name="buildingId">The ID of the unit to assess.</param>
    /// <param name="request">The request containing the assessment details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if the assessment is recorded successfully.</returns>
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

        var command = new RecordUnitAssessment(
            LeasableUnitId.Create(buildingId),
            request.AssessedValue,
            request.AssessmentDate,
            request.ConditionNotes,
            userId);

        await bus.InvokeAsync(command, cancellationToken);

        return Created();
    }

    /// <summary>Gets a paginated list of assessments for a dwelling unit.</summary>
    /// <param name="buildingId">The ID of the unit.</param>
    /// <param name="request">Pagination parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of assessments, newest first.</returns>
    [HttpGet("{buildingId}/assessments")]
    [ProducesResponseType(typeof(PaginatedResponse<BuildingAssessmentItemResponse>), 200)]
    [ProducesResponseType(404)]
    [EndpointName("getBuildingAssessments")]
    public async Task<IActionResult> GetBuildingAssessments(
        string buildingId,
        [FromQuery] PaginationQuerySpec<object> request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildingId);

        var query = new GetDwellingUnitAssessments(LeasableUnitId.Create(buildingId), request.Page);
        var result = await bus.InvokeAsync<PaginatedResponse<DwellingUnitAssessmentView>>(query, cancellationToken);

        var response = result.MapItems(v => new BuildingAssessmentItemResponse(
            v.Id.ToString(), v.UnitId, v.AssessedValue, v.AssessmentDate, v.ConditionNotes, v.AssessedBy, v.RecordedAt));

        return Ok(response);
    }
}
