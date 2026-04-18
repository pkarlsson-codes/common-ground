using CommonGround.PropertyRegistry.Domain.Buildings;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Changes the status of a building.</summary>
/// <param name="BuildingId">The building to update.</param>
/// <param name="NewStatus">The new status to set.</param>
/// <param name="ChangedBy">The staff user making the change.</param>
public sealed record UpdateBuildingStatus(
    BuildingId BuildingId,
    BuildingStatus NewStatus,
    UserId ChangedBy);

/// <summary>Handles <see cref="UpdateBuildingStatus"/>.</summary>
public static class UpdateBuildingStatusHandler
{
    /// <summary>Loads the building, changes status, and persists.</summary>
    public static async Task Handle(
        UpdateBuildingStatus command,
        IBuildingRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var building = await repository.GetByIdAsync(command.BuildingId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.BuildingId.Value);

        building.ChangeStatus(command.NewStatus, command.ChangedBy);

        repository.Stage(building);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}