namespace CommonGround.API.Demo;

/// <summary>
/// What a visitor wrote with "Send feedback" on a demo server. Stored in the default tenant, outside the sandbox
/// trust, so it outlives the trust's deletion. The operator reads it with <c>deploy/feedback.sh</c>.
/// </summary>
public sealed class DemoFeedback
{
    /// <summary>The most characters a message holds.</summary>
    public const int MessageMaxLength = 4000;

    /// <summary>The most characters the page address holds; a longer one is cut.</summary>
    public const int PageMaxLength = 500;

    /// <summary>Gets or sets the document ID.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the sandbox trust the visitor was in.</summary>
    public string TrustId { get; set; } = string.Empty;

    /// <summary>Gets or sets the visitor.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the address the operator may reply to, or <see langword="null"/> when the visitor didn't allow a reply.</summary>
    public string? ReplyTo { get; set; }

    /// <summary>Gets or sets the page the visitor was on, such as <c>/resales/resale-01…</c>.</summary>
    public string? Page { get; set; }

    /// <summary>Gets or sets what the visitor wrote.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets when it was sent.</summary>
    public DateTimeOffset SentAt { get; set; }

    /// <summary>Records feedback, refusing an empty or too long message.</summary>
    /// <param name="trustId">The sandbox trust.</param>
    /// <param name="userId">The visitor.</param>
    /// <param name="replyTo">The address the operator may reply to, if any.</param>
    /// <param name="page">The page the visitor was on, if known.</param>
    /// <param name="message">What the visitor wrote.</param>
    /// <param name="sentAt">When it was sent.</param>
    /// <returns>The feedback.</returns>
    public static DemoFeedback Send(string trustId, string userId, string? replyTo, string? page, string? message, DateTimeOffset sentAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trustId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var text = message?.Trim() ?? string.Empty;
        if (text.Length == 0)
        {
            throw DemoViolation.FeedbackEmpty();
        }

        if (text.Length > MessageMaxLength)
        {
            throw DemoViolation.FeedbackTooLong();
        }

        var where = string.IsNullOrWhiteSpace(page) ? null : page.Trim();
        if (where is { Length: > PageMaxLength })
        {
            where = where[..PageMaxLength];
        }

        return new DemoFeedback
        {
            Id = Guid.CreateVersion7(sentAt),
            TrustId = trustId,
            UserId = userId,
            ReplyTo = string.IsNullOrWhiteSpace(replyTo) ? null : replyTo,
            Page = where,
            Message = text,
            SentAt = sentAt,
        };
    }
}
