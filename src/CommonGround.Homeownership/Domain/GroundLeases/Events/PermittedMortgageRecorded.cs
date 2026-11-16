using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <param name="NoticeAddress">Where the trust sends copies of notices, or null when not on file.</param>
/// <param name="PermittedOn">The day the trust permitted it.</param>
public sealed record PermittedMortgageRecorded(
    string AggregateId,
    string MortgageId,
    string Lender,
    string? NoticeAddress,
    Money OriginalPrincipal,
    DateOnly PermittedOn,
    MortgagePermissionSource Source,
    string RecordedBy) : DomainEvent(AggregateId);
