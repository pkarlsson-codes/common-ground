using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.Insurance;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Changes the insurance a lease requires from a day on (Art 9.4).</summary>
/// <param name="GroundLeaseId">The lease.</param>
/// <param name="Requirements">The replacement value and liability per occurrence required from that day.</param>
/// <param name="EffectiveFrom">The first day they apply.</param>
/// <param name="SetBy">The staff user setting them.</param>
public sealed record SetInsuranceRequirements(
    GroundLeaseId GroundLeaseId,
    InsuranceRequirements Requirements,
    DateOnly EffectiveFrom,
    UserId SetBy);

/// <summary>Handles <see cref="SetInsuranceRequirements"/>.</summary>
public static class SetInsuranceRequirementsHandler
{
    /// <summary>Sets the lease's insurance requirements.</summary>
    public static InsuranceRequirementsSet? Handle(SetInsuranceRequirements command, [WriteAggregate] GroundLease lease)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(lease);

        return lease.SetInsuranceRequirements(command.Requirements, command.EffectiveFrom, command.SetBy);
    }
}
