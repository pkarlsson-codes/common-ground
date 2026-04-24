using CommonGround.SharedKernel.Domain;

namespace CommonGround.Stewardship.Domain.StewardshipCases;

/// <summary>Unique identifier for a stewardship case.</summary>
/// <param name="value">The string value of the identifier.</param>
public sealed class StewardshipCaseId(string value)
    : EntityId<StewardshipCaseId>(value, Prefix)
{
    private const string Prefix = "stewardshipcase";
}
