using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

/// <summary>Raised when a forbearance request is submitted to a lender.</summary>
/// <param name="AggregateId">The foreclosure prevention case identifier.</param>
/// <param name="RequestId">The new forbearance request identifier.</param>
/// <param name="LenderName">The name of the lender.</param>
/// <param name="RequestedDate">The date the request was submitted.</param>
/// <param name="RequestedBy">The staff member who submitted the request.</param>
public sealed record ForbearanceRequested(
    string AggregateId,
    Guid RequestId,
    string LenderName,
    DateOnly RequestedDate,
    string RequestedBy) : DomainEvent(AggregateId);
