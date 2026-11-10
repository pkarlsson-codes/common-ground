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
            request.Search);

        var result = await bus.InvokeForTenantAsync<PaginatedResponse<LeaseFeeBalance>>(bus.TenantId!, query, cancellationToken);

        return Results.Ok(result.MapItems(b => new LeaseFeeBalanceResponse(
            b.Lease.Id,
            b.Lease.HomeId,
            b.HomeLabel,
            b.Lease.HomeownerFullName,
            b.Lease.Status,
            b.Statement.MonthlyFee,
            b.Statement.Balance,
            b.Statement.OldestUnpaidDueDate,
            b.Statement.DaysOverdue)));
    }
}
