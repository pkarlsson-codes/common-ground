using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Recommissions a decommissioned parcel.</summary>
public sealed record RecommissionParcel(LandParcelId LandParcelId, UserId ChangedBy);

/// <summary>Handles <see cref="RecommissionParcel"/>.</summary>
public static class RecommissionParcelHandler
{
    /// <summary>Loads the parcel, recommissions it, and persists.</summary>
    public static async Task Handle(
        RecommissionParcel command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events.FetchForWriting<LandParcel>(command.LandParcelId.Value, cancellationToken).ConfigureAwait(false);
        var parcel = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(LandParcel), command.LandParcelId.Value);

        parcel.Recommission(command.ChangedBy);

        parcel.ValidateInvariants();
        stream.AppendMany(parcel.UncommittedEvents.ToArray());
        parcel.UncommittedEvents.MarkAsCommitted();
    }
}
