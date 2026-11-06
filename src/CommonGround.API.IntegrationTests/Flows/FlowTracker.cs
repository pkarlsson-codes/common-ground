using FluentAssertions;
using Wolverine.Marten;
using Wolverine.Tracking;

namespace CommonGround.API.IntegrationTests.Flows;

/// <summary>
/// Runs a step of a flow on the <see cref="DurableApiFactory"/> host and waits for the events it leads to
/// to reach every handler. Create one per test.
/// </summary>
internal sealed class FlowTracker(DurableApiFactory factory)
{
    private static readonly string FailingHandlerQueue = nameof(FailingApplicationSelectedHandler).ToLowerInvariant();

    private readonly List<string> _deadLettered = [];

    /// <summary>Gets the queues of the messages <see cref="FailingApplicationSelectedHandler"/> dead-lettered.</summary>
    public IReadOnlyList<string> FailedOnPurpose => _deadLettered;

    /// <summary>
    /// Runs <paramref name="action"/>, then waits for the events it leads to to reach every handler. A
    /// catch-up forwards only the events stored so far, and handlers append more (a fallen-through
    /// purchase starts a new selection), so it catches up again until a round forwards nothing. Fails
    /// if a handler dead-letters a message, other than <see cref="FailingApplicationSelectedHandler"/>,
    /// whose failures are kept in <see cref="FailedOnPurpose"/>.
    /// </summary>
    public async Task TrackAsync(Func<Task> action)
    {
        var session = await TrackRoundAsync(action);
        for (var round = 0; session.AllRecordsInOrder().Any(r => r.MessageEventType == MessageEventType.Sent); round++)
        {
            round.Should().BeLessThan(10, "the events a step leads to should settle");
            session = await TrackRoundAsync(() => Task.CompletedTask);
        }
    }

    /// <summary>Runs <paramref name="action"/> as <see cref="TrackAsync(Func{Task})"/> does and returns its result.</summary>
    public async Task<T> TrackAsync<T>(Func<Task<T>> action)
    {
        T result = default!;
        await TrackAsync(async () => { result = await action(); });
        return result;
    }

    private async Task<ITrackedSession> TrackRoundAsync(Func<Task> action)
    {
        var session = await factory.Services.TrackActivity(TimeSpan.FromSeconds(30))
            .PauseThenCatchUpOnMartenDaemonActivity()
            .ExecuteAndWaitAsync(_ => action());

        var deadLettered = session.AllRecordsInOrder()
            .Where(r => r.MessageEventType == MessageEventType.MovedToErrorQueue)
            .Select(r => r.Envelope!.Destination!.ToString())
            .ToList();
        var failedOnPurpose = deadLettered.Where(d => d.Contains(FailingHandlerQueue, StringComparison.Ordinal)).ToList();
        deadLettered.Except(failedOnPurpose).Should().BeEmpty("no handler should fail");
        _deadLettered.AddRange(failedOnPurpose);

        return session;
    }
}
