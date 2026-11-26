using Microsoft.Extensions.Options;

namespace CommonGround.API.Demo;

/// <summary>
/// Demo mode, read from the <c>Demo</c> config section. On a demo server every trust is a sandbox that is
/// deleted after <see cref="SandboxDays"/>. A server for real work leaves <see cref="Enabled"/> off.
/// </summary>
public sealed class DemoOptions
{
    /// <summary>The config section name.</summary>
    public const string SectionName = "Demo";

    /// <summary>Gets or sets a value indicating whether this server is a demo server.</summary>
    public bool Enabled { get; set; }

    /// <summary>Gets or sets how many days a sandbox trust lives before it is deleted.</summary>
    public int SandboxDays { get; set; } = 14;

    /// <summary>Gets or sets how many sandboxes may exist at once.</summary>
    public int MaxSandboxes { get; set; } = 100;
}

/// <summary>Refuses to start a demo server whose <see cref="DemoOptions"/> can't work.</summary>
public sealed class DemoOptionsValidator : IValidateOptions<DemoOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, DemoOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        var failures = new List<string>();

        if (options.SandboxDays < 1)
        {
            failures.Add("Demo:SandboxDays must be at least 1.");
        }

        if (options.MaxSandboxes < 1)
        {
            failures.Add("Demo:MaxSandboxes must be at least 1.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
