namespace CommonGround.API.Demo;

/// <summary>
/// A trust on a demo server, and when it is deleted. Kept outside the domain: sandboxes are the operator's
/// concern, not something a trust does. Stored in the default tenant, one per trust.
/// </summary>
public sealed class DemoSandbox
{
    /// <summary>Gets or sets the trust ID, which is also the document ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the user who started the sandbox.</summary>
    public string OwnerUserId { get; set; } = string.Empty;

    /// <summary>Gets or sets when the sandbox was started.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the instant after which the sandbox is deleted.</summary>
    public DateTimeOffset DeletesAt { get; set; }

    /// <summary>Gets or sets whether the sandbox's trust is ready to use.</summary>
    public DemoSandboxStatus Status { get; set; }

    /// <summary>Starts a sandbox for a trust, deleted <paramref name="sandboxDays"/> days after <paramref name="createdAt"/>.</summary>
    /// <param name="trustId">The trust.</param>
    /// <param name="ownerUserId">The user who started it.</param>
    /// <param name="createdAt">When it was started.</param>
    /// <param name="sandboxDays">How many days it lives.</param>
    /// <param name="status">Its status at the start.</param>
    /// <returns>The sandbox.</returns>
    public static DemoSandbox Start(string trustId, string ownerUserId, DateTimeOffset createdAt, int sandboxDays, DemoSandboxStatus status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trustId);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserId);
        ArgumentOutOfRangeException.ThrowIfLessThan(sandboxDays, 1);

        return new DemoSandbox
        {
            Id = trustId,
            OwnerUserId = ownerUserId,
            CreatedAt = createdAt,
            DeletesAt = createdAt.AddDays(sandboxDays),
            Status = status,
        };
    }
}

/// <summary>Where a sandbox's trust is in being set up.</summary>
public enum DemoSandboxStatus
{
    /// <summary>The trust is being filled with sample records.</summary>
    Seeding,

    /// <summary>The trust can be used.</summary>
    Ready,

    /// <summary>Filling the trust failed. It is deleted on its date like any other sandbox.</summary>
    Failed,
}
