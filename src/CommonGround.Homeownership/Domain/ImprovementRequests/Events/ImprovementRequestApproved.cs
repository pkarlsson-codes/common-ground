using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.ImprovementRequests.Events;

/// <summary>Raised when the trust approves the work and pre-approves a credit for it.</summary>
/// <param name="ApprovedOn">The day the trust approved it.</param>
/// <param name="LeaseImprovementsCap">The lease's improvements cap when approved, or null when it has none.</param>
public sealed record ImprovementRequestApproved(
    string AggregateId,
    string GroundLeaseId,
    DateOnly ApprovedOn,
    Money PreApprovedCredit,
    Money? LeaseImprovementsCap,
    Money CreditOnLeaseBefore,
    string? Notes,
    string ApprovedBy) : DomainEvent(AggregateId);
