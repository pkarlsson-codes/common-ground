using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when a mortgage the trust permitted is added to a lease.</summary>
/// <param name="AggregateId">The ground lease ID.</param>
/// <param name="MortgageId">The mortgage's ID.</param>
/// <param name="Lender">The lender or servicer.</param>
/// <param name="NoticeAddress">Where the trust sends copies of notices, or null when not on file.</param>
/// <param name="OriginalPrincipal">The loan amount when it closed.</param>
/// <param name="PermittedOn">The day the trust permitted it.</param>
/// <param name="Source">How the trust permitted it.</param>
/// <param name="RecordedBy">The staff user who recorded it.</param>
public sealed record PermittedMortgageRecorded(
    string AggregateId,
    string MortgageId,
    string Lender,
    string? NoticeAddress,
    Money OriginalPrincipal,
    DateOnly PermittedOn,
    MortgagePermissionSource Source,
    string RecordedBy) : DomainEvent(AggregateId);
