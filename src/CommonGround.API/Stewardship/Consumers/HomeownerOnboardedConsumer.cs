using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;
using CommonGround.Stewardship.Domain.StewardshipCases;

namespace CommonGround.API.Stewardship.Consumers;

/// <summary>Handles homeowner onboarding events by creating a stewardship case.</summary>
public sealed class HomeownerOnboardedConsumer(
    IStewardshipCaseRepository repository,
    IUnitOfWork unitOfWork)
{
    /// <summary>Creates a stewardship case for a newly onboarded homeowner.</summary>
    /// <param name="event">The homeowner onboarding event.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(HomeownerOnboarded @event, CancellationToken ct)
    {
        var sc = StewardshipCase.Open(
            @event.AggregateId,
            DateOnly.FromDateTime(@event.OccurredAt.UtcDateTime),
            UserId.Create(@event.OnboardedBy));

        repository.Stage(sc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }
}
