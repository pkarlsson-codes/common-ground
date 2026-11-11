using Wolverine.Postgresql.Transport;
using Wolverine.Runtime;
using Wolverine.Runtime.Routing;
using Endpoint = Wolverine.Configuration.Endpoint;

namespace CommonGround.API.Infrastructure.Messaging;

/// <summary>
/// Sends every integration event to one PostgreSQL queue. It is a routing convention, not a
/// <c>Publish</c> rule, because only conventional routing keeps the local handler routes as well
/// (<c>ConventionalLocalRoutingIsAdditive</c>); a <c>Publish</c> rule replaces them.
/// </summary>
/// <param name="queue">The queue that integration events go to.</param>
internal sealed class PostgresIntegrationEventRouting(PostgresqlQueue queue) : IMessageRoutingConvention
{
    /// <inheritdoc />
    public void DiscoverListeners(IWolverineRuntime runtime, IReadOnlyList<Type> handledMessageTypes)
    {
        // A service split out of the API listens to the queue; the API only sends to it.
    }

    /// <inheritdoc />
    public IEnumerable<Endpoint> DiscoverSenders(Type messageType, IWolverineRuntime runtime) =>
        IntegrationEvents.Includes(messageType) ? [queue] : [];
}
