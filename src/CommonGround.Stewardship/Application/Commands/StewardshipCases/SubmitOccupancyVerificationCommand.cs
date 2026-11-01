using CommonGround.Stewardship.Domain.StewardshipCases;

namespace CommonGround.Stewardship.Application.Commands.StewardshipCases;

/// <summary>
/// Command to submit occupancy verification for a stewardship case.
/// </summary>
/// <param name="StewardshipCaseId">The ID of the stewardship case.</param>
/// <param name="VerificationId">The ID of the occupancy verification.</param>
/// <param name="IsPrimaryResident">Whether the respondent is the primary resident.</param>
/// <param name="SubletDisclosed">Whether any sublets were disclosed.</param>
/// <param name="MajorChanges">Optional notes about major changes to the property.</param>
/// <param name="SubmittedDate">The date the verification was submitted.</param>
public sealed record SubmitOccupancyVerificationCommand(
    StewardshipCaseId StewardshipCaseId,
    Guid VerificationId,
    bool IsPrimaryResident,
    bool SubletDisclosed,
    string? MajorChanges,
    DateOnly SubmittedDate);
