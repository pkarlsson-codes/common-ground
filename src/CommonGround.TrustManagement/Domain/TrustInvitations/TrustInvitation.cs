using CommonGround.SharedKernel.Domain;
using CommonGround.TrustManagement.Application.Errors;
using CommonGround.TrustManagement.Domain.TrustInvitations.Events;

namespace CommonGround.TrustManagement.Domain.TrustInvitations;

/// <summary>
/// A one-time link a trust member makes so a colleague can join the trust. The link carries a secret token;
/// the invitation keeps only its hash.
/// </summary>
public sealed class TrustInvitation : AggregateRoot<TrustInvitationId>
{
    /// <summary>How long an invitation can be used after it is issued.</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);

#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private TrustInvitation() { }
#pragma warning restore CS8618

    /// <summary>Gets the trust the invitation joins.</summary>
    public TrustId TrustId { get; private set; }

    /// <summary>Gets the address the invitation was made for. A record for staff, not checked on joining.</summary>
    public EmailAddress Email { get; private set; }

    /// <summary>Gets when the invitation stops working.</summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>Gets whether someone has joined with the invitation.</summary>
    public bool IsAccepted { get; private set; }

    /// <summary>Gets whether a member cancelled the invitation.</summary>
    public bool IsCancelled { get; private set; }

    /// <summary>Decides to issue an invitation to join a trust.</summary>
    /// <param name="id">The new invitation's id.</param>
    /// <param name="trustId">The trust to join.</param>
    /// <param name="email">The address the invitation is for.</param>
    /// <param name="tokenHash">The hash of the link's secret token.</param>
    /// <param name="issuedBy">The member making the invitation.</param>
    /// <param name="issuedAt">The current instant.</param>
    /// <returns>The event that starts the invitation's stream.</returns>
    public static TrustInvitationIssued Issue(
        TrustInvitationId id, TrustId trustId, EmailAddress email, string tokenHash, UserId issuedBy, DateTimeOffset issuedAt)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        return new TrustInvitationIssued(id.Value, trustId.Value, email.Value, tokenHash, issuedBy.Value, issuedAt.Add(Lifetime));
    }

    /// <summary>Decides to let a user join the trust with this invitation.</summary>
    /// <param name="userId">The signed-in user joining. They must not belong to a trust; the caller checks.</param>
    /// <param name="now">The current instant.</param>
    /// <returns>The acceptance.</returns>
    public TrustInvitationAccepted Accept(UserId userId, DateTimeOffset now)
    {
        if (IsAccepted || IsCancelled || now >= ExpiresAt)
        {
            throw BusinessRuleViolation.TrustInvitation.Accept.NoLongerValid();
        }

        return new TrustInvitationAccepted(Id.Value, TrustId.Value, userId.Value);
    }

    /// <summary>Decides to cancel the invitation so its link stops working.</summary>
    /// <param name="trustId">The canceller's trust. An invitation of another trust is treated as not found.</param>
    /// <param name="cancelledBy">The member cancelling.</param>
    /// <returns>The cancellation, or <see langword="null"/> when it is already cancelled.</returns>
    public TrustInvitationCancelled? Cancel(TrustId trustId, UserId cancelledBy)
    {
        if (trustId != TrustId)
        {
            throw BusinessRuleViolation.TrustInvitation.NotFound();
        }

        if (IsAccepted)
        {
            throw BusinessRuleViolation.TrustInvitation.Cancel.AlreadyAccepted();
        }

        return IsCancelled ? null : new TrustInvitationCancelled(Id.Value, TrustId.Value, cancelledBy.Value);
    }

    /// <summary>Starts an invitation from its first event.</summary>
    public static TrustInvitation Create(TrustInvitationIssued e)
    {
        ArgumentNullException.ThrowIfNull(e);

        return new TrustInvitation
        {
            Id = TrustInvitationId.Create(e.AggregateId),
            TrustId = TrustId.Create(e.TrustId),
            Email = EmailAddress.Create(e.Email),
            ExpiresAt = e.ExpiresAt,
        };
    }

    /// <summary>Applies <see cref="TrustInvitationAccepted"/>.</summary>
    public void Apply(TrustInvitationAccepted e)
    {
        ArgumentNullException.ThrowIfNull(e);
        IsAccepted = true;
    }

    /// <summary>Applies <see cref="TrustInvitationCancelled"/>.</summary>
    public void Apply(TrustInvitationCancelled e)
    {
        ArgumentNullException.ThrowIfNull(e);
        IsCancelled = true;
    }
}
