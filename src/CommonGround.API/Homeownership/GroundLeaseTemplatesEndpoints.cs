using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Homeownership;

/// <summary>Endpoints for managing ground lease templates.</summary>
public static class GroundLeaseTemplatesEndpoints
{
    /// <summary>
    /// Handles GET requests to retrieve a paginated list of ground lease templates, with optional filtering by name and status. The endpoint accepts query parameters for pagination, search, and status, and returns a paginated response containing summary information about each ground lease template.
    /// </summary>
    /// <param name="request">The query parameters for pagination, search, and status filtering.</param>
    /// <param name="bus">The message bus used to send the query.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated response containing summary information about each ground lease template.</returns>
    [WolverineGet("/api/ground-lease-templates")]
    public static async Task<IResult> GetGroundLeaseTemplates(
        [FromQuery] GetGroundLeaseTemplatesQuerySpec request,
        IMessageBus bus,
        CancellationToken cancellationToken = default)
    {
        var query = new GetGroundLeaseTemplates(request.Page, request.Search, request.Status);

        var result = await bus.InvokeForTenantAsync<PaginatedResponse<GroundLeaseTemplateListItemView>>(
            bus.TenantId!, query, cancellationToken);

        var response = result.MapItems(v => new GroundLeaseTemplateListItemResponse(
            v.Id,
            v.TemplateName,
            v.Status));

        return Results.Ok(response);
    }
}
