using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Changes a permitted mortgage's lender and notice address.</summary>
/// <param name="GroundLeaseId">The lease.</param>
/// <param name="MortgageId">The mortgage.</param>
/// <param name="Lender">The lender or servicer now.</param>
/// <param name="NoticeAddress">Where the trust sends copies of notices now, if known.</param>
/// <param name="EditedBy">The staff user changing it.</param>
public sealed record EditPermittedMortgage(
    GroundLeaseId GroundLeaseId,
    PermittedMortgageId MortgageId,
    string Lender,
    string? NoticeAddress,
    UserId EditedBy);

/// <summary>Handles <see cref="EditPermittedMortgage"/>.</summary>
public static class EditPermittedMortgageHandler
{
    /// <summary>Changes the mortgage on the lease.</summary>
    public static PermittedMortgageEdited? Handle(EditPermittedMortgage command, [WriteAggregate] GroundLease lease)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(lease);

        return lease.EditPermittedMortgage(command.MortgageId, command.Lender, command.NoticeAddress, command.EditedBy);
    }
}
