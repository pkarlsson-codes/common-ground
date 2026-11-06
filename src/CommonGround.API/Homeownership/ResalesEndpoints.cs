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

        return Results.Ok(result.MapItems(item => new ResaleResponse(
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
            item.Resale.RecordedAt)));
    }
}
