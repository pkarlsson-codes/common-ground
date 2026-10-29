using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.TrustManagement.Application.Commands;
using Microsoft.AspNetCore.Authorization;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.TrustManagement;

public static class TrustsEndpoints
{
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

        var result = await bus.InvokeAsync<CreateTrustResult>(
            new CreateTrust(request.Name, request.TimeZone, userId, correlationId),
            cancellationToken).ConfigureAwait(false);

        return Results.Ok(new CreateTrustResponse(result.TrustId));
    }
}