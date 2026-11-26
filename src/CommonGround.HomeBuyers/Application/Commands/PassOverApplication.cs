using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Records that a home selection passed over an application: another household was chosen while it could have been.</summary>
public sealed record PassOverApplication(
    HomeownerApplicationId ApplicationId,
    HomeSelectionId SelectionId,
    string HomeId);

/// <summary>Handles <see cref="PassOverApplication"/>.</summary>
public static class PassOverApplicationHandler
{
    /// <summary>Fetches the application for writing.</summary>
    public static async Task<IEventStream<HomeownerApplication>> LoadAsync(
        PassOverApplication command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);

        var application = await session.Events
            .FetchForWriting<HomeownerApplication>(command.ApplicationId.Value, cancellationToken)
            .ConfigureAwait(false);
        _ = application.Aggregate
            ?? throw new AggregateNotFoundException(typeof(HomeownerApplication), command.ApplicationId.Value);

        return application;
    }

    /// <summary>
    /// Records the pass-over. Does nothing if it was already recorded for this selection, so a retried message is
    /// harmless. Whether the household could have been chosen was settled when the selection was recorded.
    /// </summary>
    public static void Handle(PassOverApplication command, IEventStream<HomeownerApplication> application)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(application);

        application.Decide(a => a.PassOver(command.SelectionId, command.HomeId));
    }
}
