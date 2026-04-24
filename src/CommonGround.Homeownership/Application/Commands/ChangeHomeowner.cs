using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Changes the homeowner on a ground lease.</summary>
public sealed record ChangeHomeowner(
    GroundLeaseId GroundLeaseId,
    Homeowner NewHomeowner,
    UserId ChangedBy);

/// <summary>Handles <see cref="ChangeHomeowner"/>.</summary>
public static class ChangeHomeownerHandler
{
    /// <summary>Loads the lease, changes the homeowner, and persists.</summary>
    public static async Task Handle(
        ChangeHomeowner command,
        IGroundLeaseRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lease = await repository.GetByIdAsync(command.GroundLeaseId, cancellationToken).ConfigureAwait(false)
            ?? throw new CommonGround.SharedKernel.Domain.Exceptions.AggregateNotFoundException(command.GroundLeaseId.Value);
        lease.ChangeHomeowner(command.NewHomeowner, command.ChangedBy);

        repository.Stage(lease);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
