using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.ImprovementRequests;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Infrastructure.API;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Homeownership;

/// <summary>Homeowners' requests to alter or add to the home, and the trust's replies (Model Lease Art 7.3).</summary>
public static class ImprovementRequestsEndpoints
{
    [WolverinePost("/api/ground-leases/{groundLeaseId}/improvement-requests")]
    public static async Task<IResult> ReceiveImprovementRequest(
        string groundLeaseId,
        ReceiveImprovementRequestRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);

        var id = ImprovementRequestId.Generate();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new ReceiveImprovementRequest(
                id,
                GroundLeaseId.Create(groundLeaseId),
                request.ReceivedOn,
                request.Description,
                Money.Create(request.EstimatedCost),
                httpContext.GetUserId()),
            cancellationToken);

        return Results.Created($"/api/improvement-requests/{id.Value}", new ReceiveImprovementRequestResponse(id.Value));
    }

    [WolverineGet("/api/ground-leases/{groundLeaseId}/improvement-requests")]
    public static async Task<IResult> GetImprovementRequests(string groundLeaseId, IMessageBus bus, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);

        var list = await bus.InvokeForTenantAsync<ImprovementRequestList>(bus.TenantId!,
            new GetImprovementRequests(GroundLeaseId.Create(groundLeaseId)), cancellationToken);

        return Results.Ok(list.Requests.Select(ToResponse).ToList());
    }

    [WolverineGet("/api/improvement-requests/{improvementRequestId}")]
    public static async Task<IResult> GetImprovementRequest(string improvementRequestId, IMessageBus bus, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(improvementRequestId);

        var view = await bus.InvokeForTenantAsync<ImprovementRequestView>(bus.TenantId!,
            new GetImprovementRequest(ImprovementRequestId.Create(improvementRequestId)), cancellationToken);

        return Results.Ok(ToResponse(view));
    }

    [WolverinePost("/api/improvement-requests/{improvementRequestId}/approve")]
    public static async Task<IResult> ApproveImprovementRequest(
        string improvementRequestId,
        ApproveImprovementRequestRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(improvementRequestId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new ApproveImprovementRequest(
                ImprovementRequestId.Create(improvementRequestId),
                request.ApprovedOn,
                Money.Create(request.PreApprovedCredit),
                request.Notes,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/improvement-requests/{improvementRequestId}/deny")]
    public static async Task<IResult> DenyImprovementRequest(
        string improvementRequestId,
        DenyImprovementRequestRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(improvementRequestId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new DenyImprovementRequest(ImprovementRequestId.Create(improvementRequestId), request.DeniedOn, request.Reason, httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/improvement-requests/{improvementRequestId}/completed")]
    public static async Task<IResult> RecordImprovementCompleted(
        string improvementRequestId,
        RecordImprovementCompletedRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(improvementRequestId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordImprovementCompleted(
                ImprovementRequestId.Create(improvementRequestId),
                request.CompletedOn,
                Money.Create(request.ActualCost),
                request.ReceiptDocumentIds,
                Money.Create(request.ConfirmedCredit),
                request.Notes,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/improvement-requests/{improvementRequestId}/withdraw")]
    public static async Task<IResult> WithdrawImprovementRequest(
        string improvementRequestId,
        WithdrawImprovementRequestRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(improvementRequestId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new WithdrawImprovementRequest(ImprovementRequestId.Create(improvementRequestId), request.WithdrawnOn, request.Reason, httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    private static ImprovementRequestResponse ToResponse(ImprovementRequestView v) => new(
        v.Id,
        v.GroundLeaseId,
        v.HomeId,
        v.HomeownerFullName,
        v.Status,
        v.ReceivedOn,
        v.ReplyDueOn,
        v.Description,
        v.EstimatedCost,
        v.ApprovedOn,
        v.PreApprovedCredit,
        v.LeaseImprovementsCap,
        v.CreditOnLeaseBefore,
        v.ApprovalNotes,
        v.DeniedOn,
        v.DenialReason,
        v.CompletedOn,
        v.ActualCost,
        v.ReceiptDocumentIds,
        v.ConfirmedCredit,
        v.CompletionNotes,
        v.WithdrawnOn,
        v.WithdrawnReason,
        v.RecordedAt);
}
