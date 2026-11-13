using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using Marten;
using JasperFx.Events;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records a mortgage the trust permitted outside CommonGround on a lease.</summary>
/// <param name="GroundLeaseId">The lease.</param>
/// <param name="MortgageId">The new mortgage's ID, chosen by the caller.</param>
/// <param name="Lender">The lender or servicer.</param>
/// <param name="NoticeAddress">Where the trust sends copies of notices, if known.</param>
/// <param name="OriginalPrincipal">The loan amount when it closed.</param>
/// <param name="PermittedOn">The day the trust permitted it.</param>
/// <param name="RecordedBy">The staff user recording it.</param>
public sealed record RecordPermittedMortgage(
    GroundLeaseId GroundLeaseId,
    PermittedMortgageId MortgageId,
    string Lender,
    string? NoticeAddress,
    Money OriginalPrincipal,
    DateOnly PermittedOn,
    UserId RecordedBy);

/// <summary>What recording or paying off a permitted mortgage decides from.</summary>
/// <param name="Lease">The lease stream, fetched for writing.</param>
/// <param name="Today">The trust's today.</param>
public sealed record PermittedMortgageDecision(IEventStream<GroundLease> Lease, DateOnly Today)
{
    /// <summary>Fetches the lease for writing and the trust's today.</summary>
    public static async Task<PermittedMortgageDecision> LoadAsync(
        GroundLeaseId groundLeaseId,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var lease = await session.Events
            .FetchForWriting<GroundLease>(groundLeaseId.Value, cancellationToken)
            .ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new PermittedMortgageDecision(lease, today);
    }
}

/// <summary>Handles <see cref="RecordPermittedMortgage"/>.</summary>
public static class RecordPermittedMortgageHandler
{
    /// <summary>Fetches the lease for writing and the trust's today.</summary>
    public static Task<PermittedMortgageDecision> LoadAsync(
        RecordPermittedMortgage command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        return PermittedMortgageDecision.LoadAsync(command.GroundLeaseId, session, trustClock, cancellationToken);
    }

    /// <summary>Records the mortgage on the lease.</summary>
    public static void Handle(RecordPermittedMortgage command, PermittedMortgageDecision decision)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(decision);

        decision.Lease.Decide(l => l.RecordPermittedMortgage(
            command.MortgageId,
            command.Lender,
            command.NoticeAddress,
            command.OriginalPrincipal,
            command.PermittedOn,
            decision.Today,
            command.RecordedBy));
    }
}
