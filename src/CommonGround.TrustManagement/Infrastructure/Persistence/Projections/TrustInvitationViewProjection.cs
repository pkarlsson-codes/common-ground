using CommonGround.TrustManagement.Application.ReadModels;
using CommonGround.TrustManagement.Domain.TrustInvitations.Events;
using Marten.Events.Aggregation;

namespace CommonGround.TrustManagement.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="TrustInvitationView"/> from invitation events.</summary>
internal sealed class TrustInvitationViewProjection : SingleStreamProjection<TrustInvitationView, string>
{
    /// <summary>Starts the view from the issued invitation.</summary>
    public static TrustInvitationView Create(TrustInvitationIssued e) =>
        new()
        {
            Id = e.AggregateId,
            TrustId = e.TrustId,
            Email = e.Email,
            TokenHash = e.TokenHash,
            IssuedBy = e.IssuedBy,
            IssuedAt = e.OccurredAt,
            ExpiresAt = e.ExpiresAt,
        };

    /// <summary>Marks the invitation used.</summary>
    public static void Apply(TrustInvitationView view, TrustInvitationAccepted e)
    {
        view.Status = TrustInvitationView.Accepted;
        view.AcceptedBy = e.UserId;
    }

    /// <summary>Marks the invitation cancelled.</summary>
#pragma warning disable IDE0060 // Marten matches Apply methods by the event parameter's type.
    public static void Apply(TrustInvitationView view, TrustInvitationCancelled e) => view.Status = TrustInvitationView.Cancelled;
#pragma warning restore IDE0060
}
