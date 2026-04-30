using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.PropertyRegistry.Application.ReadModels;
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
        var query = new CommonGround.PropertyRegistry.Application.Queries.GetDevelopmentProjects(
            request.Page,
            request.Filter?.Search);

        var result = await bus.InvokeAsync<PaginatedResponse<DevelopmentProjectListItemView>>(query, cancellationToken);

        var response = result.MapItems(v => new DevelopmentProjectListItemResponse(
            v.Id, v.LandParcelId, v.Description, v.Status));

        return Ok(response);
    }
}
