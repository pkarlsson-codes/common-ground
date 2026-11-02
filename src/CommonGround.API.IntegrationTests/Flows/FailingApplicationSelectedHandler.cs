using System.Collections.Concurrent;
using CommonGround.HomeBuyers.Contracts.Events;

namespace CommonGround.API.IntegrationTests.Flows;

/// <summary>
/// A third reaction to <see cref="ApplicationSelected"/>, registered only on the durable test host,
/// that fails for the homes a test names, to show one bounded context's failure leaves the others' work
/// committed.
/// </summary>
public static class FailingApplicationSelectedHandler
{
    /// <summary>Gets the homes whose selection this handler fails on.</summary>
    public static ConcurrentDictionary<string, bool> FailForHomes { get; } = new();

    /// <summary>Fails if the selected home is one a test named.</summary>
    public static void Handle(ApplicationSelected @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (FailForHomes.ContainsKey(@event.HomeId))
            throw new InvalidOperationException($"Failing on purpose for home {@event.HomeId}.");
    }
}
