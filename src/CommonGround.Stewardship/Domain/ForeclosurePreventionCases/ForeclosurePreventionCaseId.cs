using CommonGround.SharedKernel.Domain;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

/// <summary>Unique identifier for a foreclosure prevention case.</summary>
/// <param name="value">The string value of the identifier.</param>
public sealed class ForeclosurePreventionCaseId(string value)
    : EntityId<ForeclosurePreventionCaseId>(value, Prefix)
{
    private const string Prefix = "foreclosureprevention";
}
