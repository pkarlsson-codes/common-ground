using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Creates a new ground lease.</summary>
public sealed record CreateGroundLease(
    LandParcelId LandParcelId,
    HomeId UnitId,
    DateOnly LeaseStartDate,
    int TermYears,
    string CovenantTerms,
    ResalePolicyTerms ResalePolicyTerms,
    UserId CreatedBy);

/// <summary>Handles <see cref="CreateGroundLease"/>.</summary>
public static class CreateGroundLeaseHandler
{
    /// <summary>Creates a new ground lease and returns its ID.</summary>
    public static Task<GroundLeaseId> Handle(
        CreateGroundLease command,
        IDocumentSession session,
        CancellationToken cancellationToken)
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

        lease.ValidateInvariants();
        session.Events.StartStream<GroundLease>(lease.Id.Value, lease.UncommittedEvents.ToArray());
        lease.UncommittedEvents.MarkAsCommitted();

        return Task.FromResult(lease.Id);
    }
}
