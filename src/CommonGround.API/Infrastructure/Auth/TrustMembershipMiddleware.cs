using CommonGround.Identity.Application.Queries;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.API.Infrastructure.Auth;

/// <summary>
/// Refuses a request made inside a trust the user no longer belongs to. The gateway keeps the trust on the
/// session for up to 7 days, so without this check a removed member keeps access until the session ends.
/// </summary>
internal sealed class TrustMembershipMiddleware(RequestDelegate next)
{
    /// <summary>The <c>code</c> of the 403 problem. The gateway and the web app look for it.</summary>
    public const string NotAMemberCode = "identity.trust_membership.ended";

    /// <summary>A header on the 403, so the gateway can drop the trust from the session without reading the body.</summary>
    public const string MembershipHeader = "X-Trust-Membership";

    /// <summary>Checks the user's membership when the token carries both a user and a trust.</summary>
    /// <param name="context">The request.</param>
    /// <param name="store">The document store that holds users.</param>
    /// <param name="problemDetails">Writes the 403 problem.</param>
    /// <returns>A task that completes when the request is handled.</returns>
    public async Task InvokeAsync(HttpContext context, IDocumentStore store, IProblemDetailsService problemDetails)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(problemDetails);

        var userId = context.User.FindFirst("user_id")?.Value;
        var trustId = context.User.FindFirst("tenant_id")?.Value;

        if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(trustId))
        {
            var session = store.QuerySession();
            await using var sessionScope = session.ConfigureAwait(false);
            if (!await TrustMembership.AllowsAsync(session, userId, trustId, context.RequestAborted).ConfigureAwait(false))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.Headers[MembershipHeader] = "ended";
                await problemDetails.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = new ProblemDetails
                    {
                        Title = "Not a member",
                        Detail = "You are no longer a member of this trust.",
                        Status = StatusCodes.Status403Forbidden,
                        Extensions = { ["code"] = NotAMemberCode },
                    },
                }).ConfigureAwait(false);
                return;
            }
        }

        await next(context).ConfigureAwait(false);
    }
}
