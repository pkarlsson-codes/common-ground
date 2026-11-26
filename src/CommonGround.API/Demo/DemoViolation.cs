using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.API.Demo;

/// <summary>Errors a demo server returns.</summary>
public static class DemoViolation
{
    /// <summary>The demo already holds <see cref="DemoOptions.MaxSandboxes"/> sandboxes.</summary>
    /// <returns>The error.</returns>
    public static BusinessRuleViolationException Full() => new("demo.full");

    /// <summary>Feedback was sent with no message.</summary>
    /// <returns>The error.</returns>
    public static BusinessRuleViolationException FeedbackEmpty() => new("demo.feedback.empty");

    /// <summary>Feedback is longer than <see cref="DemoFeedback.MessageMaxLength"/> characters.</summary>
    /// <returns>The error.</returns>
    public static BusinessRuleViolationException FeedbackTooLong() => new("demo.feedback.too_long");
}
