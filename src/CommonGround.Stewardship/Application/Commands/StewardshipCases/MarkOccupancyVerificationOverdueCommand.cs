using CommonGround.Stewardship.Domain.StewardshipCases;

namespace CommonGround.Stewardship.Application.Commands.StewardshipCases;

/// <summary>
/// Command to mark an occupancy verification as overdue for a stewardship case.
/// </summary>
/// <param name="StewardshipCaseId">The ID of the stewardship case.</param>
/// <param name="VerificationId">The ID of the occupancy verification.</param>
/// <param name="Deadline">The deadline that was passed.</param>
public sealed record MarkOccupancyVerificationOverdueCommand(
    StewardshipCaseId StewardshipCaseId,
    Guid VerificationId,
    DateOnly Deadline);
