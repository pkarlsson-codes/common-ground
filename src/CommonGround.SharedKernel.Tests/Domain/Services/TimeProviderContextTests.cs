using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Infrastructure.Services;
using FluentAssertions;

namespace CommonGround.SharedKernel.Tests.Domain.Services;

public class TimeProviderContextTests
{
    public class TestBed
    {
        public ControllableTimeProvider Outer { get; } = new(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero));

        public ControllableTimeProvider Inner { get; } = new(new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero));
    }

    public class TheUseMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenScopedClock_WhenReadInScope_ThenIsScopedClock()
        {
            using (TimeProviderContext.Use(_testBed.Outer))
            {
                TimeProviderContext.Current.Should().BeSameAs(_testBed.Outer);
                TimeProviderContext.IsScoped.Should().BeTrue();
            }
        }

        [Fact]
        public void GivenScopeDisposed_WhenRead_ThenNotScoped()
        {
            var scope = TimeProviderContext.Use(_testBed.Outer);

            scope.Dispose();

            TimeProviderContext.Current.Should().NotBeSameAs(_testBed.Outer);
            TimeProviderContext.IsScoped.Should().BeFalse();
        }

        [Fact]
        public void GivenNestedScopes_WhenInnerDisposed_ThenOuterClockIsBack()
        {
            using (TimeProviderContext.Use(_testBed.Outer))
            {
                var inner = TimeProviderContext.Use(_testBed.Inner);
                TimeProviderContext.Current.Should().BeSameAs(_testBed.Inner);

                inner.Dispose();
                inner.Dispose();

                TimeProviderContext.Current.Should().BeSameAs(_testBed.Outer);
            }
        }

        [Fact]
        public async Task GivenScopeInOneFlow_WhenOtherFlowReads_ThenOtherFlowDoesNotSeeIt()
        {
            using var scopeSet = new SemaphoreSlim(0);
            using var otherRead = new SemaphoreSlim(0);
            var otherSawScoped = true;

            var scoped = Task.Run(async () =>
            {
                using (TimeProviderContext.Use(_testBed.Outer))
                {
                    scopeSet.Release();
                    await otherRead.WaitAsync();
                }
            });
            var other = Task.Run(async () =>
            {
                await scopeSet.WaitAsync();
                otherSawScoped = TimeProviderContext.IsScoped;
                otherRead.Release();
            });

            await Task.WhenAll(scoped, other);

            otherSawScoped.Should().BeFalse();
        }

        [Fact]
        public async Task GivenScopedClock_WhenAwaitedWorkReads_ThenSeesScopedClock()
        {
            ITimeProvider? seen = null;

            using (TimeProviderContext.Use(_testBed.Outer))
            {
                await Task.Run(() => seen = TimeProviderContext.Current);
            }

            seen.Should().BeSameAs(_testBed.Outer);
        }
    }
}
