using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when staff change a permitted mortgage's lender or notice address, such as when servicing moves.</summary>
/// <param name="AggregateId">The ground lease ID.</param>
/// <param name="MortgageId">The mortgage's ID.</param>
/// <param name="Lender">The lender or servicer now.</param>
/// <param name="NoticeAddress">Where the trust sends copies of notices now, or null when not on file.</param>
/// <param name="EditedBy">The staff user who changed it.</param>
public sealed record PermittedMortgageEdited(
    string AggregateId,
    string MortgageId,
    string Lender,
    string? NoticeAddress,
    string EditedBy) : DomainEvent(AggregateId);
