namespace CommonGround.Stewardship.Application.Commands.StewardshipCases;

/// <summary>
/// Command to cancel a property inspection for a stewardship case.
/// </summary>
/// <param name="StewardshipCaseId">The ID of the stewardship case.</param>
/// <param name="InspectionId">The ID of the inspection to cancel.</param>
/// <param name="Reason">The reason for cancellation.</param>
/// <param name="CancelledBy">The user ID of who cancelled the inspection.</param>
public sealed record CancelPropertyInspectionCommand(
    string StewardshipCaseId,
    Guid InspectionId,
    string Reason,
    string CancelledBy);
