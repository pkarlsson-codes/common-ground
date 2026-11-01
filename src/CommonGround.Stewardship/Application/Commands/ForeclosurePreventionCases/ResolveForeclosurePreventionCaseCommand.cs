using CommonGround.SharedKernel.Domain;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

namespace CommonGround.Stewardship.Application.Commands.ForeclosurePreventionCases;

/// <summary>
/// Command to resolve a foreclosure prevention case.
/// </summary>
/// <param name="ForeclosurePreventionCaseId">The ID of the foreclosure prevention case.</param>
/// <param name="Outcome">The outcome of the case resolution.</param>
/// <param name="ResolvedDate">The date the case was resolved.</param>
/// <param name="ResolvedBy">The user ID of who resolved the case.</param>
public sealed record ResolveForeclosurePreventionCaseCommand(
    ForeclosurePreventionCaseId ForeclosurePreventionCaseId,
    ForeclosurePreventionOutcome Outcome,
    DateOnly ResolvedDate,
    UserId ResolvedBy);
