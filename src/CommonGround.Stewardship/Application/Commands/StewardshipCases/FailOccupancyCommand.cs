namespace CommonGround.Stewardship.Application.Commands.StewardshipCases;

/// <summary>
/// Command to fail an occupancy verification for a stewardship case.
/// </summary>
/// <param name="StewardshipCaseId">The ID of the stewardship case.</param>
/// <param name="VerificationId">The ID of the occupancy verification.</param>
/// <param name="Reason">The reason for failing the verification.</param>
/// <param name="FailedDate">The date the verification failed.</param>
/// <param name="RecordedBy">The user ID of who recorded the failure.</param>
public sealed record FailOccupancyCommand(
    string StewardshipCaseId,
    Guid VerificationId,
    string Reason,
    DateOnly FailedDate,
    string RecordedBy);
