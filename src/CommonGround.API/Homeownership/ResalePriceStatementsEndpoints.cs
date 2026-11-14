using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.API.Infrastructure.ExceptionHandling;
using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Homeownership;

public static class ResalePriceStatementsEndpoints
{
    /// <summary>The most leases one round preview prices.</summary>
    internal const int MaxRoundPreviewLeases = 1000;

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

        return Results.Ok(ToResponse(price));
    }

    [WolverinePost("/api/resale-price-statements/preview")]
    public static async Task<IResult> PreviewResalePriceStatements(
        PreviewResalePriceStatementsRequest request,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Leases.Count > MaxRoundPreviewLeases)
        {
            return Results.BadRequest($"A round preview prices at most {MaxRoundPreviewLeases} leases.");
        }

        // One lease at a time with the single-lease query, so a round prices each lease exactly as its own statement
        // would. A lease that can't be priced gets its refusal; the rest still get prices.
        var results = new List<ResalePriceStatementPreviewResponse>(request.Leases.Count);
        foreach (var lease in request.Leases)
        {
            try
            {
                var price = await bus.InvokeForTenantAsync<ResalePriceStatementPrice>(bus.TenantId!,
                    new GetResalePriceStatementPreview(
                        GroundLeaseId.Create(lease.GroundLeaseId),
                        request.StatementDate,
                        new ResalePriceStatementFigures(lease.MarketValue, lease.MarketValueSource, lease.IndexValue, null)),
                    cancellationToken);
                results.Add(new ResalePriceStatementPreviewResponse(lease.GroundLeaseId, ToResponse(price), null, null));
            }
            catch (BusinessRuleViolationException refusal)
            {
                results.Add(new ResalePriceStatementPreviewResponse(
                    lease.GroundLeaseId, null, refusal.Code, DomainExceptionHandler.GetUserFacingMessage(refusal)));
            }
        }

        return Results.Ok(new ResalePriceStatementPreviewsResponse(results));
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

    private static ResalePriceStatementPriceResponse ToResponse(ResalePriceStatementPrice price) => new(
        price.StatementDate,
        price.YearsOwned,
        price.MarketValue?.Amount,
        price.MarketValueSource,
        price.IndexValue,
        price.ImprovementsCredit?.Amount,
        price.Price.Amount);
}
