using CommonGround.API.Clients;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class GroundLeasesEndpoints
{
    [WolverinePost("/api/v1/ground-leases")]
    [ProducesResponseType<CreateGroundLeaseResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> CreateGroundLease(
        CreateGroundLeaseRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.CreateGroundLeaseAsync(request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/ground-leases/{result.GroundLeaseId}", result);
    }

    [WolverineGet("/api/v1/ground-leases/{groundLeaseId}")]
    [ProducesResponseType<GroundLeaseResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetGroundLease(
        string groundLeaseId,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        var result = await client.GetGroundLeaseAsync(groundLeaseId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/ground-leases")]
    [ProducesResponseType<PaginatedResponse<GroundLeaseListItemResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetGroundLeases(
        [FromQuery] GetGroundLeasesQuerySpec querySpec,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetGroundLeasesAsync(querySpec, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/ground-leases/{groundLeaseId}/stewardship-check-ins")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RecordStewardshipCheckIn(
        string groundLeaseId,
        RecordStewardshipCheckInRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordStewardshipCheckInAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/ground-leases/{groundLeaseId}/resale-estimate")]
    [ProducesResponseType<ResaleEstimateResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> CalculateResaleEstimate(
        string groundLeaseId,
        CalculateResaleEstimateRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.CalculateResaleEstimateAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/ground-leases/{groundLeaseId}/lease-fee-payments")]
    [ProducesResponseType<RecordLeaseFeePaymentResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> RecordLeaseFeePayment(
        string groundLeaseId,
        RecordLeaseFeePaymentRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.RecordLeaseFeePaymentAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/ground-leases/{groundLeaseId}/lease-fee-payments/{result.PaymentId}", result);
    }

    [WolverinePost("/api/v1/ground-leases/{groundLeaseId}/lease-fee-payments/{paymentId}/void")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> VoidLeaseFeePayment(
        string groundLeaseId,
        string paymentId,
        VoidLeaseFeePaymentRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(paymentId);
        ArgumentNullException.ThrowIfNull(request);
        await client.VoidLeaseFeePaymentAsync(groundLeaseId, paymentId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverineGet("/api/v1/lease-fee-balances")]
    [ProducesResponseType<PaginatedResponse<LeaseFeeBalanceResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetLeaseFeeBalances(
        [FromQuery] GetLeaseFeeBalancesQuerySpec querySpec,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetLeaseFeeBalancesAsync(querySpec, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/ground-leases/{groundLeaseId}/lease-fee-notices")]
    [ProducesResponseType<RecordLeaseFeeNoticeResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> RecordLeaseFeeNotice(
        string groundLeaseId,
        RecordLeaseFeeNoticeRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.RecordLeaseFeeNoticeAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/ground-leases/{groundLeaseId}/lease-fee-notices/{result.NoticeId}", result);
    }

    [WolverinePost("/api/v1/ground-leases/{groundLeaseId}/lease-fee-interest-waivers")]
    [ProducesResponseType<WaiveLeaseFeeInterestResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> WaiveLeaseFeeInterest(
        string groundLeaseId,
        WaiveLeaseFeeInterestRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.WaiveLeaseFeeInterestAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/ground-leases/{groundLeaseId}/lease-fee-interest-waivers/{result.WaiverId}", result);
    }

    [WolverinePut("/api/v1/ground-leases/{groundLeaseId}/lease-fee-tracking")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> CorrectLeaseFeeTracking(
        string groundLeaseId,
        CorrectLeaseFeeTrackingRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.CorrectLeaseFeeTrackingAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverineGet("/api/v1/lease-fee-notices/due")]
    [ProducesResponseType<LeaseFeeNoticesDueResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetLeaseFeeNoticesDue(
        [FromQuery] string? groundLeaseId,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetLeaseFeeNoticesDueAsync(groundLeaseId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/lease-fee-notice-policy")]
    [ProducesResponseType<LeaseFeeNoticePolicyResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetLeaseFeeNoticePolicy(
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetLeaseFeeNoticePolicyAsync(cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePut("/api/v1/lease-fee-notice-policy")]
    [ProducesResponseType<LeaseFeeNoticePolicyResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> SetLeaseFeeNoticePolicy(
        SetLeaseFeeNoticePolicyRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.SetLeaseFeeNoticePolicyAsync(request, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/ground-leases/{groundLeaseId}/permitted-mortgages")]
    [ProducesResponseType<RecordPermittedMortgageResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> RecordPermittedMortgage(
        string groundLeaseId,
        RecordPermittedMortgageRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.RecordPermittedMortgageAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/ground-leases/{groundLeaseId}/permitted-mortgages/{result.MortgageId}", result);
    }

    [WolverinePut("/api/v1/ground-leases/{groundLeaseId}/permitted-mortgages/{mortgageId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> EditPermittedMortgage(
        string groundLeaseId,
        string mortgageId,
        EditPermittedMortgageRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageId);
        ArgumentNullException.ThrowIfNull(request);
        await client.EditPermittedMortgageAsync(groundLeaseId, mortgageId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/ground-leases/{groundLeaseId}/permitted-mortgages/{mortgageId}/paid-off")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> MarkPermittedMortgagePaidOff(
        string groundLeaseId,
        string mortgageId,
        MarkPermittedMortgagePaidOffRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageId);
        ArgumentNullException.ThrowIfNull(request);
        await client.MarkPermittedMortgagePaidOffAsync(groundLeaseId, mortgageId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }
}
