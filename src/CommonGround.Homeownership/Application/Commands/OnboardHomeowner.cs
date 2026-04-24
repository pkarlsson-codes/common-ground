using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Onboards the first homeowner to a ground lease.</summary>
public sealed record OnboardHomeowner(
    GroundLeaseId GroundLeaseId,
    Homeowner Homeowner,
    UserId OnboardedBy);

/// <summary>Handles <see cref="OnboardHomeowner"/>.</summary>
public static class OnboardHomeownerHandler
{
    /// <summary>Loads the lease, onboards the homeowner, and persists.</summary>
    public static async Task Handle(
        OnboardHomeowner command,
        IGroundLeaseRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lease = await repository.GetByIdAsync(command.GroundLeaseId, cancellationToken).ConfigureAwait(false)
            ?? throw new CommonGround.SharedKernel.Domain.Exceptions.AggregateNotFoundException(command.GroundLeaseId.Value);
        lease.OnboardHomeowner(command.Homeowner, command.OnboardedBy);

        repository.Stage(lease);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
