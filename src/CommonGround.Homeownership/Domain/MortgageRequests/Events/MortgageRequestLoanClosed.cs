using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.MortgageRequests.Events;

/// <summary>
/// Raised when an approved loan closes. The lease records it as a permitted mortgage, in the same transaction, and
/// marks the mortgages it paid off.
/// </summary>
/// <param name="AggregateId">The mortgage request ID.</param>
/// <param name="GroundLeaseId">The homeowner's lease.</param>
/// <param name="ClosedOn">The day the loan closed.</param>
/// <param name="PermittedMortgageId">The permitted mortgage the lease records for the loan.</param>
/// <param name="PaidOffMortgageIds">The lease's permitted mortgages the loan paid off.</param>
/// <param name="RecordedBy">The staff user who recorded it.</param>
public sealed record MortgageRequestLoanClosed(
    string AggregateId,
    string GroundLeaseId,
    DateOnly ClosedOn,
    string PermittedMortgageId,
    IReadOnlyList<string> PaidOffMortgageIds,
    string RecordedBy) : DomainEvent(AggregateId);
