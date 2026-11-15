using CommonGround.API.Clients;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class ResalesEndpoints
{
    [WolverinePost("/api/v1/ground-leases/{groundLeaseId}/intent-to-sell")]
    [ProducesResponseType<RecordIntentToSellResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> RecordIntentToSell(
        string groundLeaseId,
        RecordIntentToSellRequest request,
        IResalesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.RecordIntentToSellAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/resales/{result.ResaleId}", result);
    }

    [WolverinePost("/api/v1/resales/{resaleId}/withdraw")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> WithdrawIntentToSell(
        string resaleId,
        WithdrawIntentToSellRequest request,
        IResalesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);
        await client.WithdrawIntentToSellAsync(resaleId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverineGet("/api/v1/resales")]
    [ProducesResponseType<PaginatedResponse<ResaleResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetResales(
        [FromQuery] GetResalesQuerySpec querySpec,
        IResalesApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetResalesAsync(querySpec, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/resales/{resaleId}")]
    [ProducesResponseType<ResaleResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetResale(
        string resaleId,
        IResalesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        var result = await client.GetResaleAsync(resaleId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/resales/{resaleId}/price-preview")]
    [ProducesResponseType<ResalePriceResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> PreviewResalePrice(
        string resaleId,
        PreviewResalePriceRequest request,
        IResalesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.PreviewResalePriceAsync(resaleId, request, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/resales/{resaleId}/appraisal")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RecordResaleAppraisal(
        string resaleId,
        RecordResaleAppraisalRequest request,
        IResalesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordResaleAppraisalAsync(resaleId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/resales/{resaleId}/exercise-option")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> ExercisePurchaseOption(
        string resaleId,
        ExercisePurchaseOptionRequest request,
        IResalesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);
        await client.ExercisePurchaseOptionAsync(resaleId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/resales/{resaleId}/buyback-closing")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RecordBuybackClosed(
        string resaleId,
        RecordBuybackClosedRequest request,
        IResalesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordBuybackClosedAsync(resaleId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverineGet("/api/v1/resales/{resaleId}/payoff-statement")]
    [ProducesResponseType<PayoffStatementResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetPayoffStatement(
        string resaleId,
        [FromQuery] DateOnly goodThrough,
        IResalesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        var result = await client.GetPayoffStatementAsync(resaleId, goodThrough, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/resales/{resaleId}/payoff-statements")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RecordPayoffStatement(
        string resaleId,
        RecordPayoffStatementRequest request,
        IResalesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordPayoffStatementAsync(resaleId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }
}
