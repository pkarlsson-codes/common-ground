using CommonGround.API.Demo;
using FluentAssertions;

namespace CommonGround.API.Tests.Demo;

public class DemoOptionsValidatorTests
{
    public class TestBed
    {
        public DemoOptionsValidator Sut { get; } = new();

        public DemoOptions Enabled(int sandboxDays = 14, int maxSandboxes = 100) =>
            new() { Enabled = true, SandboxDays = sandboxDays, MaxSandboxes = maxSandboxes };
    }

    public class TheValidateMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenDemoModeOff_WhenValidated_ThenSucceedsWithZeroSandboxDays()
        {
            var result = _testBed.Sut.Validate(null, new DemoOptions { Enabled = false, SandboxDays = 0 });

            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public void GivenDemoModeOnWithDefaults_WhenValidated_ThenSucceeds()
        {
            var result = _testBed.Sut.Validate(null, _testBed.Enabled());

            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public void GivenZeroSandboxDaysAndZeroMaxSandboxes_WhenValidated_ThenFailsForBoth()
        {
            var result = _testBed.Sut.Validate(null, _testBed.Enabled(sandboxDays: 0, maxSandboxes: 0));

            result.Failures.Should().Equal("Demo:SandboxDays must be at least 1.", "Demo:MaxSandboxes must be at least 1.");
        }
    }
}
