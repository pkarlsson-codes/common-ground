using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Withdraws a land parcel for redevelopment.</summary>
public sealed record WithdrawParcelForRedevelopment(LandParcelId LandParcelId, UserId ChangedBy);

/// <summary>Handles <see cref="WithdrawParcelForRedevelopment"/>.</summary>
public static class WithdrawParcelForRedevelopmentHandler
{
    /// <summary>Loads the parcel, withdraws it for redevelopment, and persists.</summary>
    public static async Task Handle(
        WithdrawParcelForRedevelopment command,
        ILandParcelRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var parcel = await repository.GetByIdAsync(command.LandParcelId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.LandParcelId.Value);

        parcel.WithdrawForRedevelopment(command.ChangedBy);

        repository.Stage(parcel);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
