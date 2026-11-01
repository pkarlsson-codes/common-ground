namespace CommonGround.SharedKernel.Domain;

/// <summary>
/// Base class for all aggregate roots in the domain. An aggregate is a decider: its decision methods
/// validate a request against its current state and return the events it decides on (or null when there
/// is nothing to do) without changing state; Marten evolves it by calling its public static <c>Create</c>
/// for the first event and its public <c>Apply</c> methods for the rest.
/// </summary>
/// <typeparam name="TIdentity">The type of the aggregate's unique identifier.</typeparam>
public abstract class AggregateRoot<TIdentity> : Entity<TIdentity>
    where TIdentity : struct, IEntityId<TIdentity>
{
    /// <summary>
    /// Initializes a new instance of the AggregateRoot class.
    /// </summary>
    protected AggregateRoot()
    {
    }
}
