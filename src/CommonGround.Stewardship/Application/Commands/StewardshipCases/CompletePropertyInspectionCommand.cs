using CommonGround.SharedKernel.Domain;

namespace CommonGround.Stewardship.Application.Commands.StewardshipCases;

/// <summary>
/// Command to complete a property inspection for a stewardship case.
/// </summary>
/// <param name="StewardshipCaseId">The ID of the stewardship case.</param>
/// <param name="InspectionId">The ID of the inspection.</param>
/// <param name="OccupancyConfirmed">Whether occupancy was confirmed.</param>
/// <param name="ConditionNotes">Optional notes about property condition.</param>
/// <param name="CompletedDate">The date the inspection was completed.</param>
/// <param name="RecordedBy">The user ID of who recorded the completion.</param>
public sealed record CompletePropertyInspectionCommand(
    string StewardshipCaseId,
    Guid InspectionId,
    bool OccupancyConfirmed,
    string? ConditionNotes,
    DateOnly CompletedDate,
    UserId RecordedBy);
