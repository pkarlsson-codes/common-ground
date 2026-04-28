using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Marks parcel ready for occupancy.</summary>
public sealed record MarkParcelReadyForOccupancy(LandParcelId LandParcelId, UserId ChangedBy);

/// <summary>Handles <see cref="MarkParcelReadyForOccupancy"/>.</summary>
public static class MarkParcelReadyForOccupancyHandler
{
    /// <summary>Loads the parcel, marks ready for occupancy, and persists.</summary>
    public static async Task Handle(
        MarkParcelReadyForOccupancy command,
        ILandParcelRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var parcel = await repository.GetByIdAsync(command.LandParcelId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.LandParcelId.Value);

        parcel.MarkReadyForOccupancy(command.ChangedBy);

        repository.Stage(parcel);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}