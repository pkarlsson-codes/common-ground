using CommonGround.SharedKernel.Domain;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

namespace CommonGround.Stewardship.Application.Commands.ForeclosurePreventionCases;

/// <summary>
/// Command to record the outcome of a forbearance request for a foreclosure prevention case.
/// </summary>
/// <param name="ForeclosurePreventionCaseId">The ID of the foreclosure prevention case.</param>
/// <param name="RequestId">The ID of the forbearance request.</param>
/// <param name="Outcome">The outcome of the forbearance request.</param>
/// <param name="OutcomeDate">The date the outcome occurred.</param>
/// <param name="Notes">Optional notes about the outcome.</param>
/// <param name="RecordedBy">The user ID of who recorded the outcome.</param>
public sealed record RecordForbearanceOutcomeCommand(
    string ForeclosurePreventionCaseId,
    Guid RequestId,
    ForbearanceOutcome Outcome,
    DateOnly OutcomeDate,
    string? Notes,
    UserId RecordedBy);
