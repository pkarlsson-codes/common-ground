using CommonGround.SharedKernel.Domain;

namespace DddNamespace;

/// <summary>TODO: Describe the DddEntity entity.</summary>
public sealed class DddEntity : Entity<DddEntityId>
{
    private DddEntity(DddEntityId id)
    {
        Id = id;
    }

    /// <summary>Creates a new DddEntity.</summary>
    public static DddEntity Create()
    {
        return new DddEntity(DddEntityId.Generate());
    }
}
