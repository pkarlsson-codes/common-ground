using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.PropertyRegistry.Domain.LeasableUnits;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Creates a new ground lease.</summary>
public sealed record CreateGroundLease(
    LandParcelId LandParcelId,
    LeasableUnitId UnitId,
    DateOnly LeaseStartDate,
    int TermYears,
    string CovenantTerms,
    ResalePolicyTerms ResalePolicyTerms,
    UserId CreatedBy);

/// <summary>Handles <see cref="CreateGroundLease"/>.</summary>
public static class CreateGroundLeaseHandler
{
    /// <summary>Creates a new ground lease and returns its ID.</summary>
    public static async Task<GroundLeaseId> Handle(
        CreateGroundLease command,
        IGroundLeaseRepository repository,
        IUnitOfWork unitOfWork,
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

        repository.Stage(lease);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return lease.Id;
    }
}
