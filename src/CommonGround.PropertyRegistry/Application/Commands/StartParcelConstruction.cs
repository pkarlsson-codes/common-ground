using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Starts parcel construction.</summary>
public sealed record StartParcelConstruction(LandParcelId LandParcelId, UserId ChangedBy);

/// <summary>Handles <see cref="StartParcelConstruction"/>.</summary>
public static class StartParcelConstructionHandler
{
    /// <summary>Loads the parcel, starts construction, and persists.</summary>
    public static async Task Handle(
        StartParcelConstruction command,
        ILandParcelRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var parcel = await repository.GetByIdAsync(command.LandParcelId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.LandParcelId.Value);

        parcel.StartConstruction(command.ChangedBy);

        repository.Stage(parcel);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}