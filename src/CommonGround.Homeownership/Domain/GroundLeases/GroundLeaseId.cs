using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases;

/// <summary>
/// Unique identifier for a ground lease.
/// </summary>
/// <param name="value">The string value of the identifier.</param>
public sealed class GroundLeaseId(string value) : EntityId<GroundLeaseId>(value, Prefix)
{
    private const string Prefix = "groundlease";
}