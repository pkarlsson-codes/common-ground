using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.SharedKernel.Infrastructure.Services;
using FluentAssertions;

namespace CommonGround.SharedKernel.Tests.Infrastructure.Seeding;

// SeedClock sets the app-wide TimeProviderContext, so these tests must not run alongside others.
[CollectionDefinition(nameof(SeedClockTests), DisableParallelization = true)]
public class SeedClockTests
{
    public class TestBed
    {
        public IanaTimeZone Denver { get; } = IanaTimeZone.Create("America/Denver");
    }

    [Collection(nameof(SeedClockTests))]
    public class TheRunAsyncMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public async Task GivenClockMovedToDate_WhenSeedReadsTime_ThenSeesSeededTimeOfDayInTrustZone()
        {
            DateTimeOffset seen = default;

            await SeedClock.RunAsync(_testBed.Denver, clock =>
            {
                clock.MoveTo(new DateOnly(2026, 7, 1));
                seen = TimeProviderContext.Current.UtcNow;
                return Task.CompletedTask;
            });

            seen.Should().Be(new DateTimeOffset(2026, 7, 1, 16, 0, 0, TimeSpan.Zero));
        }

        [Fact]
        public async Task GivenSeedCompletes_WhenRun_ThenRestoresOriginalClock()
        {
            var original = new ControllableTimeProvider(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero));
            TimeProviderContext.SetCurrent(original);
            try
            {
                await SeedClock.RunAsync(_testBed.Denver, clock =>
                {
                    clock.MoveTo(new DateOnly(2026, 7, 1));
                    return Task.CompletedTask;
                });

                TimeProviderContext.Current.Should().BeSameAs(original);
            }
            finally
            {
                TimeProviderContext.Reset();
            }
        }

        [Fact]
        public async Task GivenSeedThrows_WhenRun_ThenRestoresOriginalClock()
        {
            var original = new ControllableTimeProvider(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero));
            TimeProviderContext.SetCurrent(original);
            try
            {
                var act = () => SeedClock.RunAsync(_testBed.Denver, clock =>
                {
                    clock.MoveTo(new DateOnly(2026, 7, 1));
                    throw new InvalidOperationException("seed failed");
                });

                await act.Should().ThrowAsync<InvalidOperationException>();
                TimeProviderContext.Current.Should().BeSameAs(original);
            }
            finally
            {
                TimeProviderContext.Reset();
            }
        }

        [Fact]
        public async Task GivenClockNotMoved_WhenSeedReadsTime_ThenSeesTimeRunWasCalledAt()
        {
            var startedAt = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);
            TimeProviderContext.SetCurrent(new ControllableTimeProvider(startedAt));
            try
            {
                DateTimeOffset seen = default;

                await SeedClock.RunAsync(_testBed.Denver, _ =>
                {
                    seen = TimeProviderContext.Current.UtcNow;
                    return Task.CompletedTask;
                });

                seen.Should().Be(startedAt);
            }
            finally
            {
                TimeProviderContext.Reset();
            }
        }
    }
}
