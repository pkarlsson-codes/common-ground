using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.LeaseFeeNoticePolicies;
using CommonGround.SharedKernel.Domain;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Sets the trust's lease fee notice policy.</summary>
/// <param name="Schedule">When each notice is due.</param>
/// <param name="LetterSignOff">The lines printed at the end of every letter; empty for none.</param>
/// <param name="SetBy">The staff user setting it.</param>
public sealed record SetLeaseFeeNoticePolicy(LeaseFeeNoticeSchedule Schedule, string? LetterSignOff, UserId SetBy);

/// <summary>Handles <see cref="SetLeaseFeeNoticePolicy"/>.</summary>
public static class SetLeaseFeeNoticePolicyHandler
{
    /// <summary>Starts the trust's policy stream the first time, and appends to it after.</summary>
    public static async Task Handle(
        SetLeaseFeeNoticePolicy command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);

        var id = LeaseFeeNoticePolicyId.ForTrust(session.TenantId);
        var stream = await session.Events
            .FetchForWriting<LeaseFeeNoticePolicy>(id.Value, cancellationToken)
            .ConfigureAwait(false);

        stream.AppendOne(LeaseFeeNoticePolicy.Set(id, command.Schedule, command.LetterSignOff, command.SetBy));
    }
}
