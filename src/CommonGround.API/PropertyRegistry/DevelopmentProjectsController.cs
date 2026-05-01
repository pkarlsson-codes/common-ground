using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.PropertyRegistry.Application.Commands;
using CommonGround.PropertyRegistry.Application.Queries;
using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.DevelopmentProjects;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CommonGround.API.PropertyRegistry;

/// <summary>
/// API controller for development project management.
/// </summary>
[ApiController]
[Route("api/development-projects")]
public sealed class DevelopmentProjectsController(IMessageBus bus) : ControllerBase
{
    /// <summary>
    /// Gets a paginated list of development projects.
    /// </summary>
    /// <param name="request">Pagination and filter parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of development projects.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<DevelopmentProjectListItemResponse>), 200)]
    [EndpointName("getDevelopmentProjects")]
    public async Task<IActionResult> GetDevelopmentProjects(
        [FromQuery] PaginationQuerySpec<GetDevelopmentProjectsFilter> request,
        CancellationToken cancellationToken)
    {
        var query = new GetDevelopmentProjects(
            request.Page,
            request.Filter?.Search);

        var result = await bus.InvokeAsync<PaginatedResponse<DevelopmentProjectListItemView>>(query, cancellationToken);

        var response = result.MapItems(v => new DevelopmentProjectListItemResponse(
            v.Id, v.LandParcelId, v.Name, v.Description, v.Status, v.ProjectType));

        return Ok(response);
    }

    /// <summary>
    /// Initiates a new development project for a land parcel.
    /// </summary>
    /// <param name="request">The initiation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the newly created development project.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(InitiateDevelopmentProjectResponse), 201)]
    [EndpointName("initiateDevelopmentProject")]
    public async Task<IActionResult> InitiateDevelopmentProject(
        [FromBody] InitiateDevelopmentProjectRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = HttpContext.GetUserId();
        var landParcelId = new LandParcelId(request.LandParcelId);

        if (request.Type != "NewConstruction" && request.Type != "Rehabilitation")
        {
            return BadRequest($"Unrecognized project type: '{request.Type}'.");
        }

        DevelopmentProjectId id = request.Type switch
        {
            "NewConstruction" => await bus.InvokeAsync<DevelopmentProjectId>(
                new InitiateNewConstructionProject(landParcelId, request.Name, request.Description, userId),
                cancellationToken),
            _ => await bus.InvokeAsync<DevelopmentProjectId>(
                new InitiateRehabilitationProject(
                    landParcelId,
                    (request.ExistingHomes ?? []).Select(h => new HomeId(h)).ToList(),
                    request.Name,
                    request.Description,
                    userId),
                cancellationToken)
        };

        return CreatedAtAction(
            nameof(GetDevelopmentProjects),
            new { },
            new InitiateDevelopmentProjectResponse(id.Value));
    }
}
