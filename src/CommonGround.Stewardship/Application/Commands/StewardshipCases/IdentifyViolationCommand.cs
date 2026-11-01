using CommonGround.SharedKernel.Domain;
using CommonGround.Stewardship.Domain.StewardshipCases;

namespace CommonGround.Stewardship.Application.Commands.StewardshipCases;

/// <summary>
/// Command to identify a violation for a stewardship case.
/// </summary>
/// <param name="StewardshipCaseId">The ID of the stewardship case.</param>
/// <param name="Type">The type of lease violation.</param>
/// <param name="Description">The description of the violation.</param>
/// <param name="IdentifiedDate">The date the violation was identified.</param>
/// <param name="IdentifiedBy">The user ID of who identified the violation.</param>
public sealed record IdentifyViolationCommand(
    StewardshipCaseId StewardshipCaseId,
    LeaseViolationType Type,
    string Description,
    DateOnly IdentifiedDate,
    UserId IdentifiedBy);
