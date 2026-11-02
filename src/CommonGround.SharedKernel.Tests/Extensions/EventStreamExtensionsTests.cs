using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Extensions;
using FluentAssertions;
using JasperFx.Events;

namespace CommonGround.SharedKernel.Tests.Extensions;

public class EventStreamExtensionsTests
{
    public class TheDecideMethod
    {
        [Fact]
        public void GivenADecision_WhenDecided_ThenAppendsWhatWasDecided()
        {
            var stream = new StubEventStream<Gate>(Gate.Create(new GateBuilt()));

            stream.Decide(gate => gate.Open());

            stream.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<GateOpened>();
        }

        [Fact]
        public void GivenAnEarlierDecision_WhenDecidedAgain_ThenSeesTheStateItProduced()
        {
            var stream = new StubEventStream<Gate>(Gate.Create(new GateBuilt()));

            stream.Decide(gate => gate.Open());
            stream.Decide(gate => gate.Close());

            stream.EventsAppended.Should().SatisfyRespectively(
                e => e.Should().BeOfType<GateOpened>(),
                e => e.Should().BeOfType<GateClosed>());
        }

        [Fact]
        public void GivenAnEarlierDecision_WhenTheSameDecisionIsRepeated_ThenItsGuardSeesTheNewState()
        {
            var stream = new StubEventStream<Gate>(Gate.Create(new GateBuilt()));
            stream.Decide(gate => gate.Open());

            var act = () => stream.Decide(gate => gate.Open());

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GivenAnEventAppendedDirectly_WhenDecided_ThenSeesThatEventToo()
        {
            var stream = new StubEventStream<Gate>(Gate.Create(new GateBuilt()));
            stream.AppendOne(new GateOpened());

            stream.Decide(gate => gate.Close());

            stream.EventsAppended.Last().Should().BeOfType<GateClosed>();
        }

        [Fact]
        public void GivenNoAggregate_WhenDecided_ThenThrowsAggregateNotFound()
        {
            var stream = new StubEventStream<Gate>(null!);

            var act = () => stream.Decide(gate => gate.Open());

            act.Should().Throw<AggregateNotFoundException>();
        }
    }
}

public sealed record GateBuilt;

public sealed record GateOpened;

public sealed record GateClosed;

/// <summary>A gate that can only be opened when closed, and closed when open.</summary>
public sealed class Gate
{
    public Guid Id { get; set; }

    public bool IsOpen { get; private set; }

    public static Gate Create(GateBuilt built) => new();

    public GateOpened Open() => IsOpen ? throw new InvalidOperationException("Already open.") : new GateOpened();

    public GateClosed Close() => IsOpen ? new GateClosed() : throw new InvalidOperationException("Already closed.");

    public void Apply(GateOpened opened) => IsOpen = true;

    public void Apply(GateClosed closed) => IsOpen = false;
}
