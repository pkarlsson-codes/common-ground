using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Domain.Households.Events;
using CommonGround.HomeBuyers.Domain.Households;
using Wolverine;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>Schedules expiry of a mortgage prequalification when one is recorded.</summary>
public static class MortgagePrequalificationRecordedHandler
{
    /// <summary>Schedules an <see cref="ExpireMortgagePrequalification"/> message at the prequalification's expiry date.</summary>
    public static ScheduledMessage<ExpireMortgagePrequalification> Handle(MortgagePrequalificationRecorded @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var expiryTimestampUtc = new DateTimeOffset(@event.ExpiryDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        return new ExpireMortgagePrequalification(HouseholdId.Create(@event.AggregateId), @event.ExpiryDate)
            .ScheduledAt(expiryTimestampUtc);
    }
}
