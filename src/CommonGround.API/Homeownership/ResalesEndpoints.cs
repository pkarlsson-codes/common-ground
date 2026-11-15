using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Homeownership;

public static class ResalesEndpoints
{
    [WolverinePost("/api/ground-leases/{groundLeaseId}/intent-to-sell")]
    public static async Task<IResult> RecordIntentToSell(
        string groundLeaseId,
        RecordIntentToSellRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);

        if (!NoticeChannel.TryFromDisplayName(request.Channel, out var channel))
        {
            return Results.BadRequest($"Unrecognized notice channel: '{request.Channel}'.");
        }

        var id = ResaleId.Generate();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordIntentToSell(
                id,
                GroundLeaseId.Create(groundLeaseId),
                request.NoticeDate,
                channel,
                request.Notes,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.Created($"/api/resales/{id.Value}", new RecordIntentToSellResponse(id.Value));
    }

    [WolverinePost("/api/resales/{resaleId}/withdraw")]
    public static async Task<IResult> WithdrawIntentToSell(
        string resaleId,
        WithdrawIntentToSellRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new WithdrawIntentToSell(
                ResaleId.Create(resaleId),
                request.WithdrawnDate,
                request.Reason,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverineGet("/api/resales")]
    public static async Task<IResult> GetResales(
        [FromQuery] GetResalesQuerySpec request,
        IMessageBus bus,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ResaleStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status) && !ResaleStatus.TryFromDisplayName(request.Status, out status))
        {
            return Results.BadRequest($"Unrecognized resale status: '{request.Status}'.");
        }

        var query = new GetResales(
            request.Page,
            status,
            EntityIds.CreateOrNull<GroundLeaseId>(request.GroundLeaseId));

        var result = await bus.InvokeForTenantAsync<PaginatedResponse<ResaleListItem>>(bus.TenantId!, query, cancellationToken);

        return Results.Ok(result.MapItems(ToResponse));
    }

    [WolverineGet("/api/resales/{resaleId}")]
    public static async Task<IResult> GetResale(
        string resaleId,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);

        var item = await bus.InvokeForTenantAsync<ResaleListItem>(bus.TenantId!, new GetResale(ResaleId.Create(resaleId)), cancellationToken);

        return Results.Ok(ToResponse(item));
    }

    [WolverinePost("/api/resales/{resaleId}/price-preview")]
    public static async Task<IResult> PreviewResalePrice(
        string resaleId,
        PreviewResalePriceRequest request,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);

        var price = await bus.InvokeForTenantAsync<ResalePrice>(bus.TenantId!,
            new GetResalePricePreview(
                ResaleId.Create(resaleId),
                Money.Create(request.AppraisedValue),
                request.AppraisalDate,
                Money.CreateOrNull(request.ApprovedImprovements),
                request.CurrentIndexValue),
            cancellationToken);

        return Results.Ok(new ResalePriceResponse(price.FormulaPrice.Amount, price.PurchaseOptionPrice.Amount));
    }

    [WolverinePost("/api/resales/{resaleId}/appraisal")]
    public static async Task<IResult> RecordResaleAppraisal(
        string resaleId,
        RecordResaleAppraisalRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordResaleAppraisal(
                ResaleId.Create(resaleId),
                Money.Create(request.AppraisedValue),
                request.AppraisalDate,
                request.AppraiserName,
                Money.CreateOrNull(request.ApprovedImprovements),
                request.CurrentIndexValue,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/resales/{resaleId}/exercise-option")]
    public static async Task<IResult> ExercisePurchaseOption(
        string resaleId,
        ExercisePurchaseOptionRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new ExercisePurchaseOption(ResaleId.Create(resaleId), request.ExercisedDate, request.Notes, httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverineGet("/api/resales/{resaleId}/payoff-statement")]
    public static async Task<IResult> GetPayoffStatement(
        string resaleId,
        [FromQuery] DateOnly goodThrough,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);

        var statement = await bus.InvokeForTenantAsync<PayoffStatement>(
            bus.TenantId!, new GetPayoffStatement(ResaleId.Create(resaleId), goodThrough), cancellationToken);

        return Results.Ok(new PayoffStatementResponse(
            statement.GoodThrough,
            statement.FeesOwed.Amount,
            statement.InterestOwed.Amount,
            statement.Credit.Amount,
            statement.TotalOwed.Amount,
            statement.OldestUnpaidMonth));
    }

    [WolverinePost("/api/resales/{resaleId}/payoff-statements")]
    public static async Task<IResult> RecordPayoffStatement(
        string resaleId,
        RecordPayoffStatementRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordPayoffStatement(ResaleId.Create(resaleId), request.GoodThrough, httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/resales/{resaleId}/buyback-closing")]
    public static async Task<IResult> RecordBuybackClosed(
        string resaleId,
        RecordBuybackClosedRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordBuybackClosed(
                ResaleId.Create(resaleId),
                request.ClosingDate,
                Money.Create(request.PricePaid),
                request.Notes,
                request.OfferForSale,
                request.HoldReason,
                request.FeesPaidFromProceeds is { } feesPaid ? Money.Create(feesPaid) : null,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    private static ResaleResponse ToResponse(ResaleListItem item) => new(
        item.Resale.Id,
        item.Resale.GroundLeaseId,
        item.Resale.LandParcelId,
        item.Resale.HomeId,
        item.HomeLabel,
        item.Resale.HomeownerFullName,
        item.Resale.Status,
        item.Resale.NoticeDate,
        item.Resale.NoticeChannel,
        item.Resale.Notes,
        item.Resale.OptionWindowEndsOn,
        item.Resale.WithdrawnDate,
        item.Resale.WithdrawnReason,
        item.Resale.RecordedAt,
        item.Resale.AppraisedValue,
        item.Resale.AppraisalDate,
        item.Resale.AppraiserName,
        item.Resale.ApprovedImprovements,
        item.Resale.CurrentIndexValue,
        item.Resale.FormulaPrice,
        item.Resale.PurchaseOptionPrice,
        item.Resale.OptionExercisedDate,
        item.Resale.OptionExercisedNotes,
        item.Resale.ClosingDate,
        item.Resale.PricePaid,
        item.Resale.ClosingNotes,
        item.Resale.OfferedForSale,
        item.Resale.HoldReason,
        item.Resale.FeesPaidFromProceeds,
        item.Resale.PayoffStatements.Select(p => new PayoffStatementPrintDto(p.PrintedAt, p.PrintedBy, p.GoodThrough, p.TotalOwed)).ToList());
}
