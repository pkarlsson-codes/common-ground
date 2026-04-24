using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;
using CommonGround.Stewardship.Domain.StewardshipCases;

namespace CommonGround.API.Stewardship.Consumers;

/// <summary>Handles ground lease termination events by closing active stewardship cases.</summary>
public sealed class GroundLeaseTerminatedConsumer(
    IStewardshipCaseRepository repository,
    IUnitOfWork unitOfWork)
{
    /// <summary>Closes the active stewardship case for the terminated ground lease.</summary>
    /// <param name="event">The ground lease termination event.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(GroundLeaseTerminated @event, CancellationToken ct)
    {
        var sc = await repository.GetActiveByGroundLeaseIdAsync(@event.AggregateId, ct)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                $"No active StewardshipCase found for ground lease {@event.AggregateId}.");

        sc.Close(@event.Reason, @event.TerminatedDate, UserId.Create(@event.TerminatedBy));
        repository.Stage(sc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }
}
