using CommonGround.API.Contracts.Demo.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.SharedKernel.Domain.Services;
using Marten;
using Microsoft.Extensions.Options;
using Wolverine.Http;

namespace CommonGround.API.Demo;

/// <summary>Demo mode endpoints.</summary>
public static class DemoEndpoints
{
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
        return Results.Ok(new DemoResponse(deletesOn, demo.FeedbackUrl!));
    }
}
