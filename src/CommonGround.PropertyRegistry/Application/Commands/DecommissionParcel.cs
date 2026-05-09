using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Decommissions a parcel.</summary>
public sealed record DecommissionParcel(LandParcelId LandParcelId, UserId ChangedBy);

/// <summary>Handles <see cref="DecommissionParcel"/>.</summary>
public static class DecommissionParcelHandler
{
    /// <summary>Loads the parcel, decommissions it, and persists.</summary>
    public static async Task Handle(
        DecommissionParcel command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var parcel = await session
            .LoadAggregateAsync<LandParcel>(command.LandParcelId, cancellationToken)
            .ConfigureAwait(false);

        parcel.Decommission(command.ChangedBy);

        session.UpdateAggregate(parcel);
    }
}
