using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Returns parcel to available.</summary>
public sealed record ReturnParcelToAvailable(LandParcelId LandParcelId, UserId ChangedBy);

/// <summary>Handles <see cref="ReturnParcelToAvailable"/>.</summary>
public static class ReturnParcelToAvailableHandler
{
    /// <summary>Loads the parcel, returns to available, and persists.</summary>
    public static async Task Handle(
        ReturnParcelToAvailable command,
        ILandParcelRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var parcel = await repository.GetByIdAsync(command.LandParcelId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.LandParcelId.Value);

        parcel.ReturnToAvailable(command.ChangedBy);

        repository.Stage(parcel);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}