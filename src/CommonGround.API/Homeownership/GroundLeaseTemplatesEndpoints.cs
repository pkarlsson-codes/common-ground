using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

#pragma warning disable CA2007
#pragma warning disable SA1600

namespace CommonGround.API.Homeownership;

/// <summary>Endpoints for managing ground lease templates.</summary>
public static class GroundLeaseTemplatesEndpoints
{
    [WolverinePost("/api/ground-lease-templates")]
    public static async Task<IResult> CreateGroundLeaseTemplate(
        CreateGroundLeaseTemplateRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        var resalePolicyTerms = ResalePolicyTerms.FromString(
            request.FormulaKind,
            request.AppreciationShare,
            request.IndexType,
            request.BaselineIndexValue,
            request.AnnualPercentage,
            request.ImprovementsCap);

        var command = new CreateGroundLeaseTemplate(
            request.TemplateName,
            request.TermYears,
            request.CovenantTerms,
            resalePolicyTerms,
            userId);

        var id = await bus.InvokeForTenantAsync<GroundLeaseTemplateId>(bus.TenantId!, command, cancellationToken);

        return Results.Created($"/api/ground-lease-templates/{id.Value}", new CreateGroundLeaseTemplateResponse(id.Value));
    }

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

#pragma warning restore CA2007
#pragma warning restore SA1600
