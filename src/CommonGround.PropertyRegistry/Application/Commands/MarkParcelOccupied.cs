using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Marks parcel occupied.</summary>
public sealed record MarkParcelOccupied(LandParcelId LandParcelId, UserId ChangedBy);

/// <summary>Handles <see cref="MarkParcelOccupied"/>.</summary>
public static class MarkParcelOccupiedHandler
{
    /// <summary>Loads the parcel, marks occupied, and persists.</summary>
    public static async Task Handle(
        MarkParcelOccupied command,
        ILandParcelRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var parcel = await repository.GetByIdAsync(command.LandParcelId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.LandParcelId.Value);

        parcel.MarkOccupied(command.ChangedBy);

        repository.Stage(parcel);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}