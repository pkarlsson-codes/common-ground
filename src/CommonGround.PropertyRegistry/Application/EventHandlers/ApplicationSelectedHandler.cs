using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.Homes.Events;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Reacts to applicant selection by marking a home under contract.</summary>
public static class ApplicationSelectedHandler
{
    /// <summary>Fetches the home's stream for writing.</summary>
    public static Task<IEventStream<Home>> LoadAsync(ApplicationSelected @event, IDocumentSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(session);

        return session.Events.FetchForWriting<Home>(@event.HomeId, cancellationToken);
    }

    /// <summary>Marks the home under contract from the day the applicant was selected.</summary>
    public static void Handle(ApplicationSelected @event, IEventStream<Home> stream)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(stream);

        stream.Decide(home =>
        {
            return home.MarkUnderContract(@event.SelectedDate);
        });
    }
}
