using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.TrustManagement.Application.Commands;
using CommonGround.TrustManagement.Application.Queries;
using CommonGround.TrustManagement.Domain.Trusts;
using Microsoft.AspNetCore.Authorization;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.TrustManagement;

public static class TrustsEndpoints
{
    [WolverineGet("/api/trusts/current")]
    public static async Task<IResult> GetCurrentTrust(
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var trustId = httpContext.GetTenantId();

        var trust = await bus.InvokeAsync<Trust?>(new GetTrust(trustId.Value), cancellationToken).ConfigureAwait(false);

        return trust is null
            ? Results.NotFound()
            : Results.Ok(new TrustResponse(trust.Id.Value, trust.Name, trust.TimeZone.Value));
    }

    [WolverinePost("/api/trusts")]
    [NotTenanted]
    [Authorize]
    public static async Task<IResult> CreateTrust(
        CreateTrustRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (httpContext.TryGetTenantId() is not null)
        {
            return Results.Conflict(new { error = "User already belongs to a trust." });
        }

        var userId = httpContext.GetUserId();
        var correlationId = httpContext.GetCorrelationId() ?? httpContext.TraceIdentifier;

        var trustId = TrustId.Generate();
        await bus.InvokeAsync(
            new CreateTrust(trustId, request.Name, request.TimeZone, userId, correlationId),
            cancellationToken).ConfigureAwait(false);

        return Results.Ok(new CreateTrustResponse(trustId.Value));
    }
}