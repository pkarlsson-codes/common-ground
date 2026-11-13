using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Homeownership;

public static class ResalePriceStatementsEndpoints
{
    [WolverinePost("/api/ground-leases/{groundLeaseId}/resale-price-statements/preview")]
    public static async Task<IResult> PreviewResalePriceStatement(
        string groundLeaseId,
        PreviewResalePriceStatementRequest request,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);

        var price = await bus.InvokeForTenantAsync<ResalePriceStatementPrice>(bus.TenantId!,
            new GetResalePriceStatementPreview(
                GroundLeaseId.Create(groundLeaseId),
                request.StatementDate,
                new ResalePriceStatementFigures(request.MarketValue, request.MarketValueSource, request.IndexValue, request.ImprovementsCredit)),
            cancellationToken);

        return Results.Ok(new ResalePriceStatementPriceResponse(
            price.StatementDate,
            price.YearsOwned,
            price.MarketValue?.Amount,
            price.MarketValueSource,
            price.IndexValue,
            price.ImprovementsCredit?.Amount,
            price.Price.Amount));
    }

    [WolverinePost("/api/ground-leases/{groundLeaseId}/resale-price-statements")]
    public static async Task<IResult> RecordResalePriceStatement(
        string groundLeaseId,
        RecordResalePriceStatementRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);

        if (!NoticeDelivery.TryFromDisplayName(request.Delivery, out var delivery))
        {
            return Results.BadRequest($"Unrecognized notice delivery: '{request.Delivery}'.");
        }

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordResalePriceStatement(
                GroundLeaseId.Create(groundLeaseId),
                request.StatementDate,
                new ResalePriceStatementFigures(request.MarketValue, request.MarketValueSource, request.IndexValue, request.ImprovementsCredit),
                delivery,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverineGet("/api/resale-price-statements/due")]
    public static async Task<IResult> GetResalePriceStatementsDue(
        [FromQuery] bool? includeAll,
        [FromQuery] string? search,
        IMessageBus bus,
        CancellationToken cancellationToken = default)
    {
        var result = await bus.InvokeForTenantAsync<ResalePriceStatementsDue>(
            bus.TenantId!, new GetResalePriceStatementsDue(includeAll ?? false, search), cancellationToken);

        var leases = result.Leases.Select(s => new ResalePriceStatementLeaseResponse(
            s.Lease.Id,
            s.Lease.HomeownerFullName,
            s.Lease.HomeId,
            s.HomeLabel,
            s.Lease.LeaseStartDate,
            s.Lease.BasePrice,
            s.Lease.OriginalAppraisedValue,
            s.Lease.FormulaKind,
            s.Lease.AppreciationShare,
            s.Lease.IndexType,
            s.Lease.BaselineIndexValue,
            s.Lease.AnnualPercentage,
            s.Lease.ImprovementsCap,
            s.LastStatementDate,
            s.NextDueOn)).ToList();

        return Results.Ok(new ResalePriceStatementsDueResponse(result.Today, result.LetterSignOff, leases));
    }
}
