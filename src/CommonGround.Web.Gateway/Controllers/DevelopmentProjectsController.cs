using CommonGround.API.Clients;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.Web.Gateway.Controllers;

/// <summary>Gateway proxy for development project endpoints.</summary>
/// <param name="client">Typed client for the PropertyRegistry API.</param>
[ApiController]
[Route("api/v1/development-projects")]
public sealed class DevelopmentProjectsController(IDevelopmentProjectsApiClient client) : ControllerBase
{
    /// <summary>Gets a paginated list of development projects.</summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="search">Optional free-text search filter.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with a paginated list of development projects.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<DevelopmentProjectListItemResponse>), 200)]
    [EndpointName("getDevelopmentProjects")]
    public async Task<IActionResult> GetDevelopmentProjects(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetDevelopmentProjectsAsync(page, pageSize, search, cancellationToken);
        return Ok(result);
    }
}
