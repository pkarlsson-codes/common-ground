using CommonGround.SharedKernel.Domain;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

namespace CommonGround.Stewardship.Application.Commands.ForeclosurePreventionCases;

/// <summary>Opens a foreclosure prevention case for a lease whose notice of default is unpaid.</summary>
/// <param name="ForeclosurePreventionCaseId">The new case's ID, chosen by the caller.</param>
/// <param name="GroundLeaseId">The lease the case is for.</param>
/// <param name="UnpaidDefault">The lease's notice of default as read from its fee ledger, or null when it has no unpaid one.</param>
/// <param name="OpenedDate">The trust's today.</param>
/// <param name="OpenedBy">The staff member opening the case.</param>
public sealed record OpenForeclosurePreventionCaseForLeaseDefaultCommand(
    ForeclosurePreventionCaseId ForeclosurePreventionCaseId,
    string GroundLeaseId,
    LeaseDefaultNotice? UnpaidDefault,
    DateOnly OpenedDate,
    UserId OpenedBy);
