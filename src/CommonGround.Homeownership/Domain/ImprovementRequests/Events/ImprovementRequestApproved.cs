using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.ImprovementRequests.Events;

/// <summary>Raised when the trust approves the work and pre-approves a credit for it.</summary>
/// <param name="AggregateId">The improvement request ID.</param>
/// <param name="GroundLeaseId">The homeowner's lease.</param>
/// <param name="ApprovedOn">The day the trust approved it.</param>
/// <param name="PreApprovedCredit">The most the trust will count for this work.</param>
/// <param name="LeaseImprovementsCap">The lease's improvements cap when approved, or null when it has none.</param>
/// <param name="CreditOnLeaseBefore">The confirmed credit the lease already had when approved.</param>
/// <param name="Notes">Anything staff noted.</param>
/// <param name="ApprovedBy">The staff user who recorded it.</param>
public sealed record ImprovementRequestApproved(
    string AggregateId,
    string GroundLeaseId,
    DateOnly ApprovedOn,
    Money PreApprovedCredit,
    Money? LeaseImprovementsCap,
    Money CreditOnLeaseBefore,
    string? Notes,
    string ApprovedBy) : DomainEvent(AggregateId);
