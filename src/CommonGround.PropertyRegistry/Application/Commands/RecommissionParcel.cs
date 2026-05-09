using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
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

        var parcel = await session
            .LoadAggregateAsync<LandParcel>(command.LandParcelId, cancellationToken)
            .ConfigureAwait(false);

        parcel.Recommission(command.ChangedBy);

        session.UpdateAggregate(parcel);
    }
}
