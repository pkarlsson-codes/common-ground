using System.Globalization;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Infrastructure.Messaging;
using CommonGround.SharedKernel.Infrastructure.Services;
using FluentAssertions;
using Wolverine;

namespace CommonGround.SharedKernel.Tests.Infrastructure.Messaging;

public class EventClockMiddlewareTests
{
    public class TestBed
    {
        public DateTimeOffset EventTime { get; } = new(2025, 3, 4, 17, 0, 0, TimeSpan.Zero);

        public DateTimeOffset LaterTime { get; } = new(2025, 6, 1, 6, 0, 0, TimeSpan.Zero);

        public Envelope EventEnvelope() => new(new SomethingHappened { OccurredAt = EventTime });

        public Envelope CommandEnvelope(DateTimeOffset? clockHeader = null)
        {
            var envelope = new Envelope(new DoSomething());
            if (clockHeader is { } at)
            {
                envelope.Headers[EventClockMiddleware.ClockHeader] = at.ToString("O", CultureInfo.InvariantCulture);
            }

            return envelope;
        }

        /// <summary>Runs Before, reads the clock as a handler would, then Finally.</summary>
        public static (DateTimeOffset Seen, bool ScopedAfter) Handle(Envelope envelope)
        {
            var scope = EventClockMiddleware.Before(envelope);
            var seen = TimeProviderContext.Current.UtcNow;
            EventClockMiddleware.Finally(scope);
            return (seen, TimeProviderContext.IsScoped);
        }

        public static string? Header(Envelope envelope) =>
            envelope.TryGetHeader(EventClockMiddleware.ClockHeader, out var value) ? value : null;
    }

    public class TheBeforeMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenEvent_WhenHandled_ThenHandlerSeesEventTimeAndClockIsRestored()
        {
            var envelope = _testBed.EventEnvelope();

            var (seen, scopedAfter) = TestBed.Handle(envelope);

            seen.Should().Be(_testBed.EventTime);
            scopedAfter.Should().BeFalse();
        }

        [Fact]
        public void GivenEvent_WhenHandled_ThenStampsEventTimeForCascadedMessages()
        {
            var envelope = _testBed.EventEnvelope();

            TestBed.Handle(envelope);

            DateTimeOffset.Parse(TestBed.Header(envelope)!, CultureInfo.InvariantCulture).Should().Be(_testBed.EventTime);
        }

        [Fact]
        public void GivenCommandWithClockHeader_WhenHandled_ThenHandlerSeesHeaderTime()
        {
            var (seen, _) = TestBed.Handle(_testBed.CommandEnvelope(_testBed.EventTime));

            seen.Should().Be(_testBed.EventTime);
        }

        [Fact]
        public void GivenScheduledCommandDueAfterItsCause_WhenHandled_ThenHandlerSeesDueTime()
        {
            var envelope = _testBed.CommandEnvelope(_testBed.EventTime);
            envelope.ScheduledTime = _testBed.LaterTime;

            var (seen, _) = TestBed.Handle(envelope);

            seen.Should().Be(_testBed.LaterTime);
        }

        [Fact]
        public void GivenEventWithOlderClockHeader_WhenHandled_ThenEventTimeWins()
        {
            var envelope = _testBed.EventEnvelope();
            envelope.Headers[EventClockMiddleware.ClockHeader] = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero).ToString("O", CultureInfo.InvariantCulture);

            var (seen, _) = TestBed.Handle(envelope);

            seen.Should().Be(_testBed.EventTime);
        }

        [Fact]
        public void GivenCommandWithoutHeader_WhenHandledUnscoped_ThenClockAndHeaderUntouched()
        {
            var envelope = _testBed.CommandEnvelope();

            var (_, scopedAfter) = TestBed.Handle(envelope);

            scopedAfter.Should().BeFalse();
            TestBed.Header(envelope).Should().BeNull();
        }

        [Fact]
        public void GivenCommandInvokedUnderScopedClock_WhenHandled_ThenStampsScopedTime()
        {
            var envelope = _testBed.CommandEnvelope();

            using (TimeProviderContext.Use(new ControllableTimeProvider(_testBed.LaterTime)))
            {
                var (seen, _) = TestBed.Handle(envelope);
                seen.Should().Be(_testBed.LaterTime);
            }

            DateTimeOffset.Parse(TestBed.Header(envelope)!, CultureInfo.InvariantCulture).Should().Be(_testBed.LaterTime);
        }
    }

    private sealed record SomethingHappened() : DomainEvent("aggregate-1");

    private sealed record DoSomething;
}
