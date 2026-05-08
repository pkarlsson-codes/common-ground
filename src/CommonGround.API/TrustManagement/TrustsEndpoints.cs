using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.TrustManagement.Application.Commands;
using Microsoft.AspNetCore.Authorization;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.TrustManagement;

/// <summary>
/// Wolverine HTTP endpoints for trust management operations.
/// </summary>
public static class TrustsEndpoints
{
    /// <summary>
    /// Creates a new trust.
    /// </summary>
    /// <param name="request">The request containing the trust name.</param>
    /// <param name="httpContext">The current HTTP context used to extract identity claims.</param>
    /// <param name="bus">The Wolverine message bus.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stable ID of the newly created trust.</returns>
    [WolverinePost("/api/trusts")]
    [NotTenanted]
    [Authorize(Policy = "ServiceToken")]
    public static async Task<IResult> CreateTrust(
        CreateTrustRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();
        var correlationId = httpContext.GetCorrelationId() ?? httpContext.TraceIdentifier;

        var result = await bus.InvokeAsync<CreateTrustResult>(
            new CreateTrust(request.Name, userId, correlationId),
            cancellationToken).ConfigureAwait(false);

        return Results.Ok(new CreateTrustResponse(result.TrustId));
    }
}