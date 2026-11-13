using CommonGround.API.Demo;
using FluentAssertions;

namespace CommonGround.API.Tests.Demo;

public class DemoOptionsValidatorTests
{
    public class TestBed
    {
        public DemoOptionsValidator Sut { get; } = new();

        public DemoOptions Enabled(string? feedbackUrl = "mailto:demo@example.org", int sandboxDays = 14, int maxSandboxes = 100) =>
            new() { Enabled = true, FeedbackUrl = feedbackUrl, SandboxDays = sandboxDays, MaxSandboxes = maxSandboxes };
    }

    public class TheValidateMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenDemoModeOff_WhenValidated_ThenSucceedsWithoutAFeedbackUrl()
        {
            var result = _testBed.Sut.Validate(null, new DemoOptions { Enabled = false, SandboxDays = 0 });

            result.Succeeded.Should().BeTrue();
        }

        [Theory]
        [InlineData("mailto:demo@example.org")]
        [InlineData("https://forms.example.org/commonground")]
        public void GivenAMailtoOrWebFeedbackUrl_WhenValidated_ThenSucceeds(string feedbackUrl)
        {
            var result = _testBed.Sut.Validate(null, _testBed.Enabled(feedbackUrl));

            result.Succeeded.Should().BeTrue();
        }

        [Theory]
        [InlineData(null, "Demo:FeedbackUrl is required when demo mode is on.")]
        [InlineData(" ", "Demo:FeedbackUrl is required when demo mode is on.")]
        [InlineData("demo@example.org", "Demo:FeedbackUrl must be a mailto:, https: or http: link.")]
        [InlineData("ftp://example.org/feedback", "Demo:FeedbackUrl must be a mailto:, https: or http: link.")]
        public void GivenAMissingOrUnusableFeedbackUrl_WhenValidated_ThenFails(string? feedbackUrl, string expected)
        {
            var result = _testBed.Sut.Validate(null, _testBed.Enabled(feedbackUrl));

            result.Failures.Should().Equal(expected);
        }

        [Fact]
        public void GivenZeroSandboxDaysAndZeroMaxSandboxes_WhenValidated_ThenFailsForBoth()
        {
            var result = _testBed.Sut.Validate(null, _testBed.Enabled(sandboxDays: 0, maxSandboxes: 0));

            result.Failures.Should().Equal("Demo:SandboxDays must be at least 1.", "Demo:MaxSandboxes must be at least 1.");
        }
    }
}
