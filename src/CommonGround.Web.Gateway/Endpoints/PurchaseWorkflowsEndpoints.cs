using CommonGround.API.Clients;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Wolverine.Http;

#pragma warning disable CA2007
#pragma warning disable SA1600

namespace CommonGround.Web.Gateway.Endpoints;

public static class PurchaseWorkflowsEndpoints
{
    [WolverinePost("/api/v1/purchase-workflows")]
    public static async Task<IResult> InitiatePurchaseWorkflow(
        InitiatePurchaseWorkflowRequest request,
        IPurchaseWorkflowsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.InitiatePurchaseWorkflowAsync(request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/purchase-workflows/{result.PurchaseWorkflowId}", result);
    }

    [WolverineGet("/api/v1/purchase-workflows/{workflowId}")]
    public static async Task<IResult> GetPurchaseWorkflow(
        string workflowId,
        IPurchaseWorkflowsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        var result = await client.GetPurchaseWorkflowAsync(workflowId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/purchase-workflows")]
    public static async Task<IResult> GetPurchaseWorkflows(
        int pageNumber,
        int pageSize,
        string? groundLeaseId,
        string? phase,
        string? search,
        IPurchaseWorkflowsApiClient client,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0)
        {
            pageNumber = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 20;
        }

        var result = await client.GetPurchaseWorkflowsAsync(pageNumber, pageSize, groundLeaseId, phase, search, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/purchase-workflows/{workflowId}/eligibility")]
    public static async Task<IResult> RecordEligibilityVerified(
        string workflowId,
        RecordEligibilityVerifiedRequest request,
        IPurchaseWorkflowsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordEligibilityVerifiedAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/purchase-workflows/{workflowId}/financing")]
    public static async Task<IResult> RecordFinancingCommitted(
        string workflowId,
        RecordFinancingCommittedRequest request,
        IPurchaseWorkflowsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordFinancingCommittedAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/purchase-workflows/{workflowId}/inspection")]
    public static async Task<IResult> RecordInspectionCompleted(
        string workflowId,
        RecordInspectionCompletedRequest request,
        IPurchaseWorkflowsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordInspectionCompletedAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/purchase-workflows/{workflowId}/title")]
    public static async Task<IResult> RecordTitleCleared(
        string workflowId,
        RecordTitleClearedRequest request,
        IPurchaseWorkflowsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordTitleClearedAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/purchase-workflows/{workflowId}/lease-execution")]
    public static async Task<IResult> RecordLeaseExecuted(
        string workflowId,
        RecordLeaseExecutedRequest request,
        IPurchaseWorkflowsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordLeaseExecutedAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/purchase-workflows/{workflowId}/closing/schedule")]
    public static async Task<IResult> ScheduleClosing(
        string workflowId,
        ScheduleClosingRequest request,
        IPurchaseWorkflowsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.ScheduleClosingAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/purchase-workflows/{workflowId}/closing/record")]
    public static async Task<IResult> RecordPurchaseClosed(
        string workflowId,
        RecordPurchaseClosedRequest request,
        IPurchaseWorkflowsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordPurchaseClosedAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/purchase-workflows/{workflowId}/fail")]
    public static async Task<IResult> FailPurchase(
        string workflowId,
        FailPurchaseRequest request,
        IPurchaseWorkflowsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.FailPurchaseAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/purchase-workflows/{workflowId}/withdraw")]
    public static async Task<IResult> WithdrawPurchase(
        string workflowId,
        WithdrawPurchaseRequest request,
        IPurchaseWorkflowsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.WithdrawPurchaseAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/purchase-workflows/{workflowId}/stall")]
    public static async Task<IResult> StallWorkflow(
        string workflowId,
        StallWorkflowRequest request,
        IPurchaseWorkflowsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.StallWorkflowAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }
}
