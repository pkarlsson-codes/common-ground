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

    [WolverinePost("/api/v1/ground-leases/{groundLeaseId}/resale-price-statements/preview")]
    [ProducesResponseType<ResalePriceStatementPriceResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> PreviewResalePriceStatement(
        string groundLeaseId,
        PreviewResalePriceStatementRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        return Results.Ok(await client.PreviewResalePriceStatementAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false));
    }

    [WolverinePost("/api/v1/ground-leases/{groundLeaseId}/resale-price-statements")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RecordResalePriceStatement(
        string groundLeaseId,
        RecordResalePriceStatementRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordResalePriceStatementAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
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

    [WolverineGet("/api/v1/resale-price-statements/due")]
    [ProducesResponseType<ResalePriceStatementsDueResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetResalePriceStatementsDue(
        [FromQuery] bool? includeAll,
        [FromQuery] string? search,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetResalePriceStatementsDueAsync(includeAll ?? false, search, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/resale-price-statements/preview")]
    [ProducesResponseType<ResalePriceStatementPreviewsResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> PreviewResalePriceStatements(
        PreviewResalePriceStatementsRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Results.Ok(await client.PreviewResalePriceStatementsAsync(request, cancellationToken).ConfigureAwait(false));
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

    [WolverinePost("/api/v1/ground-leases/{groundLeaseId}/insurance-requirements")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> SetInsuranceRequirements(
        string groundLeaseId,
        SetInsuranceRequirementsRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.SetInsuranceRequirementsAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/ground-leases/{groundLeaseId}/mortgage-requests")]
    [ProducesResponseType<ReceiveMortgageRequestResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> ReceiveMortgageRequest(
        string groundLeaseId,
        ReceiveMortgageRequestRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.ReceiveMortgageRequestAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/mortgage-requests/{result.MortgageRequestId}", result);
    }

    [WolverineGet("/api/v1/ground-leases/{groundLeaseId}/mortgage-requests")]
    [ProducesResponseType<IReadOnlyList<MortgageRequestResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetMortgageRequests(
        string groundLeaseId,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        return Results.Ok(await client.GetMortgageRequestsAsync(groundLeaseId, cancellationToken).ConfigureAwait(false));
    }

    [WolverineGet("/api/v1/mortgage-requests/{mortgageRequestId}")]
    [ProducesResponseType<MortgageRequestResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetMortgageRequest(
        string mortgageRequestId,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);
        return Results.Ok(await client.GetMortgageRequestAsync(mortgageRequestId, cancellationToken).ConfigureAwait(false));
    }

    [WolverinePost("/api/v1/mortgage-requests/{mortgageRequestId}/loan-closed")]
    [ProducesResponseType<RecordMortgageLoanClosedResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> RecordMortgageLoanClosed(
        string mortgageRequestId,
        RecordMortgageLoanClosedRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);
        ArgumentNullException.ThrowIfNull(request);
        return Results.Ok(await client.RecordMortgageLoanClosedAsync(mortgageRequestId, request, cancellationToken).ConfigureAwait(false));
    }

    [WolverinePost("/api/v1/mortgage-requests/{mortgageRequestId}/appraisal")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RecordMortgageRequestAppraisal(
        string mortgageRequestId,
        RecordMortgageRequestAppraisalRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordMortgageRequestAppraisalAsync(mortgageRequestId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/mortgage-requests/{mortgageRequestId}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> ApproveMortgageRequest(
        string mortgageRequestId,
        ApproveMortgageRequestRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);
        ArgumentNullException.ThrowIfNull(request);
        await client.ApproveMortgageRequestAsync(mortgageRequestId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/mortgage-requests/{mortgageRequestId}/deny")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> DenyMortgageRequest(
        string mortgageRequestId,
        DenyMortgageRequestRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);
        ArgumentNullException.ThrowIfNull(request);
        await client.DenyMortgageRequestAsync(mortgageRequestId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/mortgage-requests/{mortgageRequestId}/withdraw")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> WithdrawMortgageRequest(
        string mortgageRequestId,
        WithdrawMortgageRequestRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);
        ArgumentNullException.ThrowIfNull(request);
        await client.WithdrawMortgageRequestAsync(mortgageRequestId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }
}
