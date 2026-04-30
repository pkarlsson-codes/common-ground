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

/// <summary>API controller for home management.</summary>
/// <param name="bus">The Wolverine message bus.</param>
[ApiController]
[Route("api/homes")]
public sealed class HomesController(IMessageBus bus) : ControllerBase
{
    /// <summary>
    /// Registers a new home in the CLT portfolio.
    /// </summary>
    /// <param name="request">The request containing home details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the newly registered home.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RegisterBuildingResponse), 201)]
    [EndpointName("registerHome")]
    public async Task<IActionResult> RegisterHome(
        [FromBody] RegisterBuildingRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = HttpContext.GetUserId();

        var command = new RegisterHome(
            LandParcelId.Create(request.LandParcelId),
            HomeType.FromDisplayName(request.BuildingType),
            request.Bedrooms,
            request.Bathrooms,
            request.SquareFootage,
            request.YearBuilt,
            request.InitialAssessedValue,
            userId);

        var id = await bus.InvokeAsync<LeasableUnitId>(command, cancellationToken);

        return CreatedAtAction(nameof(GetHome), new { homeId = id.Value }, new RegisterBuildingResponse(id.Value));
    }

    /// <summary>
    /// Retrieves detailed information about a specific home by its ID.
    /// </summary>
    /// <param name="homeId">The ID of the home to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The detailed information of the home.</returns>
    [HttpGet("{homeId}")]
    [ProducesResponseType(typeof(BuildingResponse), 200)]
    [ProducesResponseType(404)]
    [EndpointName("getHome")]
    public async Task<IActionResult> GetHome(
        string homeId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        var view = await bus.InvokeAsync<HomeView>(
            new GetHome(LeasableUnitId.Create(homeId)),
            cancellationToken);

        var response = new BuildingResponse(view.Id, view.LandParcelId,
            view.UnitType, view.Bedrooms, view.Bathrooms, view.SquareFootage,
            view.YearBuilt, view.AssessedValue, view.LastAssessedDate,
            view.ConditionNotes, view.Status, view.RegisteredAt);

        return Ok(response);
    }

    /// <summary>Gets a paginated list of homes.</summary>
    /// <param name="request">The pagination and filter parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of homes.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<BuildingListItemResponse>), 200)]
    [EndpointName("getHomes")]
    public async Task<IActionResult> GetHomes(
        [FromQuery] PaginationQuerySpec<GetBuildingsFilter> request,
        CancellationToken cancellationToken = default)
    {
        var landParcelFilter = request.Filter?.LandParcelIds?
            .Select(LandParcelId.Create).ToList().AsReadOnly();
        var statusFilter = request.Filter?.Statuses?
            .Select(UnitStatus.FromDisplayName).ToList().AsReadOnly();
        var query = new GetHomes(request.Page, landParcelFilter, statusFilter);

        var result = await bus
            .InvokeAsync<PaginatedResponse<HomeListItemView>>(
                query, cancellationToken);

        var response = result.MapItems(v => new BuildingListItemResponse(
            v.Id, v.LandParcelId, v.UnitType, v.Bedrooms, v.Bathrooms, v.SquareFootage, v.AssessedValue, v.Status));

        return Ok(response);
    }

    /// <summary>
    /// Records a new assessment for a home, including the assessed value and any relevant notes.
    /// </summary>
    /// <param name="homeId">The ID of the home to assess.</param>
    /// <param name="request">The request containing the assessment details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if the assessment is recorded successfully.</returns>
    [HttpPost("{homeId}/assessments")]
    [ProducesResponseType(201)]
    [EndpointName("recordHomeAssessment")]
    public async Task<IActionResult> RecordHomeAssessment(
        string homeId,
        [FromBody] RecordBuildingAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = HttpContext.GetUserId();

        var command = new RecordUnitAssessment(
            LeasableUnitId.Create(homeId),
            request.AssessedValue,
            request.AssessmentDate,
            request.ConditionNotes,
            userId);

        await bus.InvokeAsync(command, cancellationToken);

        return Created();
    }

    /// <summary>Gets a paginated list of assessments for a home.</summary>
    /// <param name="homeId">The ID of the home.</param>
    /// <param name="request">Pagination parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of assessments, newest first.</returns>
    [HttpGet("{homeId}/assessments")]
    [ProducesResponseType(typeof(PaginatedResponse<BuildingAssessmentItemResponse>), 200)]
    [ProducesResponseType(404)]
    [EndpointName("getHomeAssessments")]
    public async Task<IActionResult> GetHomeAssessments(
        string homeId,
        [FromQuery] PaginationQuerySpec<object> request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        var query = new GetHomeAssessments(LeasableUnitId.Create(homeId), request.Page);
        var result = await bus.InvokeAsync<PaginatedResponse<HomeAssessmentView>>(query, cancellationToken);

        var response = result.MapItems(v => new BuildingAssessmentItemResponse(
            v.Id.ToString(), v.UnitId, v.AssessedValue, v.AssessmentDate, v.ConditionNotes, v.AssessedBy, v.RecordedAt));

        return Ok(response);
    }
}
