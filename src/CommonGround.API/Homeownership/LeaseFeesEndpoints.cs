using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Homeownership;

public static class LeaseFeesEndpoints
{
    [WolverinePost("/api/ground-leases/{groundLeaseId}/lease-fee-payments")]
    public static async Task<IResult> RecordLeaseFeePayment(
        string groundLeaseId,
        RecordLeaseFeePaymentRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);

        if (!PaymentMethod.TryFromDisplayName(request.Method, out var method))
        {
            return Results.BadRequest($"Unrecognized payment method: '{request.Method}'.");
        }

        var id = LeaseFeePaymentId.Generate();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordLeaseFeePayment(
                GroundLeaseId.Create(groundLeaseId),
                id,
                request.ReceivedDate,
                request.Amount,
                method,
                request.Reference,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.Created(
            $"/api/ground-leases/{groundLeaseId}/lease-fee-payments/{id.Value}",
            new RecordLeaseFeePaymentResponse(id.Value));
    }

    [WolverinePost("/api/ground-leases/{groundLeaseId}/lease-fee-payments/{paymentId}/void")]
    public static async Task<IResult> VoidLeaseFeePayment(
        string groundLeaseId,
        string paymentId,
        VoidLeaseFeePaymentRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(paymentId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new VoidLeaseFeePayment(
                GroundLeaseId.Create(groundLeaseId),
                LeaseFeePaymentId.Create(paymentId),
                request.Reason,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverineGet("/api/lease-fee-balances")]
    public static async Task<IResult> GetLeaseFeeBalances(
        [FromQuery] GetLeaseFeeBalancesQuerySpec request,
        IMessageBus bus,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = new GetLeaseFeeBalances(
            request.Page,
            request.IncludeAll,
            request.Search,
            request.NoticeDueOnly);

        var result = await bus.InvokeForTenantAsync<PaginatedResponse<LeaseFeeBalance>>(bus.TenantId!, query, cancellationToken);

        return Results.Ok(result.MapItems(ToResponse));
    }

    [WolverinePost("/api/ground-leases/{groundLeaseId}/lease-fee-notices")]
    public static async Task<IResult> RecordLeaseFeeNotice(
        string groundLeaseId,
        RecordLeaseFeeNoticeRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);

        if (!LeaseFeeNoticeLevel.TryFromDisplayName(request.Level, out var level))
        {
            return Results.BadRequest($"Unrecognized notice level: '{request.Level}'.");
        }

        if (!NoticeDelivery.TryFromDisplayName(request.Delivery, out var delivery))
        {
            return Results.BadRequest($"Unrecognized notice delivery: '{request.Delivery}'.");
        }

        var id = LeaseFeeNoticeId.Generate();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordLeaseFeeNotice(
                GroundLeaseId.Create(groundLeaseId),
                id,
                level,
                request.SentOn,
                delivery,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.Created(
            $"/api/ground-leases/{groundLeaseId}/lease-fee-notices/{id.Value}",
            new RecordLeaseFeeNoticeResponse(id.Value));
    }

    [WolverineGet("/api/lease-fee-notices/due")]
    public static async Task<IResult> GetLeaseFeeNoticesDue(
        [FromQuery] string? groundLeaseId,
        IMessageBus bus,
        CancellationToken cancellationToken = default)
    {
        var result = await bus.InvokeForTenantAsync<LeaseFeeNoticesDue>(
            bus.TenantId!, new GetLeaseFeeNoticesDue(groundLeaseId), cancellationToken);

        return Results.Ok(new LeaseFeeNoticesDueResponse(
            result.Today,
            result.LetterSignOff,
            [.. result.Notices.Select(ToResponse)]));
    }

    [WolverineGet("/api/lease-fee-notice-policy")]
    public static async Task<IResult> GetLeaseFeeNoticePolicy(
        IMessageBus bus,
        CancellationToken cancellationToken = default)
    {
        var policy = await bus.InvokeForTenantAsync<LeaseFeeNoticePolicyDetails>(
            bus.TenantId!, new GetLeaseFeeNoticePolicy(), cancellationToken);

        return Results.Ok(ToResponse(policy));
    }

    [WolverinePut("/api/lease-fee-notice-policy")]
    public static async Task<IResult> SetLeaseFeeNoticePolicy(
        SetLeaseFeeNoticePolicyRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new SetLeaseFeeNoticePolicy(
                LeaseFeeNoticeSchedule.Create(request.FirstNoticeDays, request.SecondNoticeDays),
                request.LetterSignOff,
                httpContext.GetUserId()),
            cancellationToken);

        var policy = await bus.InvokeForTenantAsync<LeaseFeeNoticePolicyDetails>(
            bus.TenantId!, new GetLeaseFeeNoticePolicy(), cancellationToken);

        return Results.Ok(ToResponse(policy));
    }

    private static LeaseFeeNoticePolicyResponse ToResponse(LeaseFeeNoticePolicyDetails policy) =>
        new(policy.Schedule.FirstNoticeDays, policy.Schedule.SecondNoticeDays, policy.LetterSignOff, policy.IsDefault);

    private static LeaseFeeBalanceResponse ToResponse(LeaseFeeBalance b) =>
        new(
            b.Lease.Id,
            b.Lease.HomeId,
            b.HomeLabel,
            b.Lease.HomeownerFullName,
            b.Lease.Status,
            b.Statement.MonthlyFee,
            b.Statement.Balance,
            b.Statement.OldestUnpaidDueDate,
            b.Statement.DaysOverdue,
            b.NoticeDue?.DisplayName,
            b.LastNotice?.Level,
            b.LastNotice?.SentOn);
}
