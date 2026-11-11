using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Corrects the day a lease's fees are tracked from and what was owed on it.</summary>
/// <param name="GroundLeaseId">The lease.</param>
/// <param name="FeesTrackedFrom">The first day the trust tracks fees in CommonGround.</param>
/// <param name="OpeningBalance">What the homeowner owed on that day.</param>
/// <param name="CorrectedBy">The staff user correcting it.</param>
public sealed record CorrectLeaseFeeTracking(
    GroundLeaseId GroundLeaseId,
    DateOnly FeesTrackedFrom,
    Money OpeningBalance,
    UserId CorrectedBy);

/// <summary>Handles <see cref="CorrectLeaseFeeTracking"/>.</summary>
public static class CorrectLeaseFeeTrackingHandler
{
    /// <summary>Corrects the lease's fee tracking.</summary>
    public static LeaseFeeTrackingCorrected Handle(CorrectLeaseFeeTracking command, [WriteAggregate] GroundLease lease)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(lease);

        return lease.CorrectLeaseFeeTracking(command.FeesTrackedFrom, command.OpeningBalance, command.CorrectedBy);
    }
}
