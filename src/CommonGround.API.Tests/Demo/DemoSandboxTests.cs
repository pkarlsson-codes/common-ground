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
}
