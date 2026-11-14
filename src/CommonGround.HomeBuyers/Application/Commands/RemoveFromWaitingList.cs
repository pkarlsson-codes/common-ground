using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using Marten;
using JasperFx.Events;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Takes an approved application off the waiting list.</summary>
/// <param name="ApplicationId">The application.</param>
/// <param name="Reason">Why.</param>
/// <param name="Note">Staff's note; required when <paramref name="Reason"/> is Other.</param>
/// <param name="RemovedDate">The day of removal.</param>
/// <param name="RecordedBy">The staff user recording it.</param>
public sealed record RemoveFromWaitingList(
    HomeownerApplicationId ApplicationId,
    WaitingListRemovalReason Reason,
    string? Note,
    DateOnly RemovedDate,
    UserId RecordedBy);

/// <summary>The application to remove, fetched for writing, and today in the trust's time zone.</summary>
public sealed record RemoveFromWaitingListDecision(IEventStream<HomeownerApplication> Application, DateOnly Today);

/// <summary>Handles <see cref="RemoveFromWaitingList"/>.</summary>
public static class RemoveFromWaitingListHandler
{
    /// <summary>Fetches the application for writing and the trust's today.</summary>
    public static async Task<RemoveFromWaitingListDecision> LoadAsync(
        RemoveFromWaitingList command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var application = await session.Events
            .FetchForWriting<HomeownerApplication>(command.ApplicationId.Value, cancellationToken)
            .ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new RemoveFromWaitingListDecision(application, today);
    }

    /// <summary>Removes the application from the waiting list.</summary>
    public static void Handle(RemoveFromWaitingList command, RemoveFromWaitingListDecision decision)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(decision);

        decision.Application.Decide(a => a.RemoveFromWaitingList(
            command.Reason, command.Note, command.RemovedDate, decision.Today, command.RecordedBy));
    }
}
