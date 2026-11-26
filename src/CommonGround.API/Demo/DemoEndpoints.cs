using CommonGround.API.Contracts.Demo.Requests;
using CommonGround.API.Contracts.Demo.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Identity.Application.Queries;
using CommonGround.Identity.Application.ReadModels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Wolverine.Http;

namespace CommonGround.API.Demo;

/// <summary>Demo mode endpoints.</summary>
public static class DemoEndpoints
{
    // For the signed-out Welcome page, so it needs no sign-in. 404 when demo mode is off.
    [WolverineGet("/api/demo/mode")]
    [NotTenanted]
    [AllowAnonymous]
    public static IResult GetMode(IOptions<DemoOptions> options) =>
        options.Value.Enabled ? Results.Ok(new DemoModeResponse(options.Value.SandboxDays)) : Results.NotFound();

    // 404 when demo mode is off or the trust has no sandbox record.
    [WolverineGet("/api/demo")]
    public static async Task<IResult> GetDemo(
        HttpContext httpContext,
        IOptions<DemoOptions> options,
        IDocumentStore store,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        var demo = options.Value;
        if (!demo.Enabled)
        {
            return Results.NotFound();
        }

        var trustId = httpContext.GetTenantId().Value;
        var session = store.QuerySession();
        await using var sessionDisposal = session.ConfigureAwait(false);
        var sandbox = await session.LoadAsync<DemoSandbox>(trustId, cancellationToken).ConfigureAwait(false);
        if (sandbox is null)
        {
            return Results.NotFound();
        }

        var deletesOn = await trustClock.ToTrustDateAsync(trustId, sandbox.DeletesAt, cancellationToken).ConfigureAwait(false);
        return Results.Ok(new DemoResponse(deletesOn));
    }

    // Saves what a visitor wrote with "Send feedback", outside their sandbox trust so it outlives it.
    // 404 when demo mode is off or the trust has no sandbox record, like the banner it is sent from.
    [WolverinePost("/api/demo/feedback")]
    public static async Task<IResult> SendFeedback(
        SendDemoFeedbackRequest request,
        HttpContext httpContext,
        IOptions<DemoOptions> options,
        IDocumentStore store,
        ILogger<DemoFeedback> logger,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!options.Value.Enabled)
        {
            return Results.NotFound();
        }

        var trustId = httpContext.GetTenantId().Value;
        var userId = httpContext.GetUserId().Value;
        var session = store.LightweightSession();
        await using var sessionDisposal = session.ConfigureAwait(false);
        if (await session.LoadAsync<DemoSandbox>(trustId, cancellationToken).ConfigureAwait(false) is null)
        {
            return Results.NotFound();
        }

        string? replyTo = null;
        if (request.MayReply)
        {
            var user = await session.Query<UserView>()
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken)
                .ConfigureAwait(false);
            replyTo = user?.Email;
        }

        var feedback = DemoFeedback.Send(trustId, userId, replyTo, request.Page, request.Message, TimeProviderContext.Current.UtcNow);
        session.Store(feedback);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Demo feedback {FeedbackId} sent from trust {TrustId}", feedback.Id, trustId);
        }

        return Results.NoContent();
    }

    // Starts a sample trust for a user who has no trust. A user whose sample trust is still being filled gets that one back.
    [WolverinePost("/api/demo/sandboxes")]
    [NotTenanted]
    [Authorize]
    public static async Task<IResult> StartSandbox(
        HttpContext httpContext,
        IOptions<DemoOptions> options,
        IDocumentStore store,
        DemoSandboxBacklog queue,
        ILogger<DemoSandbox> logger,
        CancellationToken cancellationToken)
    {
        var demo = options.Value;
        if (!demo.Enabled)
        {
            return Results.NotFound();
        }

        if (httpContext.TryGetTenantId() is not null)
        {
            return Results.Conflict(new { error = "User already belongs to a trust." });
        }

        var userId = httpContext.GetUserId().Value;
        var session = store.LightweightSession();
        await using var sessionDisposal = session.ConfigureAwait(false);

        var latest = await LatestSandboxAsync(session, userId, cancellationToken).ConfigureAwait(false);
        if (latest is { Status: DemoSandboxStatus.Seeding })
        {
            return Results.Ok(ToResponse(latest));
        }

        await DemoCapacity.EnsureRoomAsync(session, demo, cancellationToken).ConfigureAwait(false);

        var sandbox = DemoSandbox.StartSample(TrustId.Generate().Value, userId, TimeProviderContext.Current.UtcNow, demo.SandboxDays);
        session.Store(sandbox);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        queue.Enqueue(sandbox.Id);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Demo sandbox {TrustId} started by {UserId} (run {RunKey}), deleted after {DeletesAt}", sandbox.Id, userId, sandbox.RunKey, sandbox.DeletesAt);
        }

        return Results.Ok(ToResponse(sandbox));
    }

    // The signed-in user's latest sandbox. 404 when demo mode is off, so the app knows not to offer sample data.
    // A ready sandbox the user was removed from counts as none: the gateway would put it back on the session.
    [WolverineGet("/api/demo/sandboxes/current")]
    [NotTenanted]
    [Authorize]
    public static async Task<IResult> GetCurrentSandbox(
        HttpContext httpContext,
        IOptions<DemoOptions> options,
        IDocumentStore store,
        CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return Results.NotFound();
        }

        var session = store.QuerySession();
        await using var sessionDisposal = session.ConfigureAwait(false);
        var userId = httpContext.GetUserId().Value;
        var latest = await LatestSandboxAsync(session, userId, cancellationToken).ConfigureAwait(false);
        if (latest is { Status: DemoSandboxStatus.Ready }
            && !await TrustMembership.AllowsAsync(session, userId, latest.Id, cancellationToken).ConfigureAwait(false))
        {
            latest = null;
        }

        return Results.Ok(new CurrentDemoSandboxResponse(latest is null ? null : ToResponse(latest)));
    }

    private static async Task<DemoSandbox?> LatestSandboxAsync(IQuerySession session, string userId, CancellationToken cancellationToken) =>
        await session.Query<DemoSandbox>()
            .Where(s => s.OwnerUserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

    private static DemoSandboxResponse ToResponse(DemoSandbox sandbox) => new(sandbox.Id, sandbox.Status.ToString());
}
