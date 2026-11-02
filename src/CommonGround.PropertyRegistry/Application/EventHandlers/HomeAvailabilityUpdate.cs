using CommonGround.PropertyRegistry.Application.ReadModels;
using Marten;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Loads and writes a home's <see cref="HomeAvailabilityView"/> for the handlers that keep it.</summary>
public static class HomeAvailabilityUpdate
{
    /// <summary>Loads the home's availability, if any news of its sale has arrived yet.</summary>
    public static Task<HomeAvailabilityView?> LoadAsync(IDocumentSession session, string homeId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        return session.LoadAsync<HomeAvailabilityView>(homeId, cancellationToken);
    }

    /// <summary>
    /// Records news on the home's availability, starting it when this is the first. The view is stored with
    /// optimistic concurrency, so news handled at the same time is retried rather than overwritten.
    /// </summary>
    public static IMartenOp Record(string homeId, HomeAvailabilityView? availability, Action<HomeAvailabilityView> record)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentNullException.ThrowIfNull(record);

        var view = availability ?? new HomeAvailabilityView { Id = homeId };
        record(view);

        return availability is null ? MartenOps.Insert(view) : MartenOps.Update(view);
    }
}
