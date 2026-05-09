using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Creates a new ground lease.</summary>
public sealed record CreateGroundLease(
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
    /// <summary>Creates a new ground lease and returns its ID.</summary>
    public static GroundLeaseId Handle(
        CreateGroundLease command,
        IDocumentSession session)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lease = GroundLease.Create(
            command.LandParcelId,
            command.UnitId,
            command.LeaseStartDate,
            command.TermYears,
            command.CovenantTerms,
            command.ResalePolicyTerms,
            command.CreatedBy);

        session.CreateAggregate<GroundLease, GroundLeaseId>(lease);

        return lease.Id;
    }
}
