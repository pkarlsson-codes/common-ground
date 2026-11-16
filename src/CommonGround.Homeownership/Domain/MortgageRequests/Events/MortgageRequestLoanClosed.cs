using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.MortgageRequests.Events;

/// <summary>
/// Raised when an approved loan closes. The lease records it as a permitted mortgage, in the same transaction, and
/// marks the mortgages it paid off.
/// </summary>
/// <param name="ClosedOn">The day the loan closed.</param>
public sealed record MortgageRequestLoanClosed(
    string AggregateId,
    string GroundLeaseId,
    DateOnly ClosedOn,
    string PermittedMortgageId,
    IReadOnlyList<string> PaidOffMortgageIds,
    string RecordedBy) : DomainEvent(AggregateId);
