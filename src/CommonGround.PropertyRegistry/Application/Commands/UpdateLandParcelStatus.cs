using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Changes the status of a land parcel.</summary>
/// <param name="LandParcelId">The parcel to update.</param>
/// <param name="NewStatus">The new status to set.</param>
/// <param name="ChangedBy">The staff user making the change.</param>
public sealed record UpdateLandParcelStatus(
    LandParcelId LandParcelId,
    ParcelStatus NewStatus,
    UserId ChangedBy);

/// <summary>Handles <see cref="UpdateLandParcelStatus"/>.</summary>
public static class UpdateLandParcelStatusHandler
{
    /// <summary>Loads the parcel, changes status, and persists.</summary>
    public static async Task Handle(
        UpdateLandParcelStatus command,
        ILandParcelRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var parcel = await repository.GetByIdAsync(command.LandParcelId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.LandParcelId.Value);

        switch (command.NewStatus)
        {
            case var _ when command.NewStatus == ParcelStatus.Predevelopment:
                parcel.StartPredevelopment(command.ChangedBy);
                break;
            case var _ when command.NewStatus == ParcelStatus.UnderConstruction:
                parcel.StartConstruction(command.ChangedBy);
                break;
            case var _ when command.NewStatus == ParcelStatus.Available:
                parcel.MarkReadyForOccupancy(command.ChangedBy);
                break;
            case var _ when command.NewStatus == ParcelStatus.Occupied:
                parcel.MarkOccupied(command.ChangedBy);
                break;
            case var _ when command.NewStatus == ParcelStatus.Resale:
                parcel.EnterTransitioning(command.ChangedBy);
                break;
            case var _ when command.NewStatus == ParcelStatus.Registered:
                if (parcel.Status == ParcelStatus.Decommissioned)
                    parcel.Recommission(command.ChangedBy);
                else
                    parcel.ReturnToAvailable(command.ChangedBy);

                break;
            case var _ when command.NewStatus == ParcelStatus.Decommissioned:
                parcel.Decommission(command.ChangedBy);
                break;
            default:
                throw new BusinessRuleViolationException($"Unsupported parcel status '{command.NewStatus.DisplayName}'.");
        }

        repository.Stage(parcel);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}