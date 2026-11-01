using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.PropertyRegistry.Contracts.Events;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>Seeds <see cref="HomeLabelView"/> from PropertyRegistry's HomeRegistered integration event.</summary>
public static class HomeRegisteredHandler
{
    /// <summary>Records the home's label so purchases can name it.</summary>
    public static IMartenOp Handle(HomeRegistered @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return MartenOps.Store(new HomeLabelView { Id = @event.HomeId, Label = @event.Label });
    }
}
