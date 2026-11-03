using CommonGround.SharedKernel.Infrastructure.Seeding;
using FluentAssertions;

namespace CommonGround.SharedKernel.Tests.Infrastructure.Seeding;

public class PropagationWaiterTests
{
    public class TestBed
    {
        public ScriptedProbe Probe { get; } = new();

        public PropagationWaiter Sut(TimeSpan? timeout = null) =>
            new(Probe, TimeSpan.FromMilliseconds(1), timeout ?? TimeSpan.FromSeconds(5));
    }

    /// <summary>Reports each scripted result in turn, then repeats the last one.</summary>
    public sealed class ScriptedProbe : IPropagationProbe
    {
        private readonly Queue<string?> _results = new();
        private string? _last;

        public int Checks { get; private set; }

        public ScriptedProbe Reports(params string?[] results)
        {
            foreach (var result in results)
            {
                _results.Enqueue(result);
            }

            return this;
        }

        public Task<string?> FindPendingAsync(CancellationToken cancellationToken)
        {
            Checks++;
            if (_results.Count > 0)
            {
                _last = _results.Dequeue();
            }

            return Task.FromResult(_last);
        }
    }

    public class TheWaitForIdleAsyncMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public async Task GivenPendingThenIdle_WhenWaiting_ThenReturnsOnceIdleTwiceInARow()
        {
            _testBed.Probe.Reports("1 incoming message(s)", "shards behind: everything:All at 4 of 5", null, null);

            await _testBed.Sut().WaitForIdleAsync();

            _testBed.Probe.Checks.Should().Be(4);
        }

        [Fact]
        public async Task GivenIdleOnceThenPendingAgain_WhenWaiting_ThenKeepsWaiting()
        {
            _testBed.Probe.Reports(null, "1 incoming message(s)", null, null);

            await _testBed.Sut().WaitForIdleAsync();

            _testBed.Probe.Checks.Should().Be(4);
        }

        [Fact]
        public async Task GivenStillPending_WhenTimeoutPasses_ThenThrowsNamingWhatIsPending()
        {
            _testBed.Probe.Reports("3 incoming message(s) in Wolverine's inbox");

            var act = () => _testBed.Sut(TimeSpan.FromMilliseconds(20)).WaitForIdleAsync();

            await act.Should().ThrowAsync<TimeoutException>().WithMessage("*3 incoming message(s) in Wolverine's inbox*");
        }
    }

    public class TheWaitForAsyncMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public async Task GivenRecordAppearsLater_WhenWaiting_ThenReturnsIt()
        {
            var lookups = 0;

            var found = await _testBed.Sut().WaitForAsync(
                "the ground lease",
                _ => Task.FromResult(++lookups < 3 ? null : "lease-1"));

            found.Should().Be("lease-1");
        }

        [Fact]
        public async Task GivenRecordNeverAppears_WhenTimeoutPasses_ThenThrowsNamingWhatWasAwaited()
        {
            var act = () => _testBed.Sut(TimeSpan.FromMilliseconds(20)).WaitForAsync(
                "the purchase workflow for application X",
                _ => Task.FromResult<string?>(null));

            await act.Should().ThrowAsync<TimeoutException>().WithMessage("*the purchase workflow for application X*");
        }
    }
}
