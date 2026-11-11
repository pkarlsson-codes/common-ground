using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using CommonGround.TrustManagement.Application.Errors;
using CommonGround.TrustManagement.Application.ReadModels;
using CommonGround.TrustManagement.Domain.TrustInvitations;
using JasperFx.Events;
using Marten;

namespace CommonGround.TrustManagement.Application.Commands;

/// <summary>Joins the signed-in user to the trust an invite link is for.</summary>
/// <param name="TokenHash">The hash of the link's token.</param>
/// <param name="UserId">The signed-in user. The caller has checked they belong to no trust.</param>
/// <param name="Now">The current instant.</param>
public sealed record AcceptTrustInvitation(string TokenHash, UserId UserId, DateTimeOffset Now);

/// <summary>Handles <see cref="AcceptTrustInvitation"/>.</summary>
public static class AcceptTrustInvitationHandler
{
    /// <summary>Finds the invitation by its token hash and fetches its stream for writing.</summary>
    public static async Task<IEventStream<TrustInvitation>> LoadAsync(
        AcceptTrustInvitation command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var view = await session.Query<TrustInvitationView>()
            .SingleOrDefaultAsync(x => x.TokenHash == command.TokenHash, cancellationToken)
            .ConfigureAwait(false)
            ?? throw BusinessRuleViolation.TrustInvitation.Accept.NoLongerValid();

        return await session.Events.FetchForWriting<TrustInvitation>(view.Id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Records the acceptance and returns the trust joined.</summary>
    public static TrustId Handle(AcceptTrustInvitation command, IEventStream<TrustInvitation> stream)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(stream);

        var invitation = stream.Aggregate ?? throw BusinessRuleViolation.TrustInvitation.Accept.NoLongerValid();
        stream.Decide(i => i.Accept(command.UserId, command.Now));
        return invitation.TrustId;
    }
}
