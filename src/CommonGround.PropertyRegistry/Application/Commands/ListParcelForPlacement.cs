using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Lists a land parcel for placement.</summary>
public sealed record ListParcelForPlacement(LandParcelId LandParcelId, UserId ChangedBy);

/// <summary>Handles <see cref="ListParcelForPlacement"/>.</summary>
public static class ListParcelForPlacementHandler
{
    /// <summary>Loads the parcel, lists it for placement, and persists.</summary>
    public static async Task Handle(
        ListParcelForPlacement command,
        ILandParcelRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var parcel = await repository.GetByIdAsync(command.LandParcelId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.LandParcelId.Value);

        parcel.ListForPlacement(command.ChangedBy);

        repository.Stage(parcel);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
