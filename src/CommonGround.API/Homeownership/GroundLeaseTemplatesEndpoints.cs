using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Homeownership;

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

        var id = GroundLeaseTemplateId.Generate();

        var command = new CreateGroundLeaseTemplate(
            id,
            request.TemplateName,
            request.TermYears,
            request.OptionWindowDays,
            request.CovenantTerms,
            resalePolicyTerms,
            LeaseFeeTerms.Create(Money.Create(request.LandUseFee), Money.Create(request.RepairReserveFee), request.LateInterestRate, request.InterestWaiverDays),
            userId);

        await bus.InvokeForTenantAsync(bus.TenantId!, command, cancellationToken);

        return Results.Created($"/api/ground-lease-templates/{id.Value}",
            new CreateGroundLeaseTemplateResponse(id.Value));
    }

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

    [WolverineGet("/api/ground-lease-templates/{id}")]
    public static async Task<IResult> GetGroundLeaseTemplate(
        string id,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var templateId = GroundLeaseTemplateId.Create(id);
        var view = await bus.InvokeForTenantAsync<GroundLeaseTemplateDetailView>(
            bus.TenantId!, new GetGroundLeaseTemplate(templateId), cancellationToken);
        return Results.Ok(new GroundLeaseTemplateResponse(
            view.Id, view.TemplateName, view.TermYears, view.OptionWindowDays, view.LandUseFee, view.RepairReserveFee,
            view.LateInterestRate, view.InterestWaiverDays,
            view.CovenantTerms, view.Status,
            view.FormulaKind, view.AppreciationShare, view.IndexType, view.BaselineIndexValue,
            view.AnnualPercentage, view.ImprovementsCap,
            view.CreatedAt, view.CreatedBy));
    }

    [WolverinePost("/api/ground-lease-templates/{id}/archive")]
    public static async Task<IResult> ArchiveGroundLeaseTemplate(
        string id,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!,
            new ArchiveGroundLeaseTemplate(GroundLeaseTemplateId.Create(id), userId), cancellationToken);
        return Results.NoContent();
    }

    [WolverinePost("/api/ground-lease-templates/{id}/restore")]
    public static async Task<IResult> RestoreGroundLeaseTemplate(
        string id,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RestoreGroundLeaseTemplate(GroundLeaseTemplateId.Create(id), userId), cancellationToken);
        return Results.NoContent();
    }
}
