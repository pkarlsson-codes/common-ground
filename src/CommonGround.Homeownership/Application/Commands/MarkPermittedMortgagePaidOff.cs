using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Marks a permitted mortgage on a lease paid off.</summary>
/// <param name="GroundLeaseId">The lease.</param>
/// <param name="MortgageId">The mortgage.</param>
/// <param name="PaidOffOn">The day it was paid off.</param>
/// <param name="RecordedBy">The staff user recording it.</param>
public sealed record MarkPermittedMortgagePaidOff(
    GroundLeaseId GroundLeaseId,
    PermittedMortgageId MortgageId,
    DateOnly PaidOffOn,
    UserId RecordedBy);

/// <summary>Handles <see cref="MarkPermittedMortgagePaidOff"/>.</summary>
public static class MarkPermittedMortgagePaidOffHandler
{
    /// <summary>Fetches the lease for writing and the trust's today.</summary>
    public static Task<PermittedMortgageDecision> LoadAsync(
        MarkPermittedMortgagePaidOff command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        return PermittedMortgageDecision.LoadAsync(command.GroundLeaseId, session, trustClock, cancellationToken);
    }

    /// <summary>Marks the mortgage paid off.</summary>
    public static void Handle(MarkPermittedMortgagePaidOff command, PermittedMortgageDecision decision)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(decision);

        decision.Lease.Decide(l => l.MarkPermittedMortgagePaidOff(
            command.MortgageId, command.PaidOffOn, decision.Today, command.RecordedBy));
    }
}
