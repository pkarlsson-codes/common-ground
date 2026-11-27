using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>
/// Records that staff checked an approved household's changed income or size and it still qualifies, so it can be
/// chosen again. The check itself happens outside the app, as at approval.
/// </summary>
/// <param name="ApplicationId">The application.</param>
/// <param name="ConfirmedDate">The day of the check.</param>
/// <param name="ConfirmedBy">The staff user confirming.</param>
public sealed record ConfirmQualification(
    HomeownerApplicationId ApplicationId,
    DateOnly ConfirmedDate,
    UserId ConfirmedBy);

/// <summary>The application to confirm, fetched for writing, and today in the trust's time zone.</summary>
public sealed record ConfirmQualificationDecision(IEventStream<HomeownerApplication> Application, DateOnly Today);

/// <summary>Handles <see cref="ConfirmQualification"/>.</summary>
public static class ConfirmQualificationHandler
{
    /// <summary>Fetches the application for writing and the trust's today.</summary>
    public static async Task<ConfirmQualificationDecision> LoadAsync(
        ConfirmQualification command,
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

        return new ConfirmQualificationDecision(application, today);
    }

    /// <summary>Confirms the household still qualifies.</summary>
    public static void Handle(ConfirmQualification command, ConfirmQualificationDecision decision)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(decision);

        decision.Application.Decide(a => a.ConfirmQualification(command.ConfirmedDate, decision.Today, command.ConfirmedBy));
    }
}
