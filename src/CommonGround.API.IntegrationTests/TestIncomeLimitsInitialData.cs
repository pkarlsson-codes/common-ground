using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.IncomeLimitSchedules;
using CommonGround.SharedKernel.Domain;
using Marten;
using Marten.Schema;

namespace CommonGround.API.IntegrationTests;

/// <summary>
/// Enters an income limit schedule for the test trust, effective from <see cref="EffectiveFrom"/>, so a home
/// with a 50%, 80% or 120% limit can start a selection. Tests that enter their own schedules use dates after
/// 2200, so this one stays in effect on any real today. The 1-person 80% limit is above the 65,000 most tests
/// give a household.
/// </summary>
internal sealed class TestIncomeLimitsInitialData : IInitialData
{
    /// <summary>The date the schedule applies from.</summary>
    internal static readonly DateOnly EffectiveFrom = new(2000, 1, 1);

    /// <summary>The 80% limit for 1 to 8 people.</summary>
    internal static readonly decimal[] EightyPercentLimits = [70_000, 80_000, 90_000, 100_000, 108_000, 116_000, 124_000, 132_000];

    private static readonly decimal[] FiftyPercentLimits = [44_000, 50_000, 56_000, 62_000, 67_000, 72_000, 77_000, 82_000];
    private static readonly decimal[] OneHundredTwentyPercentLimits = [105_000, 120_000, 135_000, 150_000, 162_000, 174_000, 186_000, 198_000];

    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        await using var session = store.LightweightSession(ApiFactory.TestTrustId);
        if (await session.Query<IncomeLimitScheduleView>().AnyAsync(v => v.EffectiveFrom == EffectiveFrom, cancellation))
        {
            return;
        }

        var id = IncomeLimitScheduleId.Generate();
        var entered = IncomeLimitSchedule.Enter(
            id,
            EffectiveFrom,
            [Level(50, FiftyPercentLimits), Level(80, EightyPercentLimits), Level(120, OneHundredTwentyPercentLimits)],
            scheduleOnSameDate: null,
            UserId.Generate());
        session.Events.StartStream<IncomeLimitSchedule>(id.Value, entered);
        await session.SaveChangesAsync(cancellation);
    }

    private static IncomeLimitLevel Level(int percentOfAmi, decimal[] limits) =>
        IncomeLimitLevel.Create(percentOfAmi, [.. limits.Select(Money.Create)]);
}
