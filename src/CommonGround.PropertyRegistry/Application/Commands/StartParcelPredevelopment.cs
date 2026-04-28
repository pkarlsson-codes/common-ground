using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Starts parcel predevelopment.</summary>
public sealed record StartParcelPredevelopment(LandParcelId LandParcelId, UserId ChangedBy);

/// <summary>Handles <see cref="StartParcelPredevelopment"/>.</summary>
public static class StartParcelPredevelopmentHandler
{
    /// <summary>Loads the parcel, starts predevelopment, and persists.</summary>
    public static async Task Handle(
        StartParcelPredevelopment command,
        ILandParcelRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var parcel = await repository.GetByIdAsync(command.LandParcelId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.LandParcelId.Value);

        parcel.StartPredevelopment(command.ChangedBy);

        repository.Stage(parcel);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}