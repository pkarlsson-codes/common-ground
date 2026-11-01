using CommonGround.SharedKernel.Domain;
using DddNamespace.DddPluralFolder.Events;

namespace DddNamespace.DddPluralFolder;

/// <summary>TODO: Describe the DddAggregate aggregate.</summary>
public sealed class DddAggregate : AggregateRoot<DddAggregateId>
{
#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private DddAggregate() { }
#pragma warning restore CS8618

    /// <summary>Gets the ID of the user who created this DddAggregate.</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>
    /// Decides to start a new DddAggregate. TODO: name it after the domain verb (Register, Open, ...);
    /// never <c>Create</c>, which Marten calls to evolve the first event.
    /// </summary>
    public static DddAggregateCreated Register(DddAggregateId id, UserId createdBy)
    {
        return new DddAggregateCreated(id.Value, createdBy.Value);
    }

    /// <summary>Evolves a new DddAggregate from its stream's first event.</summary>
    public static DddAggregate Create(DddAggregateCreated e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var aggregate = new DddAggregate();
        aggregate.Apply(e);
        return aggregate;
    }

    private void Apply(DddAggregateCreated e)
    {
        Id = DddAggregateId.Create(e.AggregateId);
        CreatedBy = UserId.Create(e.CreatedBy);
    }
}
