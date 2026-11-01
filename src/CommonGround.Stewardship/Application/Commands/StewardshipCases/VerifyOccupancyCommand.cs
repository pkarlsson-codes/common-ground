using CommonGround.SharedKernel.Domain;
using CommonGround.Stewardship.Domain.StewardshipCases;

namespace CommonGround.Stewardship.Application.Commands.StewardshipCases;

/// <summary>
/// Command to verify occupancy for a stewardship case.
/// </summary>
/// <param name="StewardshipCaseId">The ID of the stewardship case.</param>
/// <param name="VerificationId">The ID of the occupancy verification.</param>
/// <param name="VerifiedDate">The date the occupancy was verified.</param>
/// <param name="VerifiedBy">The user ID of who verified the occupancy.</param>
public sealed record VerifyOccupancyCommand(
    StewardshipCaseId StewardshipCaseId,
    Guid VerificationId,
    DateOnly VerifiedDate,
    UserId VerifiedBy);
