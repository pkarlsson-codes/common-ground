namespace CommonGround.SharedKernel.Domain;

/// <summary>Represents the unique identifier for a trust entity.</summary>
public class TrustId(string value)
: EntityId<TrustId>(value, Prefix)
{
    private const string Prefix = "trust";
}