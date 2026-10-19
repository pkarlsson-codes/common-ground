using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Reacts to applicant selection by marking a home under contract.</summary>
public static class ApplicantSelectedHandler
{
    /// <summary>Creates a purchase workflow when an applicant is selected for a home.</summary>
    public static async Task Handle(
        ApplicationSelected @event,
        IDocumentSession session)
    {
        ArgumentNullException.ThrowIfNull(@event);
        
        var homeId = HomeId.Create(@event.HomeId);
        var home = await session.LoadAggregateAsync<Home>(homeId).ConfigureAwait(false);
        home.MarkUnderContract();
        session.UpdateAggregate(home);
    }
}