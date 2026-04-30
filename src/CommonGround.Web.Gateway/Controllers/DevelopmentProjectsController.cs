using CommonGround.API.Clients;
using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.Web.Gateway.Controllers;

/// <summary>Gateway proxy for development project endpoints.</summary>
/// <param name="client">Typed client for the PropertyRegistry API.</param>
[ApiController]
[Route("api/v1/development-projects")]
public sealed class DevelopmentProjectsController(IDevelopmentProjectsApiClient client) : ControllerBase
{
    /// <summary>Gets a paginated list of development projects.</summary>
    /// <param name="request">Pagination and filter parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with a paginated list of development projects.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<DevelopmentProjectListItemResponse>), 200)]
    [EndpointName("getDevelopmentProjects")]
    public async Task<IActionResult> GetDevelopmentProjects(
        [FromQuery] PaginationQuerySpec<GetDevelopmentProjectsFilter> request,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetDevelopmentProjectsAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Initiates a new development project for a land parcel.</summary>
    /// <param name="request">The initiation request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>201 with the ID of the newly created development project.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(InitiateDevelopmentProjectResponse), 201)]
    [EndpointName("initiateDevelopmentProject")]
    public async Task<IActionResult> InitiateDevelopmentProject(
        [FromBody] InitiateDevelopmentProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await client.InitiateDevelopmentProjectAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetDevelopmentProjects), new { }, result);
    }
}
