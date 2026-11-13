using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.API.Demo;

/// <summary>Errors a demo server returns.</summary>
public static class DemoViolation
{
    /// <summary>The demo already holds <see cref="DemoOptions.MaxSandboxes"/> sandboxes.</summary>
    /// <returns>The error.</returns>
    public static BusinessRuleViolationException Full() => new("demo.full");
}
