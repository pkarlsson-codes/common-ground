using CommonGround.SharedKernel.Domain;

namespace CommonGround.Stewardship.Application.Commands.StewardshipCases;

/// <summary>
/// Command to schedule a property inspection for a stewardship case.
/// </summary>
/// <param name="StewardshipCaseId">The ID of the stewardship case.</param>
/// <param name="ScheduledDate">The date the inspection is scheduled.</param>
/// <param name="InspectorName">The name of the inspector.</param>
/// <param name="ScheduledBy">The user ID of who scheduled the inspection.</param>
public sealed record SchedulePropertyInspectionCommand(
    string StewardshipCaseId,
    DateOnly ScheduledDate,
    string InspectorName,
    UserId ScheduledBy);
