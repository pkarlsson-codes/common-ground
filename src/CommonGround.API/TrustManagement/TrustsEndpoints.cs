using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;
using CommonGround.API.Demo;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Identity.Application.Commands;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.TrustManagement.Application.Commands;
using CommonGround.TrustManagement.Application.Queries;
using CommonGround.TrustManagement.Domain.Trusts;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
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
        IOptions<DemoOptions> demoOptions,
        IDocumentStore store,
        ILogger<DemoSandbox> logger,
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

        // On a demo server every trust is a sandbox. Its record is stored first, so a trust never exists
        // without a delete date.
        var demo = demoOptions.Value;
        if (demo.Enabled)
        {
            var sandbox = DemoSandbox.Start(trustId.Value, userId.Value, TimeProviderContext.Current.UtcNow, demo.SandboxDays, DemoSandboxStatus.Ready);
            var session = store.LightweightSession();
            await using var sessionDisposal = session.ConfigureAwait(false);
            await DemoCapacity.EnsureRoomAsync(session, demo, cancellationToken).ConfigureAwait(false);
            session.Store(sandbox);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Demo sandbox {TrustId} created empty by {UserId}, deleted after {DeletesAt}", sandbox.Id, sandbox.OwnerUserId, sandbox.DeletesAt);
            }
        }

        await bus.InvokeAsync(
            new CreateTrust(trustId, request.Name, request.TimeZone, userId, correlationId),
            cancellationToken).ConfigureAwait(false);

        // Assigned here as well as from TrustOnboardingRequested, so the membership check passes on the next request.
        await bus.InvokeAsync(new AssignUserToTrustIfNone(userId, trustId), cancellationToken).ConfigureAwait(false);

        return Results.Ok(new CreateTrustResponse(trustId.Value));
    }
}