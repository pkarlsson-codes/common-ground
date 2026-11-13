using CommonGround.Homeownership.Domain.MortgageRequests;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Gives the trust's written permission for a requested loan.</summary>
/// <param name="MortgageRequestId">The request.</param>
/// <param name="ApprovedOn">The day the trust approved it.</param>
/// <param name="Reason">Why the trust approved it; required when the debt is over the cap.</param>
/// <param name="ApprovedBy">The staff user recording it.</param>
public sealed record ApproveMortgageRequest(MortgageRequestId MortgageRequestId, DateOnly ApprovedOn, string? Reason, UserId ApprovedBy);

/// <summary>Refuses a requested loan.</summary>
/// <param name="MortgageRequestId">The request.</param>
/// <param name="DeniedOn">The day the trust refused it.</param>
/// <param name="Reason">Why, for the denial letter.</param>
/// <param name="DeniedBy">The staff user recording it.</param>
public sealed record DenyMortgageRequest(MortgageRequestId MortgageRequestId, DateOnly DeniedOn, string Reason, UserId DeniedBy);

/// <summary>Records that the homeowner dropped a requested loan before it closed.</summary>
/// <param name="MortgageRequestId">The request.</param>
/// <param name="WithdrawnOn">The day the homeowner withdrew it.</param>
/// <param name="Reason">Why the homeowner withdrew it.</param>
/// <param name="WithdrawnBy">The staff user recording it.</param>
public sealed record WithdrawMortgageRequest(MortgageRequestId MortgageRequestId, DateOnly WithdrawnOn, string Reason, UserId WithdrawnBy);

/// <summary>What approving, denying or withdrawing a request decides from.</summary>
/// <param name="Request">The request stream, fetched for writing.</param>
/// <param name="Today">The trust's today.</param>
public sealed record MortgageRequestDecision(IEventStream<MortgageRequest> Request, DateOnly Today)
{
    /// <summary>Fetches the request for writing and the trust's today.</summary>
    public static async Task<MortgageRequestDecision> LoadAsync(
        MortgageRequestId requestId,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var request = await session.Events
            .FetchForWriting<MortgageRequest>(requestId.Value, cancellationToken)
            .ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new MortgageRequestDecision(request, today);
    }
}

/// <summary>Handles <see cref="ApproveMortgageRequest"/>.</summary>
public static class ApproveMortgageRequestHandler
{
    /// <summary>Fetches the request for writing and the trust's today.</summary>
    public static Task<MortgageRequestDecision> LoadAsync(
        ApproveMortgageRequest command, IDocumentSession session, ITrustClock trustClock, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return MortgageRequestDecision.LoadAsync(command.MortgageRequestId, session, trustClock, cancellationToken);
    }

    /// <summary>Approves the request.</summary>
    public static void Handle(ApproveMortgageRequest command, MortgageRequestDecision decision)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(decision);

        decision.Request.Decide(r => r.Approve(command.ApprovedOn, command.Reason, decision.Today, command.ApprovedBy));
    }
}

/// <summary>Handles <see cref="DenyMortgageRequest"/>.</summary>
public static class DenyMortgageRequestHandler
{
    /// <summary>Fetches the request for writing and the trust's today.</summary>
    public static Task<MortgageRequestDecision> LoadAsync(
        DenyMortgageRequest command, IDocumentSession session, ITrustClock trustClock, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return MortgageRequestDecision.LoadAsync(command.MortgageRequestId, session, trustClock, cancellationToken);
    }

    /// <summary>Denies the request.</summary>
    public static void Handle(DenyMortgageRequest command, MortgageRequestDecision decision)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(decision);

        decision.Request.Decide(r => r.Deny(command.DeniedOn, command.Reason, decision.Today, command.DeniedBy));
    }
}

/// <summary>Handles <see cref="WithdrawMortgageRequest"/>.</summary>
public static class WithdrawMortgageRequestHandler
{
    /// <summary>Fetches the request for writing and the trust's today.</summary>
    public static Task<MortgageRequestDecision> LoadAsync(
        WithdrawMortgageRequest command, IDocumentSession session, ITrustClock trustClock, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return MortgageRequestDecision.LoadAsync(command.MortgageRequestId, session, trustClock, cancellationToken);
    }

    /// <summary>Withdraws the request.</summary>
    public static void Handle(WithdrawMortgageRequest command, MortgageRequestDecision decision)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(decision);

        decision.Request.Decide(r => r.Withdraw(command.WithdrawnOn, command.Reason, decision.Today, command.WithdrawnBy));
    }
}
