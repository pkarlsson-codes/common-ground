using CommonGround.API.Demo;
using FluentAssertions;

namespace CommonGround.API.Tests.Demo;

public class DemoSandboxTests
{
    public class TheStartMethod
    {
        [Fact]
        public void GivenFourteenSandboxDays_WhenStarted_ThenDeletesFourteenDaysAfterItWasCreated()
        {
            var createdAt = new DateTimeOffset(2026, 11, 13, 22, 30, 0, TimeSpan.Zero);

            var sandbox = DemoSandbox.Start("trust-1", "user-1", createdAt, sandboxDays: 14, DemoSandboxStatus.Ready);

            sandbox.DeletesAt.Should().Be(new DateTimeOffset(2026, 11, 27, 22, 30, 0, TimeSpan.Zero));
            sandbox.Id.Should().Be("trust-1");
        }
    }

    public class TheStartSampleMethod
    {
        [Fact]
        public void GivenTwoSampleSandboxes_WhenStarted_ThenEachIsSeedingWithItsOwnSeedRun()
        {
            var createdAt = new DateTimeOffset(2026, 11, 13, 22, 30, 0, TimeSpan.Zero);

            var first = DemoSandbox.StartSample("trust-1", "user-1", createdAt, sandboxDays: 14);
            var second = DemoSandbox.StartSample("trust-2", "user-2", createdAt, sandboxDays: 14);

            first.Status.Should().Be(DemoSandboxStatus.Seeding);
            first.RunKey.Should().StartWith("sandbox-");
            first.RunKey.Should().NotBe(second.RunKey);
        }
    }
}
