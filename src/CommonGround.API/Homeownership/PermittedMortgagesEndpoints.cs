using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Infrastructure.API;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Homeownership;

/// <summary>The mortgages the trust permitted on a ground lease (Model Lease Art 8).</summary>
public static class PermittedMortgagesEndpoints
{
    [WolverinePost("/api/ground-leases/{groundLeaseId}/permitted-mortgages")]
    public static async Task<IResult> RecordPermittedMortgage(
        string groundLeaseId,
        RecordPermittedMortgageRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);

        var id = PermittedMortgageId.Generate();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordPermittedMortgage(
                GroundLeaseId.Create(groundLeaseId),
                id,
                request.Lender,
                request.NoticeAddress,
                Money.Create(request.OriginalPrincipal),
                request.PermittedOn,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.Created(
            $"/api/ground-leases/{groundLeaseId}/permitted-mortgages/{id.Value}",
            new RecordPermittedMortgageResponse(id.Value));
    }

    [WolverinePut("/api/ground-leases/{groundLeaseId}/permitted-mortgages/{mortgageId}")]
    public static async Task<IResult> EditPermittedMortgage(
        string groundLeaseId,
        string mortgageId,
        EditPermittedMortgageRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new EditPermittedMortgage(
                GroundLeaseId.Create(groundLeaseId),
                PermittedMortgageId.Create(mortgageId),
                request.Lender,
                request.NoticeAddress,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/ground-leases/{groundLeaseId}/permitted-mortgages/{mortgageId}/paid-off")]
    public static async Task<IResult> MarkPermittedMortgagePaidOff(
        string groundLeaseId,
        string mortgageId,
        MarkPermittedMortgagePaidOffRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new MarkPermittedMortgagePaidOff(
                GroundLeaseId.Create(groundLeaseId),
                PermittedMortgageId.Create(mortgageId),
                request.PaidOffOn,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }
}
