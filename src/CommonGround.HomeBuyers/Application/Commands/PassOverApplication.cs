using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>
/// Passes over a candidate's application in a home selection that chose another household,
/// if the candidate could still have been selected on <paramref name="SelectedDate"/>.
/// </summary>
public sealed record PassOverApplication(
    HomeownerApplicationId ApplicationId,
    HomeSelectionId SelectionId,
    string HomeId,
    DateOnly SelectedDate);

/// <summary>A passed-over candidate's application, fetched for writing, and their household.</summary>
/// <param name="Application">The candidate's application stream.</param>
/// <param name="Household">The candidate's household, read-only.</param>
public sealed record PassOverTarget(IEventStream<HomeownerApplication> Application, Household Household);

/// <summary>Handles <see cref="PassOverApplication"/>.</summary>
public static class PassOverApplicationHandler
{
    /// <summary>Fetches the application for writing and loads its household.</summary>
    public static async Task<PassOverTarget> LoadAsync(
        PassOverApplication command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);

        var application = await session.Events
            .FetchForWriting<HomeownerApplication>(command.ApplicationId.Value, cancellationToken)
            .ConfigureAwait(false);
        var current = application.Aggregate
            ?? throw new AggregateNotFoundException(typeof(HomeownerApplication), command.ApplicationId.Value);
        var household = await session
            .LoadAggregateAsync<Household>(current.HouseholdId, cancellationToken)
            .ConfigureAwait(false);

        return new PassOverTarget(application, household);
    }

    /// <summary>
    /// Records the pass-over. Does nothing if the candidate withdrew, their readiness lapsed, or
    /// they were already passed over in this selection, so a retried message is harmless.
    /// </summary>
    public static void Handle(PassOverApplication command, PassOverTarget target)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(target);

        target.Application.Decide(application =>
            application.PassOver(command.SelectionId, command.HomeId, target.Household, command.SelectedDate));
    }
}
