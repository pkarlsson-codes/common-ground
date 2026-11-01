using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Creates a new ground lease.</summary>
public sealed record CreateGroundLease(
    GroundLeaseId GroundLeaseId,
    string LandParcelId,
    string UnitId,
    DateOnly LeaseStartDate,
    int TermYears,
    string CovenantTerms,
    ResalePolicyTerms ResalePolicyTerms,
    UserId CreatedBy);

/// <summary>Handles <see cref="CreateGroundLease"/>.</summary>
public static class CreateGroundLeaseHandler
{
    /// <summary>Starts the new stream.</summary>
    public static IStartStream Handle(CreateGroundLease command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var started = GroundLease.Establish(
            command.GroundLeaseId,
            command.LandParcelId,
            command.UnitId,
            command.LeaseStartDate,
            command.TermYears,
            command.CovenantTerms,
            command.ResalePolicyTerms,
            command.CreatedBy);

        return MartenOps.StartStream<GroundLease>(command.GroundLeaseId.Value, started);
    }
}
