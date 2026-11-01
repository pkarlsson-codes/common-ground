using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Homeownership;

public static class PurchaseWorkflowsEndpoints
{
    [WolverinePost("/api/purchase-workflows")]
    public static async Task<IResult> InitiatePurchaseWorkflow(
        InitiatePurchaseWorkflowRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        var command = new InitiatePurchaseWorkflow(
            GroundLeaseTemplateId.Create(request.GroundLeaseTemplateId),
            request.LandParcelId,
            request.HomeId,
            new Homeowner(request.BuyerFullName, request.BuyerEmail, request.BuyerPhone),
            request.PurchasePrice,
            userId);

        var id = await bus.InvokeForTenantAsync<PurchaseWorkflowId>(bus.TenantId!, command, cancellationToken);

        return Results.Created($"/api/purchase-workflows/{id.Value}", new InitiatePurchaseWorkflowResponse(id.Value));
    }

    [WolverineGet("/api/purchase-workflows")]
    public static async Task<IResult> GetPurchaseWorkflows(
        [FromQuery] GetPurchaseWorkflowsQuerySpec request,
        IMessageBus bus,
        CancellationToken cancellationToken = default)
    {
        var groundLeaseTemplateFilter = request.GroundLeaseTemplateId is not null
            ? GroundLeaseTemplateId.Create(request.GroundLeaseTemplateId)
            : null;

        var query = new GetPurchaseWorkflows(
            request.Page, groundLeaseTemplateFilter, request.Phase, request.Search, request.HomeId, request.HouseholdId);

        var result = await bus.InvokeForTenantAsync<PaginatedResponse<PurchaseWorkflowListItem>>(bus.TenantId!, query, cancellationToken);

        var response = result.MapItems(i => new PurchaseWorkflowListItemResponse(
            i.Workflow.Id,
            i.Workflow.GroundLeaseTemplateId,
            i.Workflow.HomeId,
            i.HomeLabel,
            i.Workflow.HouseholdId,
            i.Workflow.BuyerFullName,
            i.Workflow.PurchasePrice,
            i.Workflow.Phase,
            i.Workflow.CreatedAt,
            i.Workflow.StepEnteredAt,
            i.Workflow.ScheduledClosingDate));

        return Results.Ok(response);
    }

    [WolverineGet("/api/purchase-workflows/{workflowId}")]
    public static async Task<IResult> GetPurchaseWorkflow(
        string workflowId,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);

        var (view, homeLabel) = await bus.InvokeForTenantAsync<PurchaseWorkflowDetails>(bus.TenantId!,
            new GetPurchaseWorkflow(PurchaseWorkflowId.Create(workflowId)),
            cancellationToken);

        return Results.Ok(MapToResponse(view, homeLabel));
    }

    [WolverinePost("/api/purchase-workflows/{workflowId}/eligibility")]
    public static async Task<IResult> RecordEligibilityVerified(
        string workflowId,
        RecordEligibilityVerifiedRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordEligibilityVerified(
                PurchaseWorkflowId.Create(workflowId),
                request.VerifiedDate,
                request.Notes,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/purchase-workflows/{workflowId}/financing")]
    public static async Task<IResult> RecordFinancingCommitted(
        string workflowId,
        RecordFinancingCommittedRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordFinancingCommitted(
                PurchaseWorkflowId.Create(workflowId),
                request.LenderName,
                request.CommittedAmount,
                request.PreapprovalLetterDate,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/purchase-workflows/{workflowId}/inspection")]
    public static async Task<IResult> RecordInspectionCompleted(
        string workflowId,
        RecordInspectionCompletedRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordInspectionCompleted(
                PurchaseWorkflowId.Create(workflowId),
                request.InspectionDate,
                request.InspectorName,
                request.Notes,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/purchase-workflows/{workflowId}/title")]
    public static async Task<IResult> RecordTitleCleared(
        string workflowId,
        RecordTitleClearedRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordTitleCleared(
                PurchaseWorkflowId.Create(workflowId),
                request.ClearedDate,
                request.TitleCompanyName,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/purchase-workflows/{workflowId}/lease-execution")]
    public static async Task<IResult> RecordLeaseExecuted(
        string workflowId,
        RecordLeaseExecutedRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordLeaseExecuted(
                PurchaseWorkflowId.Create(workflowId),
                request.SignedDate,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/purchase-workflows/{workflowId}/closing/schedule")]
    public static async Task<IResult> ScheduleClosing(
        string workflowId,
        ScheduleClosingRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new ScheduleClosing(
                PurchaseWorkflowId.Create(workflowId),
                request.ScheduledClosingDate,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/purchase-workflows/{workflowId}/closing/record")]
    public static async Task<IResult> RecordPurchaseClosed(
        string workflowId,
        RecordPurchaseClosedRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordPurchaseClosed(
                PurchaseWorkflowId.Create(workflowId),
                request.ActualClosingDate,
                request.Notes,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/purchase-workflows/{workflowId}/fail")]
    public static async Task<IResult> FailPurchase(
        string workflowId,
        FailPurchaseRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new FailPurchase(
                PurchaseWorkflowId.Create(workflowId),
                request.FailureReason,
                request.FailedDate,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/purchase-workflows/{workflowId}/withdraw")]
    public static async Task<IResult> WithdrawPurchase(
        string workflowId,
        WithdrawPurchaseRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new WithdrawPurchase(
                PurchaseWorkflowId.Create(workflowId),
                request.WithdrawnDate,
                request.Reason,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/purchase-workflows/{workflowId}/stall")]
    public static async Task<IResult> StallWorkflow(
        string workflowId,
        StallWorkflowRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new StallWorkflow(
                PurchaseWorkflowId.Create(workflowId),
                request.StalledReason,
                request.StalledDate,
                request.ResolutionTargetDate,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    private static PurchaseWorkflowResponse MapToResponse(PurchaseWorkflowView view, string? homeLabel)
    {
        var eligibility = view.EligibilityDetails is null ? null
            : new EligibilityDetailsDto(view.EligibilityDetails.VerifiedDate, view.EligibilityDetails.Notes);
        var financing = view.FinancingDetails is null ? null
            : new FinancingDetailsDto(view.FinancingDetails.LenderName, view.FinancingDetails.CommittedAmount, view.FinancingDetails.PreapprovalLetterDate);
        var inspection = view.InspectionResult is null ? null
            : new InspectionResultDto(view.InspectionResult.InspectionDate, view.InspectionResult.InspectorName, view.InspectionResult.Notes);
        var title = view.TitleDetails is null ? null
            : new TitleDetailsDto(view.TitleDetails.ClearedDate, view.TitleDetails.TitleCompanyName);
        var leaseExecution = view.LeaseExecutionDetails is null ? null
            : new LeaseExecutionDetailsDto(view.LeaseExecutionDetails.SignedDate);

        return new PurchaseWorkflowResponse(
            view.Id,
            view.GroundLeaseTemplateId,
            view.LandParcelId,
            view.HomeId,
            homeLabel,
            view.ApplicationId,
            view.HouseholdId,
            view.Phase,
            view.InitiatedBy,
            view.BuyerFullName,
            view.BuyerEmail,
            view.BuyerPhone,
            view.PurchasePrice,
            view.CreatedAt,
            eligibility,
            financing,
            inspection,
            title,
            leaseExecution,
            view.ScheduledClosingDate,
            view.ActualClosingDate,
            view.ClosingNotes,
            view.FailureReason,
            view.WithdrawalReason,
            view.StalledReason,
            view.StalledDate,
            view.ResolutionTargetDate);
    }
}
