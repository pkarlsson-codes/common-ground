using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Enters parcel transitioning state.</summary>
public sealed record EnterParcelTransitioning(LandParcelId LandParcelId, UserId ChangedBy);

/// <summary>Handles <see cref="EnterParcelTransitioning"/>.</summary>
public static class EnterParcelTransitioningHandler
{
    /// <summary>Loads the parcel, enters transitioning, and persists.</summary>
    public static async Task Handle(
        EnterParcelTransitioning command,
        ILandParcelRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var parcel = await repository.GetByIdAsync(command.LandParcelId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.LandParcelId.Value);

        parcel.EnterTransitioning(command.ChangedBy);

        repository.Stage(parcel);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}