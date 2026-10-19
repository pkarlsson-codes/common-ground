using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.Households.Events;
using Wolverine;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>Schedules expiry of a mortgage prequalification when one is recorded.</summary>
public sealed class MortgagePrequalificationRecordedHandler(IMessageBus bus)
{
    /// <summary>Schedules an <see cref="ExpireMortgagePrequalification"/> message at the prequalification's expiry date.</summary>
    public async Task Handle(MortgagePrequalificationRecorded @event, CancellationToken cancellationToken)
    {
        var expiryTimestampUtc = DateTime.SpecifyKind(
            @event.ExpiryDate.ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Utc);

        await bus.ScheduleAsync(
            new ExpireMortgagePrequalification(
                HouseholdId.Create(@event.AggregateId),
                @event.ExpiryDate),
            expiryTimestampUtc).ConfigureAwait(false);
    }
}
