using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.Templates;

/// <summary>
/// Unique identifier for a ground lease template.
/// </summary>
/// <param name="value">The string value of the identifier.</param>
public sealed class GroundLeaseTemplateId(string value)
    : EntityId<GroundLeaseTemplateId>(value, Prefix)
{
    private const string Prefix = "groundleasetemplate";
}
